using TMPro;
using UnityEngine;

public class GetTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TimeText;
    private string timeString;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeString = SpeedrunManager.Instance.GetFormattedTime();
        TimeText.text = timeString ;
    }

    // Update is called once per frame
    void Update()
    {
        timeString = SpeedrunManager.Instance.GetFormattedTime();
        TimeText.text = timeString;
    }
}
