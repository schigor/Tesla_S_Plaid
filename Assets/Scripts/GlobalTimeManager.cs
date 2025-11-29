using UnityEngine;
using TMPro;

public class GlobalTimeManager : MonoBehaviour
{
    public static GlobalTimeManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeMultiplierText;

    // Mnożnik czasu dla reszty gry (przeciwnicy itp.)
    public float gameTimeMultiplier = 1.0f;


    [Header("Zmieniane przez czas")]
    public Transform player;
    public float range = 5f; // Zwiększyłem domyślnie, 1f to bardzo blisko

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else
        {
            Instance = this;
            Instance.player = player;
            Instance.range = range;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Update()
    {
        if (timeMultiplierText != null)
        {
            timeMultiplierText.text = $"Time Speed: {gameTimeMultiplier:F2}x";
            
            if (gameTimeMultiplier > 1.0f) timeMultiplierText.color = Color.red;
            else timeMultiplierText.color = Color.cyan;
        }
    }
}