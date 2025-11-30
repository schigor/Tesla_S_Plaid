using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // <-- TO JEST KLUCZOWE! Bez tego zmiana scen nie zadziała.

public class GameManager : MonoBehaviour
{
    // Miejsce, gdzie przypiszemy nasz tekst w Unity (opcjonalne, jeśli już tego nie używasz)
    public TextMeshProUGUI myText; 

    // Funkcja dla przycisku START GAME
    public void StartGame()
    {
        if (SpeedrunManager.Instance != null)
    {
        SpeedrunManager.Instance.ResetTimer();
    }
    

        // Upewnij się, że nazwa sceny jest DOKŁADNIE taka sama (wielkość liter ma znaczenie!)
        SceneManager.LoadScene("Poziom Dino");
    }

    // Funkcja dla przycisku TUTORIAL
    public void OpenTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    // Twoja funkcja testowa
    public void ChangeMessage()
    {
        if(myText != null) 
        {
             myText.text = "Witaj w Unity!";
        }
        Debug.Log("Przycisk został kliknięty!");
    }
    
    // Funkcja wyjścia
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Gra została zamknięta! (Działa tylko w buildzie)");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}