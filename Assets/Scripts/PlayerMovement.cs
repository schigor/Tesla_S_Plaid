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

    [Header("Wall Jump System")]
    [SerializeField] private Transform wallCheck;      // Punkt z przodu gracza
    [SerializeField] private LayerMask wallLayer;      // Warstwa ścian
    [SerializeField] private float wallSlidingSpeed = 2f; // Jak wolno zjeżdżamy
    [SerializeField] private Vector2 wallJumpPower = new Vector2(8f, 16f); // Siła odbicia (X, Y)
    [SerializeField] private float wallJumpDuration = 0.2f; // Czas blokady sterowania po odbiciu

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    // Zmienne do Wall Jumpa
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 1. INPUT (Blokujemy input, jeśli właśnie wykonujemy Wall Jump)
        if (!isWallJumping)
        {
            float moveLeft = Keyboard.current.aKey.isPressed ? -1f : 0f;
            float moveRight = Keyboard.current.dKey.isPressed ? 1f : 0f;
            horizontalInput = moveLeft + moveRight;
        }

        // 2. SKOK (Zwykły + Wall Jump)
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isWallSliding)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        // 3. Obsługa ślizgania po ścianie
        CheckWallSlide();

        // 4. Obracanie postaci (Tylko jeśli nie robimy Wall Jumpa)
        if (!isWallJumping)
        {
            FlipSprite();
        }
    }

    private void FixedUpdate()
    {
        // Wykrywanie kolizji
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Wykrywamy ścianę (trochę przed graczem)
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, groundCheckRadius, wallLayer);

        // Standardowy ruch (tylko jeśli nie jesteśmy w trakcie sekwencji Wall Jumpa)
        if (!isWallJumping)
        {
            // Jeśli ślizgamy się, to prędkość Y jest stała (powolna), jeśli nie - normalna fizyka
            if (isWallSliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
            }
            else
            {
                // Tu dodajemy wsparcie dla Twojego TimeManagera (jeśli istnieje)
                float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.CurrentTimeMultiplier : 1.0f;
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * timeMod, rb.linearVelocity.y);
            }
        }
    }

    private void CheckWallSlide()
    {
        // Ślizgamy się tylko jeśli: dotykamy ściany, NIE jesteśmy na ziemi i input jest w stronę ściany
        if (isTouchingWall && !isGrounded && horizontalInput != 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        isWallSliding = false; // Przerywamy ślizg
        StartCoroutine(StopMove()); // Blokujemy input na chwilę

        // Obliczamy kierunek odbicia (przeciwny do tego, gdzie patrzymy)
        float jumpDirection = -horizontalInput; 
        
        // Aplikujemy siłę (W bok i w górę)
        rb.linearVelocity = new Vector2(jumpDirection * wallJumpPower.x, wallJumpPower.y);

        // Obracamy postać od razu, żeby wyglądało to naturalnie
        if (transform.localScale.x != jumpDirection)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    // Korutyna blokująca sterowanie na ułamek sekundy
    // To KLUCZOWE dla "game feel". Bez tego gracz natychmiast anulowałby skok wciskając klawisz w stronę ściany.
    private IEnumerator StopMove()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    private void FlipSprite()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        
        Gizmos.color = Color.blue; // Niebieski dla ściany
        if (wallCheck != null) Gizmos.DrawWireSphere(wallCheck.position, groundCheckRadius);
    }
}