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

    private float timeModifier = 1.0f;
    private Transform player;
    private float range;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        facingRight = startFacingRight;

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
    }

    void FixedUpdate()
    {


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
        // 2. Sprawdzamy odległość do gracza (jeśli gracz istnieje)
       

        // 3. Ustal kierunek
        float direction = facingRight ? 1 : -1;

        // 4. Aplikujemy ruch z wyliczonym wyżej modyfikatorem
        // Jeśli jest daleko, timeMod to 1. Jeśli blisko, timeMod to np. 0.5 lub 2.
        rb.linearVelocity = new Vector2(direction * speed * timeModifier, rb.linearVelocity.y);
    }

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