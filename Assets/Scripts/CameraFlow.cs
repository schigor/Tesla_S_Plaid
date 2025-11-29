using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cel do śledzenia")]
    public Transform target; // Tu przypiszemy Gracza

    [Header("Ustawienia")]
    [Range(0, 1)]
    public float smoothSpeed = 0.125f; // Jak szybko kamera goni gracza (0.1 = wolno/płynnie, 1 = natychmiast)
    public Vector3 offset; // Przesunięcie (np. żeby kamera była oddalona w osi Z)

    // Używamy LateUpdate, żeby kamera ruszała się PO tym, jak gracz skończy swój ruch w Update/FixedUpdate.
    // To zapobiega drganiu (jitter) kamery.
    void LateUpdate()
    {
        if (target == null) return;

        // Obliczamy pozycję, w której kamera chce się znaleźć
        Vector3 desiredPosition = target.position + offset;
        
        // Płynne przejście z obecnej pozycji do pozycji docelowej
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Przypisanie nowej pozycji
        transform.position = smoothedPosition;
    }
}