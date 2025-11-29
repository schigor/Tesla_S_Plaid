using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Miejsce, gdzie przypiszemy nasz tekst w Unity
    public TextMeshProUGUI myText; 

    // Ta funkcja musi być PUBLICZNA, żeby przycisk ją widział
    public void ChangeMessage()
    {
        myText.text = "Witaj w Unity!";
        Debug.Log("Przycisk został kliknięty!");
    }
    
    public void QuitGame()
    {
        // Ta komenda zamyka zbudowaną aplikację
        Application.Quit();

        // To wyświetli komunikat w konsoli Unity, żebyś wiedział, że przycisk działa
        // (ponieważ Application.Quit nie zadziała w samym edytorze)
        Debug.Log("Gra została zamknięta! (Działa tylko w buildzie)");
        
        // Opcjonalnie: Ten kod poniżej wymusi zatrzymanie gry w Edytorze Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}