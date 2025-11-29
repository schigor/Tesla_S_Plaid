using UnityEngine;
using UnityEngine.InputSystem; // 1. To jest kluczowe dla nowego systemu!

public class PlayerMovement : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Wykrywanie Ziemi")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Sprawdzenie czy klawiatura jest podłączona (dobre praktyki)
        if (Keyboard.current == null) return;

        // 2. Pobieranie wejścia (A i D) w Nowym Systemie
        // Zamiast Input.GetAxisRaw używamy Keyboard.current
        float moveLeft = Keyboard.current.aKey.isPressed ? -1f : 0f;
        float moveRight = Keyboard.current.dKey.isPressed ? 1f : 0f;
        
        // Sumujemy: jeśli wciśniesz A (-1) i D (1) naraz, postać stanie (0)
        horizontalInput = moveLeft + moveRight;

        // 3. Skakanie pod klawiszem 'W' (lub Spacją)
        // wasPressedThisFrame to odpowiednik GetButtonDown
        bool jumpPressed = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. Obracanie postaci
        FlipSprite();
    }

    private void FixedUpdate()
    {
        // Fizyka ruchu
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Wykrywanie ziemi
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
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
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}