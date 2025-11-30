using UnityEngine;
using TMPro;

public class EndGamePortal : MonoBehaviour
{
    [Header("UI Końcowe")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private TextMeshProUGUI finalTimeText;


 void Start()
    {
   
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FinishGame();
        }
    }

    private void FinishGame()
    {
        // 1. Zatrzymujemy stoper w SpeedrunManagerze
        if (SpeedrunManager.Instance != null)
        {
            SpeedrunManager.Instance.StopTimer();
            
            // 2. Pobieramy sformatowany tekst czasu
            string timeString = SpeedrunManager.Instance.GetFormattedTime();
            
            // 3. Wyświetlamy
            if (finalTimeText != null)
            {
                finalTimeText.text = "Your time: " + timeString + (SpeedrunManager.Instance.IsBestTime() ? " New Best!" : "");
            }
        }

        // 4. Pokazujemy ekran wygranej
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        // 5. Zatrzymujemy grę
        Time.timeScale = 0f;
    }
}