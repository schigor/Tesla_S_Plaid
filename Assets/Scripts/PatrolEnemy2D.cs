using UnityEngine;

public class PatrolEnemy2D : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    public float speed = 3f;             // Prędkość poruszania
    public bool startFacingRight = true; // Czy na początku patrzy w prawo?

    [Header("Wykrywanie Ścian")]
    public Transform wallCheck;          // Pusty obiekt przed "twarzą" przeciwnika
    public float wallCheckDistance = 0.5f; // Jak daleko widzi ścianę
    public LayerMask wallLayer;          // Co jest uznawane za ścianę (Warstwa)

    private Rigidbody2D rb;
    private bool facingRight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        facingRight = startFacingRight;

        // Jeśli ustawiliśmy, że nie patrzy w prawo, obróć go na starcie
        if (!facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void Update()
    {
        // Tutaj teraz tylko sprawdzamy, czy widzimy ścianę
        CheckForWall();
    }

    void FixedUpdate()
    {
        // Pobieramy modyfikator czasu z Twojego Managera (jeśli istnieje)
        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.CurrentTimeMultiplier : 1.0f;

        // Ustal kierunek (1 lub -1)
        float direction = facingRight ? 1 : -1;

        // Ruch (używam linearVelocity tak jak w Twoim kodzie - to dla Unity 6. 
        // W starszych wersjach użyj rb.velocity)
        rb.linearVelocity = new Vector2(direction * speed * timeMod, rb.linearVelocity.y);
    }

    void CheckForWall()
    {
        if (wallCheck == null) return;

        // Ustal kierunek sprawdzania (prawo lub lewo)
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        // Wystrzel promień (Raycast) z punktu wallCheck
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, wallLayer);

        // Jeśli promień w coś trafił (hit.collider nie jest pusty) -> ZAWRACAJ
        if (hit.collider != null)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Rysuje linię w edytorze, żebyś widział zasięg wzroku przeciwnika
    void OnDrawGizmos()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            // Rysujemy linię od punktu sprawdzania w stronę, w którą patrzy
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)(direction * wallCheckDistance));
        }
    }
}