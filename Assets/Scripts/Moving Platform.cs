using UnityEngine;
using System.Collections; // Potrzebne do Coroutine

public class MovingPlatform : MonoBehaviour
{
    [Header("Ustawienia")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 1f; // Czas oczekiwania na stacji (w sekundach)

    private Vector3 targetPos;
    private bool isWaiting = false; // Czy platforma aktualnie odpoczywa?

    void Start()
    {
        transform.position = pointA.position;
        targetPos = pointB.position;
    }

    void Update()
    {
        // Jeśli aktualnie czekamy, nie wykonuj ruchu
        if (isWaiting) return;

        // Ruch platformy
        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime * timeMod);

        // Sprawdzenie czy dotarliśmy
        if (Vector2.Distance(transform.position, targetPos) < 0.05f)
        {
            // Zamiast od razu zmieniać cel, uruchamiamy procedurę czekania
            StartCoroutine(WaitAndSwitch());
        }
    }

    // Specjalna funkcja obsługująca czekanie
    IEnumerator WaitAndSwitch()
    {
        isWaiting = true; // Blokujemy ruch

        // Czekamy określoną liczbę sekund
        yield return new WaitForSeconds(waitTime);

        // Zmieniamy cel
        if (targetPos == pointB.position)
        {
            targetPos = pointA.position;
        }
        else
        {
            targetPos = pointB.position;
        }

        isWaiting = false; // Odblokowujemy ruch
    }

    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}