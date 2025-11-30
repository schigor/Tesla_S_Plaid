using System.Collections;
using UnityEngine;

public class FallingBlock : MonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float fallDelay = 0.5f;   // Czas zanim zacznie spadać
    [SerializeField] private float disableDelay = 2.0f; // Czas po którym znika (zmieniłem nazwę z destroyDelay)
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool hasPlayerSteppedOn = false;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // 1. Zapamiętujemy pozycję startową przy uruchomieniu gry
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Upewniamy się, że na starcie blok wisi nieruchomo
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasPlayerSteppedOn)
        {
            // Sprawdzamy, czy gracz jest NA bloku (opcjonalne, żeby nie spadał jak uderzysz głową od dołu)
            // relativeVelocity.y < 0 oznacza, że gracz opadał na blok
            if (collision.relativeVelocity.y <= 0) 
            {
                hasPlayerSteppedOn = true;
                StartCoroutine(Fall());
            }
        }
    }

    private IEnumerator Fall()
    {
        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
        if (timeMod < 0.01f) timeMod = 0.01f; // Zabezpieczenie przed dzieleniem przez zero

        // Czekamy na spadnięcie (uwzględniając czas gry)
        yield return new WaitForSeconds(fallDelay / timeMod);

        // Odblokowujemy grawitację -> blok spada
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Czekamy aż blok spadnie w przepaść
        yield return new WaitForSeconds(disableDelay / timeMod);

        // ZAMIAST Destroy -> Wyłączamy obiekt
        gameObject.SetActive(false);
    }

    // --- FUNKCJA RESETUJĄCA ---
    // Tę funkcję wywołamy ze skryptu PlayerHealth
    public void ResetBlock()
    {
        // 1. Zatrzymaj wszelkie odliczanie (jeśli gracz zginął ZANIM blok spadł)
        StopAllCoroutines();

        // 2. Włącz obiekt z powrotem (jeśli był ukryty)
        gameObject.SetActive(true);

        // 3. Przywróć pozycję i rotację
        transform.position = startPosition;
        transform.rotation = startRotation;

        // 4. Zresetuj fizykę
        rb.linearVelocity = Vector2.zero; // Jeśli starsze Unity: użyj rb.velocity
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // Znów "przyklejony" do powietrza

        // 5. Zresetuj flagę, żeby mógł spaść ponownie
        hasPlayerSteppedOn = false;
    }
}