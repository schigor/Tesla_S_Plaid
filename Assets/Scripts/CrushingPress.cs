using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrushingPress : MonoBehaviour
{
    [Header("Ruch Prasy")]
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;
    [SerializeField] private float pressSpeed = 3f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float pauseAtBottomDuration = 1f;
    
    [Header("Gracz i Zabijanie")]
    [SerializeField] private PlayerMovement playerMovement;
    // SpawnPoint nie jest używany przy przeładowaniu sceny, ale zostawiam pole
    [SerializeField] private Transform playerSpawnPoint; 
    
    [Header("Ustawienia Wykrywania")]
    [SerializeField] private LayerMask playerLayer; 
    [SerializeField] private Vector2 killZoneSize = new Vector2(0.8f, 0.2f);
    [SerializeField] private float killZoneOffset = 0.1f; // Odstęp od krawędzi prasy

    private BoxCollider2D col;
    private bool isPlayerCrushed = false;
    
    private Vector3 fixedStartPos;
    private Vector3 fixedEndPos;

    private void Start()
    {
        col = GetComponent<BoxCollider2D>();
        
        if (startPosition == null) startPosition = transform;
        if (endPosition == null) 
        {
            Debug.LogError("BRAK END POSITION!");
            return;
        }

        fixedStartPos = startPosition.position;
        fixedEndPos = endPosition.position;

        StartCoroutine(PressLoop());
    }

    private void FixedUpdate()
    {
        // Sprawdzamy obie strony w każdej klatce fizyki
        // 1. Sprawdź DÓŁ (Zgniatanie o podłogę)
        CheckCrush(Vector2.down);

        // 2. Sprawdź GÓRĘ (Zgniatanie o sufit/Start Position)
        CheckCrush(Vector2.up);
    }

    // Uniwersalna funkcja sprawdzająca (kierunek: Vector2.up lub Vector2.down)
    private void CheckCrush(Vector2 direction)
    {
        if (isPlayerCrushed || col == null) return;

        float checkX = col.bounds.center.x;
        float checkY = 0f;

        // Ustawiamy strefę na górze lub na dole prasy
        if (direction == Vector2.down)
        {
            // Dół: Dolna krawędź - offset
            checkY = col.bounds.min.y - killZoneOffset - (killZoneSize.y / 2);
        }
        else
        {
            // Góra: Górna krawędź + offset
            checkY = col.bounds.max.y + killZoneOffset + (killZoneSize.y / 2);
        }

        Vector2 checkPos = new Vector2(checkX, checkY);

        // Sprawdzamy czy w strefie jest gracz
        Collider2D hit = Physics2D.OverlapBox(checkPos, killZoneSize, 0f, playerLayer);

        if (hit != null)
        {
            if (hit.CompareTag("Player") || hit.GetComponent<PlayerMovement>() != null)
            {
                // LOGIKA ZABITYCH:
                
                // Przypadek 1: DÓŁ -> Zabijamy, jeśli prasa spada (nie jest w punkcie startowym)
                if (direction == Vector2.down)
                {
                    if (Vector2.Distance(transform.position, fixedStartPos) > 0.1f)
                        KillPlayer();
                }
                // Przypadek 2: GÓRA -> Zabijamy, jeśli prasa wraca (jest blisko sufitu)
                // Gracz może stać na prasie, ale jak dojedziemy do StartPosition, strefa go zmiażdży.
                else if (direction == Vector2.up)
                {
                    // Zabijamy, jeśli jesteśmy blisko sufitu (np. 0.5f od startu)
                    // Dzięki temu można bezpiecznie wskoczyć na prasę, gdy jest na dole.
                    if (Vector2.Distance(transform.position, fixedStartPos) < 1.0f) 
                    {
                        KillPlayer();
                    }
                }
            }
        }
    }

    private IEnumerator PressLoop()
    {
        while (true)
        {
            // RUCH W DÓŁ
            yield return StartCoroutine(MoveToPosition(fixedEndPos, pressSpeed));

            // PAUZA
            float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
            if (timeMod < 0.01f) timeMod = 0.01f; 
            yield return new WaitForSeconds(pauseAtBottomDuration / timeMod);

            // RUCH W GÓRĘ
            yield return StartCoroutine(MoveToPosition(fixedStartPos, returnSpeed));
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed)
    {
        // Prosty ruch bez wykrywania zacięć (bo strefy śmierci załatwią sprawę)
        while (Vector2.Distance(transform.position, targetPosition) > 0.01f)
        {
            float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
            float adjustedSpeed = speed * timeMod; 
            
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, adjustedSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
    }

    private void KillPlayer()
    {
        if (!isPlayerCrushed)
        {
            isPlayerCrushed = true;
            Debug.Log("ZNIECENIE! Restart...");

            // Resetujemy czas
            Time.timeScale = 1f;
            
            // Restart Sceny
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnDrawGizmos()
    {
        if (GetComponent<BoxCollider2D>() != null)
        {
            // Rysujemy DÓŁ (Czerwony)
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            DrawGizmoZone(Vector2.down);

            // Rysujemy GÓRĘ (Żółty) - to ta strefa pod sufitem
            Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);
            DrawGizmoZone(Vector2.up);
        }
    }

    private void DrawGizmoZone(Vector2 direction)
    {
        var tempCol = GetComponent<BoxCollider2D>();
        float checkX = tempCol.bounds.center.x; 
        
        if (!Application.isPlaying) checkX = transform.position.x + tempCol.offset.x;

        float edgeY = (direction == Vector2.down) ? tempCol.bounds.min.y : tempCol.bounds.max.y;
        if (!Application.isPlaying) 
            edgeY = transform.position.y + tempCol.offset.y + ((tempCol.size.y / 2) * (direction == Vector2.down ? -1 : 1));

        // Offset w górę lub w dół
        float yPos = edgeY + (killZoneOffset + (killZoneSize.y / 2)) * (direction == Vector2.down ? -1 : 1);

        Gizmos.DrawCube(new Vector2(checkX, yPos), killZoneSize);
    }
}