using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TimeAdaptiveMusic : MonoBehaviour
{
    // Singleton - aby upewnić się, że istnieje tylko jeden odtwarzacz muzyki
    public static TimeAdaptiveMusic Instance;

    [Header("Ustawienia")]
    [Tooltip("Jak szybko muzyka dostosowuje się do zmiany czasu (wygładzanie)")]
    public float pitchSmoothSpeed = 5f;

    private AudioSource audioSource;

    void Awake()
    {
        // Wzorzec Singleton z zachowaniem między scenami
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // KLUCZOWE: Nie niszcz tego obiektu przy zmianie sceny
        }
        else
        {
            // Jeśli taki obiekt już istnieje (np. wróciliśmy do Menu), niszczymy ten nowy, by nie było echa
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Upewnij się, że muzyka się zapętla i gra
        audioSource.loop = true;
        
        if (audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        // Sprawdzamy czy TimeManager istnieje
        if (GlobalTimeManager.Instance != null)
        {
            float targetPitch = GlobalTimeManager.Instance.gameTimeMultiplier;

            if (targetPitch >2f) 
            {
                targetPitch = 1.5f;
            }
            else if (targetPitch  < 1f) 
            {
                targetPitch = 0.65f;
            }
            else 
            {
                targetPitch = 1.0f;
            }

            // Zabezpieczenie: Pitch nie może być ujemny w prostym AudioSource (chyba że chcesz grać od tyłu)
            targetPitch = Mathf.Max(0.1f, targetPitch); 

            // Płynna zmiana prędkości muzyki (Lerp)
            audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * pitchSmoothSpeed);
        }
        else
        {
            // Jeśli nie ma TimeManagera, wracamy do normalnej prędkości (1.0)
            audioSource.pitch = Mathf.Lerp(audioSource.pitch, 1.0f, Time.deltaTime * pitchSmoothSpeed);
        }
    }
}