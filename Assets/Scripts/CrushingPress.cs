using System.Collections;
using UnityEngine;

public class CrushingPress : MonoBehaviour
{
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;
    [SerializeField] private float pressSpeed = 3f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float pauseAtBottomDuration = 1f;
    
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform playerSpawnPoint;

    private bool isPlayerCrushed = false;

    private void Start()
    {
        if (startPosition == null)
        {
            startPosition = transform;
        }
        
        // Zacznij pętlę ruchu natychmiast
        StartCoroutine(PressLoop());
    }

    private IEnumerator PressLoop()
    {
        while (true)
        {
            // Prasa idzie w dół
            yield return StartCoroutine(MoveToPosition(endPosition.position, pressSpeed));

            // Czekaj na dole
            float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
            yield return new WaitForSeconds(pauseAtBottomDuration / timeMod);

            // Prasa idzie w górę
            yield return StartCoroutine(MoveToPosition(startPosition.position, returnSpeed));
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed)
    {
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;
            float adjustedSpeed = speed / timeMod;
            
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, adjustedSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isPlayerCrushed)
        {
            // Sprawdź czy prasa jest na dole I gracz jest nad podłogą
            if (Vector2.Distance(transform.position, endPosition.position) < 0.15f)
            {
                // Pobranie pozycji gracza
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Sprawdź czy gracz ma grawitację (jest pod prasą)
                    // Gracz jest ggnieciony tylko jeśli prasa jest nad nim
                    if (playerRb.transform.position.y < transform.position.y)
                    {
                        isPlayerCrushed = true;
                        KillPlayer();
                        StartCoroutine(ResetCrushFlag());
                    }
                }
            }
        }
    }

    private void KillPlayer()
    {
        if (playerMovement != null && playerSpawnPoint != null)
        {
            // Teleportuj gracza na spawn
            playerMovement.transform.position = playerSpawnPoint.position;
            playerMovement.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator ResetCrushFlag()
    {
        yield return new WaitForSeconds(0.5f);
        isPlayerCrushed = false;
    }
}