using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private string trapTag = "Trap";
    [SerializeField] private string enemyTag = "Enemy";

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
        
        // Unity 6 używa 'linearDamping', starsze wersje 'drag'. 
        // Skoro używasz linearDamping, zakładam że masz Unity 6.
        defaultDrag = rb.linearDamping; 
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(enemyTag) && !isDead)
        {
            StartCoroutine(DieSequence());
        }
    }

    private IEnumerator DieSequence()
    {
        isDead = true;

        // --- ETAP 1: WPADANIE (TONIĘCIE) ---
        
        // 1. Odcinamy sterowanie
        if (movementScript != null) movementScript.enabled = false;

        // 2. Zmieniamy fizykę na "kisiel"
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
        rb.linearDamping = lavaDrag; 

        // 3. Czekamy na zanurzenie
        yield return new WaitForSeconds(sinkingTime);

        // --- ETAP 2: ZAMROŻENIE I NAPIS ---

        // 4. Całkowite zamrożenie
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // Wyłączamy fizykę

        // 5. Pokazujemy napis
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "YOU DIED";
        }

        // 6. Czekamy patrząc na napis
        yield return new WaitForSeconds(freezeTime);

        // --- ETAP 3: RESPAWN I RESET ---

        // 7. RESETOWANIE PUŁAPEK (Falling Blocks) - TO JEST NOWOŚĆ
        // Szukamy wszystkich bloków, nawet tych wyłączonych (SetActive false)
        FallingBlock[] blocks = FindObjectsByType<FallingBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var block in blocks)
        {
            block.ResetBlock();
        }

        // 8. Przenosimy gracza
        transform.position = spawnPoint.position;
        
        // 9. Przywracamy ustawienia fizyki
        rb.simulated = true;
        rb.linearDamping = defaultDrag; 
        rb.linearVelocity = Vector2.zero;

        // 10. Resetujemy Chaos Czasu (odkomentuj jeśli chcesz)
        if (GlobalTimeManager.Instance != null)
        {
            // GlobalTimeManager.Instance.ResetChaos();
        }

        // 11. Sprzątamy UI i oddajemy sterowanie
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (movementScript != null) movementScript.enabled = true;

        isDead = false;
    }
}