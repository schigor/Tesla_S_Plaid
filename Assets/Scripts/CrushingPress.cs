using System.Collections;
using UnityEngine;

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
    [SerializeField] private Transform playerSpawnPoint;
    
    [Header("Ustawienia Wykrywania")]
    [SerializeField] private LayerMask playerLayer; 
    [SerializeField] private Vector2 killZoneSize = new Vector2(0.8f, 0.2f);
    [SerializeField] private float killZoneOffset = 0.1f;

    private BoxCollider2D col;
    private bool isPlayerCrushed = false;
    
    // Zmienne do zapamiętania pozycji "startowych" (gdybyś ruszał end pointami w trakcie gry)
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

        // Zapamiętujemy pozycje raz na starcie
        fixedStartPos = startPosition.position;
        fixedEndPos = endPosition.position;

        StartCoroutine(PressLoop());
    }

    private void FixedUpdate()
    {
        CheckKillZone();
    }

    private void CheckKillZone()
    {
        if (isPlayerCrushed || col == null) return;

        float checkX = col.bounds.center.x;
        float checkY = col.bounds.min.y - killZoneOffset - (killZoneSize.y / 2);
        Vector2 checkPos = new Vector2(checkX, checkY);

        Collider2D hit = Physics2D.OverlapBox(checkPos, killZoneSize, 0f, playerLayer);

        if (hit != null)
        {
            if (hit.CompareTag("Player") || hit.GetComponent<PlayerMovement>() != null)
            {
                // Zabijamy tylko jeśli prasa jest w fazie spadania (nie jest blisko startu)
                if (Vector2.Distance(transform.position, fixedStartPos) > 0.5f)
                {
                    KillPlayer();
                }
            }
        }
    }

    private IEnumerator PressLoop()
    {
        while (true)
        {
            // 1. RUCH W DÓŁ
            yield return StartCoroutine(MoveToPosition(fixedEndPos, pressSpeed));

            // 2. PAUZA NA DOLE
            float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
            // Zabezpieczenie przed dzieleniem przez zero
            if (timeMod < 0.01f) timeMod = 0.01f; 
            
            yield return new WaitForSeconds(pauseAtBottomDuration / timeMod);

            // 3. RUCH W GÓRĘ
            yield return StartCoroutine(MoveToPosition(fixedStartPos, returnSpeed));
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed)
    {
        // Zmienne do wykrywania zacięcia (uderzenia w ziemię)
        Vector3 lastFramePosition = transform.position;
        float stuckTimer = 0f;

        // Pętla działa dopóki nie jesteśmy bardzo blisko celu
        while (Vector2.Distance(transform.position, targetPosition) > 0.01f)
        {
            float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
            float adjustedSpeed = speed * timeMod; 
            
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, adjustedSpeed * Time.deltaTime);

            // --- DETEKCJA KOLIZJI Z PODŁOGĄ ---
            // Sprawdzamy, czy w tej klatce ruszyliśmy się chociaż o milimetr
            float distanceMoved = Vector3.Distance(transform.position, lastFramePosition);
            
            if (distanceMoved < 0.0001f) // Jeśli stoimy w miejscu
            {
                stuckTimer += Time.deltaTime;
                
                // Jeśli stoimy w miejscu przez 0.2 sekundy mimo rozkazu ruchu...
                if (stuckTimer > 0.2f)
                {
                    // ...to znaczy że w coś uderzyliśmy (ziemia). Przerywamy ruch!
                    break; 
                }
            }
            else
            {
                // Jeśli się ruszamy, resetujemy licznik
                stuckTimer = 0f;
            }

            lastFramePosition = transform.position;
            yield return null;
        }
        
        // Jeśli przerwaliśmy pętlę przez timer (uderzenie), to NIE ustawiamy pozycji na target,
        // bo target jest pod ziemią. Zostajemy tam gdzie uderzyliśmy.
        if (stuckTimer <= 0.2f)
        {
            transform.position = targetPosition;
        }
    }

    private void KillPlayer()
    {
        if (playerMovement != null && playerSpawnPoint != null)
        {
            isPlayerCrushed = true;
            Debug.Log("ZNIECENIE!");

            Rigidbody2D playerRb = playerMovement.GetComponent<Rigidbody2D>();
            if(playerRb != null) 
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.simulated = false; 
            }

            playerMovement.transform.position = playerSpawnPoint.position;
            StartCoroutine(ResetPlayerPhysics(playerRb));
        }
    }

    private IEnumerator ResetPlayerPhysics(Rigidbody2D playerRb)
    {
        yield return new WaitForSeconds(0.2f); 
        if(playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.linearVelocity = Vector2.zero;
        }
        isPlayerCrushed = false;
    }

    private void OnDrawGizmos()
    {
        if (GetComponent<BoxCollider2D>() != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            var tempCol = GetComponent<BoxCollider2D>();
            // Bezpieczne obliczanie bounds w edytorze
            float checkX = tempCol.bounds.center.x; 
            // Jeśli gra nie działa, bounds mogą być 0, więc fallback do transform + offset
            if (Application.isPlaying == false)
            {
                 checkX = transform.position.x + tempCol.offset.x;
            }
            
            float bottomY = tempCol.bounds.min.y;
             if (Application.isPlaying == false)
            {
                 bottomY = transform.position.y + tempCol.offset.y - (tempCol.size.y / 2);
            }

            Vector2 checkPos = new Vector2(checkX, bottomY - killZoneOffset - (killZoneSize.y / 2));
            Gizmos.DrawCube(checkPos, killZoneSize);
        }
    }
}