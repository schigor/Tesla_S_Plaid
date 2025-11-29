using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float friction = 20f;

    [Header("Wykrywanie Ziemi")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Slide System")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpHorizontal = 10f; // Siła pozioma Wall Jumpa
    [SerializeField] private float wallJumpVertical = 16f;   // Siła pionowa Wall Jumpa
    [SerializeField] private float wallJumpInputLockTime = 0.2f;

    [Header("Sterowanie Czasem")]
    [SerializeField] private float timeMultiplierSlow = 0.25f;
    [SerializeField] private float timeMultiplierNormal = 1.0f;
    [SerializeField] private float timeMultiplierFast = 2.0f;
    [SerializeField] private float timeTransitionSpeed = 5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeSpeedText;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpRequested;
    
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallJumpLockTimer;
    private float currentVelocityX; // To jest nasza zapamiętana prędkość
    
    // 0 = Slow, 1 = Normal, 2 = Fast
    private int currentTimeState = 1;
    private float targetTimeMultiplier = 1.0f;
    private float currentTimeMultiplier = 1.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // RUCH - WASD
        float moveLeft = Keyboard.current.aKey.isPressed ? -1f : 0f;
        float moveRight = Keyboard.current.dKey.isPressed ? 1f : 0f;
        
        if (wallJumpLockTimer <= 0)
        {
            horizontalInput = moveLeft + moveRight;
        }
        else
        {
            wallJumpLockTimer -= Time.deltaTime;
        }

        // SKOK - W lub Space
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        // STEROWANIE CZASEM
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            currentTimeState = Mathf.Max(currentTimeState - 1, 0);
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            currentTimeState = Mathf.Min(currentTimeState + 1, 2);

        targetTimeMultiplier = currentTimeState switch
        {
            0 => timeMultiplierSlow,
            1 => timeMultiplierNormal,
            2 => timeMultiplierFast,
            _ => timeMultiplierNormal
        };

        currentTimeMultiplier = Mathf.Lerp(currentTimeMultiplier, targetTimeMultiplier, timeTransitionSpeed * Time.deltaTime);
        
        if (GlobalTimeManager.Instance != null)
            GlobalTimeManager.Instance.baseTimeSpeed = currentTimeMultiplier;

        if (timeSpeedText != null)
            timeSpeedText.text = $"Speed: {currentTimeMultiplier:F2}x";

        // Flipowanie tylko na ziemi (opcjonalne, jeśli chcesz, by gracz nie mógł się też obracać w locie)
        // Jeśli chcesz, by mógł się obracać wizualnie, ale nie zmieniać kierunku lotu, usuń warunek 'isGrounded'
        if (isGrounded) 
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            if (horizontalInput < 0 && isFacingRight) Flip();
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, groundCheckRadius, wallLayer);

        float timeMod = currentTimeMultiplier;
        bool wasWallSliding = isWallSliding;
        isWallSliding = isTouchingWall && !isGrounded;

        if (isWallSliding)
        {
            currentVelocityX = 0f;
            if (!wasWallSliding && rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(0f, 0f);
            
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed * timeMod);
        }
        else
        {
            // --- ZMIANA: BLOKADA STEROWANIA W LOCIE ---
            
            // Obliczamy nową prędkość TYLKO, gdy stoimy na ziemi.
            // Gdy jesteśmy w powietrzu, 'currentVelocityX' zostaje bez zmian (czyli takie, jak przy wybiciu).
            if (isGrounded)
            {
                float targetVelocity = horizontalInput * moveSpeed * timeMod;
                
                if (Mathf.Abs(targetVelocity) > 0.01f)
                    currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocity, acceleration * Time.fixedDeltaTime);
                else
                    currentVelocityX = Mathf.Lerp(currentVelocityX, 0f, friction * Time.fixedDeltaTime);
            }
            // ELSE: Jesteśmy w powietrzu -> nie ruszamy 'currentVelocityX', zachowuje pęd z momentu skoku.

            rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);
        }

        // SKOKI
        if (jumpRequested)
        {
            if (isWallSliding)
            {
                PerformWallJump();
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            jumpRequested = false;
        }
    }

    private void PerformWallJump()
    {
        // Obliczamy kierunek
        float jumpDirX = isFacingRight ? -wallJumpHorizontal : wallJumpHorizontal;
        float jumpDirY = wallJumpVertical;

        // --- ZMIANA: Ustawiamy prędkość bezpośrednio ---
        // Ponieważ w FixedUpdate blokujemy zmianę prędkości w locie, musimy tutaj
        // "narzucić" nową prędkość (currentVelocityX), którą system będzie potem utrzymywał.
        
        currentVelocityX = jumpDirX; // Ustawiamy prędkość poziomą "na sztywno"
        rb.linearVelocity = new Vector2(jumpDirX, jumpDirY); // Aplikujemy natychmiast

        // Obrót postaci w stronę skoku
        if (jumpDirX > 0 && !isFacingRight) Flip();
        if (jumpDirX < 0 && isFacingRight) Flip();

        wallJumpLockTimer = wallJumpInputLockTime;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.color = Color.blue;
        if (wallCheck != null) Gizmos.DrawWireSphere(wallCheck.position, groundCheckRadius);
    }
}