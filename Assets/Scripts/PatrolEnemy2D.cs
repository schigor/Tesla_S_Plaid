using UnityEngine;

public class PatrolEnemy2D: MonoBehaviour
{
    [Header("Ustawienia")]
    public float speed = 3f;            // Prędkość poruszania
    public float patrolDuration = 2f;   // Ile czasu (w sekundach) idzie w jedną stronę
    public bool startFacingRight = true; // Czy na początku patrzy w prawo?

    private Rigidbody2D rb;
    private float timer;
    private bool facingRight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = patrolDuration; // Ustawiamy licznik na start
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

        // Odliczanie czasu w dół
        timer -= Time.deltaTime;

        // Jeśli czas się skończył
        if (timer <= 0)
        {
            Flip();                // Obróć przeciwnika
            timer = patrolDuration; // Zresetuj licznik czasu
        }
    }

    void FixedUpdate()
    {

        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.CurrentTimeMultiplier : 1.0f;
        // Ruch w aktualnym kierunku
        // Jeśli facingRight to 1, jeśli nie to -1
        float direction = facingRight ? 1 : -1;

        // Ustawiamy prędkość X, zachowujemy Y (grawitację)
        rb.linearVelocity = new Vector2(direction * speed*timeMod, rb.linearVelocity.y);
    }

    // Funkcja do obracania (taka sama jak w poprzednich skryptach)
    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}