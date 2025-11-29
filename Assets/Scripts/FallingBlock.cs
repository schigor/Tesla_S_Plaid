using System.Collections;
using UnityEngine;

public class FallingBlock : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float fallDelay = 0.5f;
    [SerializeField] private float destroyDelay = 2.0f;
    
    private bool hasPlayerSteppedOn = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasPlayerSteppedOn)
        {
            hasPlayerSteppedOn = true;
            StartCoroutine(Fall());
        }
    }

    private IEnumerator Fall()
    {
        // Pobieramy mnożnik czasu z GlobalTimeManager
        float timeMod = GlobalTimeManager.Instance != null ? GlobalTimeManager.Instance.gameTimeMultiplier : 1.0f;

        // Czekamy ze względu na mnożnik czasu
        yield return new WaitForSeconds(fallDelay / timeMod);

        // Odblokowujemy grawitację, aby blok spadł
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Niszczymy blok po opóźnieniu
        Destroy(gameObject, destroyDelay / timeMod);
    }
}