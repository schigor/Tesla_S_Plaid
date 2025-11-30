using UnityEngine;
using TMPro;

public class GlobalTimeManager : MonoBehaviour
{
    public static GlobalTimeManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeMultiplierText;

    // Mnożnik czasu startuje zawsze od 1.0 po restarcie
    public float gameTimeMultiplier = 1.0f;

    [Header("Zmieniane przez czas")]
    public Transform player; // Możesz to przypisać w Inspectorze w każdej scenie
    public float range = 5f;

    private void Awake()
    {
        // Po prostu przypisujemy instancję.
        // Nowa scena = Nowy Manager = Nowa Instancja.
        Instance = this;
        
        // USUNĘLIŚMY DontDestroyOnLoad(this.gameObject);
        // Dzięki temu ten obiekt zginie przy zmianie sceny.
    }

    private void Update()
    {
        if (timeMultiplierText != null)
        {
            timeMultiplierText.text = $"Time Speed: {gameTimeMultiplier:F2}x";
            
            if (gameTimeMultiplier > 1.01f) timeMultiplierText.color = Color.red;
            else timeMultiplierText.color = Color.cyan;
        }
    }
}