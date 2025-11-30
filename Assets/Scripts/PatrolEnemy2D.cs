using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // Wymusza posiadanie SpriteRenderer
public class PatrolEnemy2D : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    public float speed = 3f;             // Prędkość poruszania
    public bool startFacingRight = true; // Czy na początku patrzy w prawo?

    [Header("Wykrywanie Ścian")]
    public Transform wallCheck;          // Pusty obiekt przed "twarzą" przeciwnika
    public float wallCheckDistance = 0.5f; // Jak daleko widzi ścianę
    public LayerMask wallLayer;          // Co jest uznawane za ścianę (Warstwa)

    // --- NOWE: Zmienne do animacji ---
    [Header("Animacja")]
    public Sprite frame1;                // Pierwsza klatka chodu
    public Sprite frame2;                // Druga klatka chodu
    public float animationSpeed = 0.2f;  // Co ile sekund zmiana klatki (przy normalnym czasie)

    private SpriteRenderer spriteRenderer;
    private float animTimer;
    // ----------------------------------

    private Rigidbody2D rb;
    private bool facingRight;

    private float timeModifier = 1.0f;
    private Transform player;
    private float range;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Pobieramy renderer
        facingRight = startFacingRight;

        // Ustaw domyślny sprite na start
        if (frame1 != null && spriteRenderer != null)
            spriteRenderer.sprite = frame1;

        // Jeśli zapomnisz przypisać gracza w inspektorze, spróbuj go znaleźć po Tagu
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

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
        CheckForWall();

        // Wywołanie animacji w Update (dla płynności wizualnej)
        HandleAnimation();
    }

    void FixedUpdate()
    {
        // Domyślnie czas płynie normalnie
        timeModifier = 1.0f;

        if (GlobalTimeManager.Instance != null)
        {
            player = GlobalTimeManager.Instance.player;
            range = GlobalTimeManager.Instance.range;

            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.position);

                // JEŚLI gracz jest w zasięgu -> pobieramy "zepsuty" czas
                if (distance <= range)
                {
                    timeModifier = GlobalTimeManager.Instance.gameTimeMultiplier;
                }
            }
        }

        // 3. Ustal kierunek
        float direction = facingRight ? 1 : -1;

        // 4. Aplikujemy ruch z wyliczonym wyżej modyfikatorem
        rb.linearVelocity = new Vector2(direction * speed * timeModifier, rb.linearVelocity.y);
    }

    // --- NOWE: Logika animacji ---
    void HandleAnimation()
    {
        // Jeśli nie mamy przypisanych klatek, nie robimy nic
        if (frame1 == null || frame2 == null) return;

        // Zwiększamy licznik czasu, mnożąc przez timeModifier
        // Dzięki temu, gdy czas gry zwalnia, animacja też zwalnia!
        animTimer += Time.deltaTime * timeModifier;

        if (animTimer >= animationSpeed)
        {
            animTimer = 0f;

            // Proste przełączanie: jeśli obecny to frame1, zmień na frame2 i odwrotnie
            if (spriteRenderer.sprite == frame1)
                spriteRenderer.sprite = frame2;
            else
                spriteRenderer.sprite = frame1;
        }
    }
    // -----------------------------

    void CheckForWall()
    {
        if (wallCheck == null) return;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, wallLayer);

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

    void OnDrawGizmos()
    {
        // Rysowanie wzroku (czerwona linia)
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)(direction * wallCheckDistance));
        }

        // Rysowanie zasięgu wpływu czasu (żółte kółko)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}