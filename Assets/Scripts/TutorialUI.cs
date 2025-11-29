using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialUI : MonoBehaviour
{
    public void GoBackToMenu()
    {
        Debug.Log("ZADZIAŁAŁO! Próbuję zmienić scenę..."); // <--- To nam powie prawdę
        SceneManager.LoadScene("Menu");
    }
}