using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel; // Tutaj przypiszemy nasz Panel (Czarne tło)
    private bool isPaused = false;

    void Start()
    {
        // Na starcie gry upewniamy się, że panel jest ukryty
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Upewniamy się, że czas płynie
    }

    void Update()
    {
        // Jeśli wciśnięto ESC
        if (Input.GetKeyDown(KeyCode.Escape))
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
    }

    public void RestartLevel()
    {
        // 1. Najważniejsze: Odmrażamy czas, bo inaczej nowa gra będzie stała w miejscu
        Time.timeScale = 1f;
        
        // 2. Pobieramy nazwę aktualnej sceny, na której jesteśmy
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 3. Ładujemy ją ponownie
        SceneManager.LoadScene(currentSceneName);
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false); // Ukryj panel
        Time.timeScale = 1f;         // Czas start
        isPaused = false;
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);  // Pokaż panel
        Time.timeScale = 0f;         // Czas stop (zamrożenie gry)
        isPaused = true;
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f; // BARDZO WAŻNE: Odmrażamy czas przed zmianą sceny!
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}