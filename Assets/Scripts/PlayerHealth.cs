using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private string trapTag = "Trap";

    [Header("Efekt Tonęcia")]
    [SerializeField] private float sinkingTime = 0.5f; // Ile czasu postać wpada w lawę zanim zastygnie
    [SerializeField] private float lavaDrag = 10f;     // Opór lawy (im więcej, tym wolniej tonie)

    [Header("UI Śmierci")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private float freezeTime = 1.0f;  // Czas wyświetlania napisu YOU DIED

    private Rigidbody2D rb;
    private PlayerMovement movementScript;
    private bool isDead = false;
    private float defaultDrag; // Zapamiętujemy oryginalny opór powietrza

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<PlayerMovement>();
        defaultDrag = rb.linearDamping; // Zapisujemy standardowy opór
    }

    private void Start()
    {
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(trapTag) && !isDead)
        {
            StartCoroutine(DieSequence());
        }
    }

    private IEnumerator DieSequence()
    {
        isDead = true;

        // --- ETAP 1: WPADANIE (TONIĘCIE) ---
        
        // 1. Odcinamy sterowanie (żeby gracz nie mógł wyskoczyć z lawy)
        if (movementScript != null) movementScript.enabled = false;

        // 2. Zmieniamy fizykę na "kisiel" (duży opór, powolne opadanie)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Zerujemy ruch na boki, zostawiamy spadanie
        rb.linearDamping = lavaDrag; 

        // 3. Czekamy, aż gracz się zanurzy (fizyka nadal działa!)
        yield return new WaitForSeconds(sinkingTime);


        // --- ETAP 2: ZAMROŻENIE I NAPIS ---

        // 4. Całkowite zamrożenie (Freeze)
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // Wyłączamy fizykę całkowicie

        // 5. Pokazujemy napis
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "YOU DIED";
        }

        // 6. Czekamy (patrząc na napis)
        yield return new WaitForSeconds(freezeTime);


        // --- ETAP 3: RESPAWN ---

        // 7. Przenosimy gracza
        transform.position = spawnPoint.position;
        
        // 8. Przywracamy ustawienia fizyki
        rb.simulated = true;
        rb.linearDamping = defaultDrag; // Przywracamy normalny opór powietrza
        rb.linearVelocity = Vector2.zero;

        // 9. Resetujemy Chaos Czasu
        /*if (GlobalTimeManager.Instance != null)
        {
            GlobalTimeManager.Instance.ResetChaos();
        }*/

        // 10. Sprzątamy UI i oddajemy sterowanie
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (movementScript != null) movementScript.enabled = true;

        isDead = false;
    }
}