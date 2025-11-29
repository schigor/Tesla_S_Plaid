using UnityEngine;
using UnityEngine.SceneManagement; // OBOWIĄZKOWE do zmiany scen

public class TutorialUI : MonoBehaviour
{
    // Funkcja, którą podepniemy pod trójkątny przycisk
    public void GoBackToMenu()
    {
        // Upewnij się, że Twoja scena menu nazywa się DOKŁADNIE "Menu"
        SceneManager.LoadScene("Menu");
    }
}