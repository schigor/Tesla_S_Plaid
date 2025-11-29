using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // INPUT
        float moveLeft = Keyboard.current.aKey.isPressed ? -1f : 0f;
        float moveRight = Keyboard.current.dKey.isPressed ? 1f : 0f;
        
        // Jeśli jesteśmy w wall jump lock, nie zmieniamy kierunku
        if (wallJumpLockTimer <= 0)
        {
            horizontalInput = moveLeft + moveRight;
        }
        else
        {
            wallJumpLockTimer -= Time.deltaTime;
        }

        // JUMP REQUEST
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        // OBRÓT
        if (horizontalInput > 0 && !isFacingRight) Flip();
        if (horizontalInput < 0 && isFacingRight) Flip();
    }

    private void FixedUpdate()
    {
        // WYKRYWANIE KOLIZJI
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, groundCheckRadius, wallLayer);

        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.CurrentTimeMultiplier : 1.0f;

        // LOGIKA WALL SLIDE
        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y <= 0;

        // PRĘDKOŚĆ
        if (isWallSliding)
        {
            // Wall slide: powolne opadanie
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * timeMod, -wallSlideSpeed * timeMod);
        }
        else if (isGrounded)
        {
            // Zwykły ruch na ziemi
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * timeMod, rb.linearVelocity.y);
        }
        else
        {
            // Powietrze - zachowaj poprzednią prędkość X jeśli nie ma inputu
            if (horizontalInput != 0)
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * timeMod, rb.linearVelocity.y);
            }
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
        // Kierunek odbicia: przeciwny do kierunku w którym patrzymy
        float jumpDirX = isFacingRight ? -wallJumpHorizontal : wallJumpHorizontal;
        float jumpDirY = wallJumpVertical;

        // Resetuj prędkość i aplikuj impuls
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(jumpDirX, jumpDirY), ForceMode2D.Impulse);

        // Natychmiastowy obrót w kierunku skoku
        if (jumpDirX > 0 && !isFacingRight) Flip();
        if (jumpDirX < 0 && isFacingRight) Flip();

        // Zablokuj input na moment
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