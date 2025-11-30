using UnityEngine;

public class SpeedrunManager : MonoBehaviour
{
    public static SpeedrunManager Instance { get; private set; }

    public float TotalTime { get; private set; }
    public bool IsRunning { get; private set; }
    public float BestTime { get; private set; }

    private void Awake()
    {
        // Singleton - upewniamy się, że jest tylko jeden taki licznik w grze
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Jeśli już istnieje (np. wróciliśmy do menu), niszczymy duplikat
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject); // To sprawia, że ten obiekt NIE ginie przy zmianie sceny
        
        StartTimer(); // Startujemy od razu (lub możesz wywołać to z innej metody)
    }

    private void Update()
    {
        if (IsRunning)
        {
            // Używamy unscaledDeltaTime, żeby Twoje spowalnianie czasu gry NIE wpływało na stoper
            TotalTime += Time.unscaledDeltaTime;
        }
    }

    public void StartTimer()
    {
        IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    // Metoda do resetowania licznika (np. przy New Game)
    public void ResetTimer()
    {
        TotalTime = 0f;
        IsRunning = true;
    }

    // Pomocnicza funkcja do ładnego wyświetlania czasu (zwraca string)
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(TotalTime / 60);
        int seconds = Mathf.FloorToInt(TotalTime % 60);
        int milliseconds = Mathf.FloorToInt((TotalTime * 100) % 100);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public bool IsBestTime()
    {
        return TotalTime < BestTime || BestTime == 0f;
    }
}