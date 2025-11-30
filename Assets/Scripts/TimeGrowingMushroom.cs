using UnityEngine;

public class TimeGrowingMushroom : MonoBehaviour
{
    [Header("Ustawienia Wzrostu")]
    [Tooltip("Mno¿nik skali (np. 2.5 = 2.5x wy¿szy i szerszy)")]
    public float scaleMultiplier = 2.5f;
    [Tooltip("Jak szybko roœnie")]
    public float growthSpeed = 2.0f;

    [Header("Aktywacja")]
    public float activationRange = 5.0f;
    public Transform player;

    private Vector3 initialScale;
    private Vector3 targetScale;
    private bool hasGrown = false; // Ta zmienna blokuje powrót do ma³ego rozmiaru

    void Start()
    {
        initialScale = transform.localScale;
        targetScale = initialScale;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // 1. Jeœli ju¿ urós³, upewniamy siê, ¿e cel to du¿y rozmiar i nie sprawdzamy dalej
        if (hasGrown)
        {
            targetScale = new Vector3(
                initialScale.x * scaleMultiplier,
                initialScale.y * scaleMultiplier,
                initialScale.z
            );
        }
        else
        {
            // 2. Jeœli jeszcze nie urós³, sprawdzamy warunki
            if (player != null)
            {
                float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
                float distance = Vector2.Distance(transform.position, player.position);

                // Warunek: blisko gracza ORAZ szybki czas
                if (distance <= activationRange && timeMod > 1.1f)
                {
                    hasGrown = true; // Zaznaczamy na sta³e: URÓS£!
                }
            }
        }

        // 3. P³ynne skalowanie do celu
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, growthSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}