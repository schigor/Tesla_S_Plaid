using UnityEngine;
using TMPro; // 1. Ważne: Dodajemy bibliotekę do obsługi tekstu

public class GlobalTimeManager : MonoBehaviour
{
    public static GlobalTimeManager Instance { get; private set; }

    [Header("UI - Debugowanie Czasu")]
    [SerializeField] private TextMeshProUGUI timeMultiplierText; // Wyświetla aktualną prędkość (np. 1.5x)
    [SerializeField] private TextMeshProUGUI amplitudeText;      // Wyświetla siłę chaosu
    [SerializeField] private TextMeshProUGUI frequencyText;      // Wyświetla szybkość zmian

    [Header("Ustawienia Bazowe")]
    public float baseTimeSpeed = 1.0f;

    [Header("Narastanie Chaosu")]
    public float amplitudeGrowthRate = 0.05f;
    public float maxAmplitude = 0.9f;
    public float frequencyGrowthRate = 0.1f;
    public float maxFrequency = 5.0f;

    // Publiczne właściwości (do odczytu przez inne skrypty)
    public float CurrentTimeMultiplier { get; private set; }
    public float CurrentAmplitude { get; private set; }
    public float CurrentFrequency { get; private set; }

    private float phase;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Update()
    {
        // 1. Logika Czasu (tak jak wcześniej)
        CurrentAmplitude += amplitudeGrowthRate * Time.unscaledDeltaTime;
        CurrentFrequency += frequencyGrowthRate * Time.unscaledDeltaTime;

        CurrentAmplitude = Mathf.Clamp(CurrentAmplitude, 0f, maxAmplitude);
        CurrentFrequency = Mathf.Clamp(CurrentFrequency, 0f, maxFrequency);

        phase += CurrentFrequency * Time.unscaledDeltaTime;
        float sineValue = Mathf.Sin(phase);

        CurrentTimeMultiplier = baseTimeSpeed + (sineValue * CurrentAmplitude);

        // 2. Aktualizacja UI (Nowość)
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Sprawdzamy czy przypisaliśmy tekst, żeby nie wywaliło błędu
        if (timeMultiplierText != null)
        {
            // F2 oznacza formatowanie do 2 miejsc po przecinku
            timeMultiplierText.text = $"Time Speed: {CurrentTimeMultiplier:F2}x";
            
            // Opcjonalnie: Zmiana koloru - Czerwony jak szybko, Niebieski jak wolno
            if (CurrentTimeMultiplier > 1.0f) timeMultiplierText.color = Color.red;
            else timeMultiplierText.color = Color.cyan;
        }

        if (amplitudeText != null)
        {
            amplitudeText.text = $"Chaos (Amp): {CurrentAmplitude:F2}";
        }

        if (frequencyText != null)
        {
            frequencyText.text = $"Pulse (Freq): {CurrentFrequency:F2}";
        }
    }

    public void ResetChaos()
    {
        CurrentAmplitude = 0f;
        CurrentFrequency = 0f;
        phase = 0f;
    }
}