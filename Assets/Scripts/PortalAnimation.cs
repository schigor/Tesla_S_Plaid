using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PortalAnimation : MonoBehaviour
{
    [Header("Ustawienia Animacji")]
    public Sprite[] frames;

    public float animationSpeed = 0.1f; // Czas wyœwietlania jednej klatki (szybciej ni¿ 0.2 dla p³ynnoœci)

    private SpriteRenderer spriteRenderer;
    private float animTimer;
    private int currentFrameIndex = 0; // Licznik: któr¹ klatkê aktualnie wyœwietlamy

    void Start()
    {
        // Pobieramy SpriteRenderer przy starcie gry
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleAnimation();
    }

    void HandleAnimation()
    {
        // Zabezpieczenie: jeœli nie przypisa³eœ klatek w inspektorze, nic nie rób
        if (frames == null || frames.Length == 0) return;

        // Naliczanie czasu
        animTimer += Time.deltaTime;

        // Jeœli min¹³ czas klatki -> zmieniamy na nastêpn¹
        if (animTimer >= animationSpeed)
        {
            animTimer = 0f; // Reset stopera

            // Przesuwamy indeks o 1 do przodu
            currentFrameIndex++;

            // Jeœli indeks wyszed³ poza liczbê dostêpnych klatek, wracamy do 0 (pêtla)
            if (currentFrameIndex >= frames.Length)
            {
                currentFrameIndex = 0;
            }

            // Podmieniamy obrazek na aktualny z listy
            spriteRenderer.sprite = frames[currentFrameIndex];
        }
    }
}