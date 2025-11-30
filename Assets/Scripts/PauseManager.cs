using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // 1. Ważne: Nowy system wejścia

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Przeciągnij tutaj cały obiekt panelu pauzy (tło + przyciski)")]
    public GameObject pausePanel; 

    private bool isPaused = false;

    void Start()
    {
        // Na starcie ukrywamy menu i upewniamy się, że gra chodzi
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Zabezpieczenie: sprawdź czy klawiatura jest podłączona
        if (Keyboard.current == null) return;

        // 2. NOWY SYSTEM: Sprawdzamy klawisz ESC (Escape)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        // Opcjonalnie: Szybki restart pod klawiszem R
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartLevel();
        }
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f; // Odmrażamy czas
        isPaused = false;
    }

    void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f; // Zamrażamy czas (fizyka i update stają)
        isPaused = true;
    }

    public void RestartLevel()
    {
        // 1. Odmrażamy fizykę (to nadal ważne!)
        Time.timeScale = 1f;
        
        // 2. Nie musimy już resetować TimeManagera, bo on zginie razem ze sceną
        // i nowy stworzy się w nowej scenie.

        // 3. Ładujemy scenę
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu"); 
    }

    public void QuitGame()
    {
        Debug.Log("Wychodzenie z gry..."); // Widać tylko w edytorze
        Application.Quit();
    }
}