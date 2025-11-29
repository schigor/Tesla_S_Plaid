using UnityEngine;

public class TimeGrowingMushroom : MonoBehaviour
{
    [Header("Ustawienia Wzrostu")]
    [Tooltip("Ile razy wy¿szy ma byæ grzyb (np. 2.0 = dwa razy wy¿szy)")]
    public float heightMultiplier = 2.5f;
    [Tooltip("Jak szybko roœnie")]
    public float growthSpeed = 2.0f;
    [Tooltip("Czy ma maleæ, gdy czas wraca do normy?")]
    public bool shrinkBack = true;

    [Header("Aktywacja")]
    public float activationRange = 5.0f;
    public Transform player;

    private Vector3 initialScale;
    private float targetHeight; // Celujemy w konkretn¹ wysokoœæ, nie ca³y wektor
    private bool isGrown = false;

    void Start()
    {
        initialScale = transform.localScale;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
        float distance = Vector2.Distance(transform.position, player.position);

        // Warunek wzrostu
        bool shouldGrow = (distance <= activationRange) && (timeMod > 1.1f);

        if (!isGrown)
        {
            if (shouldGrow)
            {
                // Cel: Pocz¹tkowa wysokoœæ * mno¿nik
                targetHeight = initialScale.y * heightMultiplier;
                isGrown = true;
            }
            else
            {
                if (shrinkBack)
                    targetHeight = initialScale.y;
                else
                    targetHeight = transform.localScale.y; // Zostaje tak jak jest
            }
        }
      

        // P³ynna zmiana TYLKO osi Y
        float newY = Mathf.MoveTowards(transform.localScale.y, targetHeight, growthSpeed * Time.deltaTime);

        // Aplikujemy now¹ skalê (X i Z bierzemy ze startu, Y zmieniamy)
        transform.localScale = new Vector3(initialScale.x, newY, initialScale.z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}