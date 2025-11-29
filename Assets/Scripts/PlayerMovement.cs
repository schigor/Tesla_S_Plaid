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
    [SerializeField] private float wallJumpHorizontal = 10f;
    [SerializeField] private float wallJumpVertical = 16f;
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
    private float currentVelocityX;
    
    // 0 = Slow, 1 = Normal, 2 = Fast
    private int currentTimeState = 1;
    private float targetTimeMultiplier = 1.0f;

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

        // STEROWANIE CZASEM - Strzałki lewo/prawo
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

        // Gładka zmiana czasu
        float currentTimeMultiplier = Mathf.Lerp(
            GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f,
            targetTimeMultiplier,
            timeTransitionSpeed * Time.deltaTime
        );
        
        if (GlobalTimeManager.Instance != null)
            GlobalTimeManager.Instance.gameTimeMultiplier = currentTimeMultiplier;

        if (timeSpeedText != null)
            timeSpeedText.text = $"Speed: {currentTimeMultiplier:F2}x";

        if (isGrounded) 
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            if (horizontalInput < 0 && isFacingRight) Flip();
        }
    }

    // Wykrycie wejścia na platformę
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // Ustawiamy platformę jako rodzica gracza
            transform.SetParent(collision.transform);
        }
    }

    // Wykrycie zejścia z platformy
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // Usuwamy rodzica (gracz wraca do "świata")
            transform.SetParent(null);
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, groundCheckRadius, wallLayer);

        // PLAYER ZAWSZE MA NORMALNY CZAS - BRAK MNOŻNIKA
        bool wasWallSliding = isWallSliding;
        isWallSliding = isTouchingWall && !isGrounded;

        if (isWallSliding)
        {
            currentVelocityX = 0f;
            if (!wasWallSliding && rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(0f, 0f);
            
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
        }
        else
        {
            if (isGrounded)
            {
                float targetVelocity = horizontalInput * moveSpeed;
                
                if (Mathf.Abs(targetVelocity) > 0.01f)
                    currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocity, acceleration * Time.fixedDeltaTime);
                else
                    currentVelocityX = Mathf.Lerp(currentVelocityX, 0f, friction * Time.fixedDeltaTime);
            }

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
        float jumpDirX = isFacingRight ? -wallJumpHorizontal : wallJumpHorizontal;
        float jumpDirY = wallJumpVertical;

        currentVelocityX = jumpDirX;
        rb.linearVelocity = new Vector2(jumpDirX, jumpDirY);

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