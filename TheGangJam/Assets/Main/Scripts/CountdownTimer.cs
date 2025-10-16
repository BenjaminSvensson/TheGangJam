using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float maxTime = 60f;
    public float currentTime;
    public bool autoStart = true;

    [Header("UI")]
    public TMP_Text timerText;   // drag your UI text here in Inspector

    private bool isPaused = false;

    private void Awake()
    {
        currentTime = maxTime;
        UpdateText();
    }

    private void Update()
    {
        if (isPaused) return;

        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0f) currentTime = 0f;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }

    public void PauseTimer() => isPaused = true;
    public void ResumeTimer() => isPaused = false;

    public void ResetToMaxTime()
    {
        currentTime = maxTime;
        UpdateText();
    }
}
