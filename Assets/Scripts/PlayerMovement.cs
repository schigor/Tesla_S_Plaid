using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement; // 1. WAŻNE: Dodane do zmiany scen

[RequireComponent(typeof(SpriteRenderer))]
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

    [Header("Ladder System")]
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private float climbSpeed = 5f;
    
    private bool isTouchingLadder;
    private bool isClimbing;
    private float verticalInput;
    private float originalGravityScale;
    
    [Header("Sterowanie Czasem")]
    [SerializeField] private float timeMultiplierSlow = 0.25f;
    [SerializeField] private float timeMultiplierNormal = 1.0f;
    [SerializeField] private float timeMultiplierFast = 2.0f;
    [SerializeField] private float timeTransitionSpeed = 5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeSpeedText;

    [Header("Animacje (Sprite'y)")]
    [SerializeField] private Sprite idleSprite;      
    [SerializeField] private Sprite runSprite;       
    [SerializeField] private Sprite jumpSprite;      
    [SerializeField] private Sprite wallSlideSprite; 

    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpRequested;

    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallJumpLockTimer;
    private float currentVelocityX;
    
    private int currentTimeState = 1;
    private float targetTimeMultiplier = 1.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // RUCH POZIOMY
        float moveLeft = Keyboard.current.aKey.isPressed ? -1f : 0f;
        float moveRight = Keyboard.current.dKey.isPressed ? 1f : 0f;
        
        if (wallJumpLockTimer <= 0) horizontalInput = moveLeft + moveRight;
        else wallJumpLockTimer -= Time.deltaTime;

        // RUCH PIONOWY
        float moveUp = Keyboard.current.wKey.isPressed ? 1f : 0f;
        float moveDown = Keyboard.current.sKey.isPressed ? -1f : 0f;
        verticalInput = moveUp + moveDown;

        // SKOK
        bool jumpKey = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
        
        if (isClimbing)
        {
             if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpRequested = true;
        }
        else if (jumpKey)
        {
            jumpRequested = true;
        }

        // CZAS
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) currentTimeState = Mathf.Max(currentTimeState - 1, 0);
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) currentTimeState = Mathf.Min(currentTimeState + 1, 2);

        targetTimeMultiplier = currentTimeState switch { 0 => timeMultiplierSlow, 1 => timeMultiplierNormal, 2 => timeMultiplierFast, _ => timeMultiplierNormal };
        
        float currentTimeMultiplier = Mathf.Lerp(GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f, targetTimeMultiplier, timeTransitionSpeed * Time.deltaTime);
        if (GlobalTimeManager.Instance != null) GlobalTimeManager.Instance.gameTimeMultiplier = currentTimeMultiplier;
        if (timeSpeedText != null) timeSpeedText.text = $"Speed: {currentTimeMultiplier:F2}x";

        if (isGrounded) 
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            if (horizontalInput < 0 && isFacingRight) Flip();
        }

        HandleAnimations();
    }

    private void HandleAnimations()
    {
        if (spriteRenderer == null) return;

        if (isWallSliding)
        {
            if (wallSlideSprite != null) spriteRenderer.sprite = wallSlideSprite;
            else if (jumpSprite != null) spriteRenderer.sprite = jumpSprite;
        }
        else if (!isGrounded && !isClimbing)
        {
            if (jumpSprite != null) spriteRenderer.sprite = jumpSprite;
        }
        else if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            if (runSprite != null) spriteRenderer.sprite = runSprite;
        }
        else
        {
            if (idleSprite != null) spriteRenderer.sprite = idleSprite;
        }
    }

    // --- NOWOŚĆ: OBSŁUGA FINISHU (PRZEJŚCIE DO NOWEJ SCENY) ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Finish"))
        {
            // 1. Resetujemy czas do normalnej prędkości (krytyczne!)
            Time.timeScale = 1f;
            if (GlobalTimeManager.Instance != null) GlobalTimeManager.Instance.gameTimeMultiplier = 1.0f;

            // 2. Ładujemy następną scenę
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }

            else if (currentSceneIndex == 4)
            {
                Debug.Log("Koniec Gry! Gratulacje");
            }
            else
            {
                Debug.Log("Koniec Gry! Wracam do menu (Scena 0).");
                SceneManager.LoadScene(0);
            }
        }
    }
    // -----------------------------------------------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform")) transform.SetParent(collision.transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform")) transform.SetParent(null);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, groundCheckRadius, wallLayer);

        isTouchingLadder = rb.IsTouchingLayers(ladderLayer);
        
        CheckClimbingState();

        if (isClimbing)
        {
            HandleClimbingMovement();
            
            if (jumpRequested)
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, jumpForce);
                isClimbing = false;
                rb.gravityScale = originalGravityScale;
                jumpRequested = false;
            }
            return; 
        }

        bool wasWallSliding = isWallSliding;
        isWallSliding = isTouchingWall && !isGrounded && !isClimbing;

        if (isWallSliding)
        {
            currentVelocityX = 0f;
            if (!wasWallSliding && rb.linearVelocity.y > 0) rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
        }
        else
        {
            if (isGrounded)
            {
                float targetVelocity = horizontalInput * moveSpeed;
                if (Mathf.Abs(targetVelocity) > 0.01f) currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocity, acceleration * Time.fixedDeltaTime);
                else currentVelocityX = Mathf.Lerp(currentVelocityX, 0f, friction * Time.fixedDeltaTime);
            }
            rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);
        }

        if (jumpRequested)
        {
            if (isWallSliding) PerformWallJump();
            else if (isGrounded) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    private void CheckClimbingState()
    {
        if (isTouchingLadder && Mathf.Abs(verticalInput) > 0.01f)
        {
            isClimbing = true;
            rb.gravityScale = 0f; 
        }
        else if (!isTouchingLadder && isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = originalGravityScale;
        }
    }

    private void HandleClimbingMovement()
    {
        float yVelocity = verticalInput * climbSpeed;
        float xVelocity = horizontalInput * moveSpeed; 
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
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