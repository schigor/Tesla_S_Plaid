using UnityEngine;
using UnityEngine.InputSystem;

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

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpRequested;
    
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallJumpLockTimer;
    private float currentVelocityX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

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

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        if (horizontalInput > 0 && !isFacingRight) Flip();
        if (horizontalInput < 0 && isFacingRight) Flip();
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, groundCheckRadius, wallLayer);

        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.CurrentTimeMultiplier : 1.0f;

        // Sprawdź czy właśnie dotknęliśmy ściany
        bool wasWallSliding = isWallSliding;
        isWallSliding = isTouchingWall && !isGrounded;

        // GŁADKA AKCELERACJA
        float targetVelocity = 0f;
        
        if (isWallSliding)
        {
            currentVelocityX = 0f;
            
            // Jeśli właśnie dotknęliśmy ściany, resetuj Y
            if (!wasWallSliding && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(0f, 0f);
            }
            
            // Teraz aplij wall slide
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed * timeMod);
        }
        else
        {
            targetVelocity = horizontalInput * moveSpeed * timeMod;
            
            if (Mathf.Abs(targetVelocity) > 0.01f)
            {
                currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
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
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * timeMod);
            }
            jumpRequested = false;
        }
    }

    private void PerformWallJump()
    {
        float jumpDirX = isFacingRight ? -wallJumpHorizontal : wallJumpHorizontal;
        float jumpDirY = wallJumpVertical;

        rb.linearVelocity = Vector2.zero;
        currentVelocityX = 0f;
        rb.AddForce(new Vector2(jumpDirX, jumpDirY), ForceMode2D.Impulse);

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