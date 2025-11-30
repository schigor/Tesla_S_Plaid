using System.Collections;
using UnityEngine;

public class WaterFaucet : MonoBehaviour
{
    [Header("Elementy Kranu")]
    [SerializeField] private GameObject dropletPrefab; // Prefab kropli
    [SerializeField] private Transform spawnPoint;     // Punkt, z którego kapią
    [SerializeField] private GameObject streamLadderObject; // Obiekt strumienia (drabiny)

    [Header("Ustawienia Czasu")]
    // Powyżej tej wartości czasu włączy się strumień (drabina)
    [SerializeField] private float fastTimeThreshold = 1.5f;

    [Header("Ustawienia Kapania")]
    // Opóźnienie między kroplami przy normalnym czasie (1.0x)
    [SerializeField] private float baseDropDelay = 0.8f; 
    // Minimalne opóźnienie (żeby nie kapało jak z karabinu maszynowego przy czasie 1.4x)
    [SerializeField] private float minDropDelay = 0.2f;

    private bool isStreamActive = false;
    private float dropTimer = 0f;

    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        // Na starcie upewniamy się, że stan jest poprawny
        UpdateFaucetState(GlobalTimeManager.Instance.gameTimeMultiplier);
    }

    private void Update()
    {
        if (GlobalTimeManager.Instance == null) return;

        float currentTimeSpeed = GlobalTimeManager.Instance.gameTimeMultiplier;

        // 1. Sprawdzamy, w jakim trybie powinien być kran
        UpdateFaucetState(currentTimeSpeed);

        // 2. Jeśli nie ma strumienia, obsługujemy kapanie
        if (!isStreamActive)
        {
            HandleDroplets(currentTimeSpeed);
        }
    }

    private void UpdateFaucetState(float timeSpeed)
    {
        // Jeśli czas jest szybszy niż próg -> Włączamy strumień/drabinę
        if (timeSpeed >= fastTimeThreshold)
        {
            if (!isStreamActive)
            {
                streamLadderObject.SetActive(true);
                isStreamActive = true;
                // Resetujemy timer, żeby po wyłączeniu strumienia kropla spadła od razu
                dropTimer = 0f; 
            }
        }
        // W przeciwnym razie -> Wyłączamy strumień
        else
        {
            if (isStreamActive)
            {
                streamLadderObject.SetActive(false);
                isStreamActive = false;
            }
        }
    }

    private void HandleDroplets(float timeSpeed)
    {
        // Zabezpieczenie przed bardzo wolnym czasem
        if (timeSpeed <= 0.1f) timeSpeed = 0.1f;

        // Obliczamy aktualne opóźnienie. Im szybszy czas, tym mniejsze opóźnienie.
        // Np. Baza 0.8s. Przy czasie 2x, opóźnienie to 0.4s.
        float currentDelay = baseDropDelay / timeSpeed;
        
        // Ograniczamy, żeby nie kapało za szybko
        currentDelay = Mathf.Max(currentDelay, minDropDelay);

        // Standardowy timer
        dropTimer -= Time.deltaTime; // Używamy deltaTime, bo czas gry wpływa na wymagane opóźnienie, a nie na szybkość upływu timera

        if (dropTimer <= 0f)
        {
            SpawnDroplet();
            dropTimer = currentDelay;
        }
    }

    private void SpawnDroplet()
    {
        if (dropletPrefab != null)
        {
            Instantiate(dropletPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}