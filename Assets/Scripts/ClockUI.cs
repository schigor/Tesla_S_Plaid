using UnityEngine;
using UnityEngine.UI; // Wymagane do obsługi UI

[RequireComponent(typeof(Image))] // Automatycznie doda komponent Image, jeśli go nie ma
public class ClockUI : MonoBehaviour
{
    [Header("Animacja")]
    [Tooltip("Przypisz tutaj swoje 4 klatki zegara w kolejności")]
    [SerializeField] private Sprite[] clockFrames;

    [Header("Prędkość")]
    [Tooltip("Ile czasu trwa jedna klatka przy normalnej prędkości (1.0x). Np. 0.25s")]
    [SerializeField] private float baseFrameDuration = 0.25f;

    private Image clockImage;
    private int currentFrameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        clockImage = GetComponent<Image>();
    }

    private void Update()
    {
        // 1. Zabezpieczenie: jeśli nie ma klatek lub Managera, nic nie rób
        if (clockFrames.Length == 0 || GlobalTimeManager.Instance == null) return;

        // 2. Pobieramy aktualny mnożnik czasu z Twojego skryptu
        float timeMultiplier = GlobalTimeManager.Instance.gameTimeMultiplier;

        // Jeśli czas jest zatrzymany lub bardzo wolny, nie animujemy (opcjonalne)
        if (timeMultiplier <= 0.01f) return;

        // 3. Obliczamy upływ czasu dla animacji
        // Używamy unscaledDeltaTime, żeby animacja UI była płynna, 
        // a mnożymy ją przez timeMultiplier, żeby reagowała na mechanikę gry.
        timer += Time.unscaledDeltaTime * timeMultiplier;

        // 4. Sprawdzamy czy czas zmienić klatkę
        if (timer >= baseFrameDuration)
        {
            // Resetujemy timer (odejmujemy duration, żeby zachować precyzję przy dużych prędkościach)
            timer -= baseFrameDuration;
            
            // Przełączamy na następną klatkę
            NextFrame();
        }
    }

    private void NextFrame()
    {
        currentFrameIndex++;

        // Zapętlenie animacji (jeśli przekroczymy ilość klatek, wracamy do 0)
        if (currentFrameIndex >= clockFrames.Length)
        {
            currentFrameIndex = 0;
        }

        // Podmiana grafiki
        clockImage.sprite = clockFrames[currentFrameIndex];
    }
}