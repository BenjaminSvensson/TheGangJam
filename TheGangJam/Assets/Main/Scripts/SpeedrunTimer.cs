using UnityEngine;
using TMPro;

public class SimpleTimerTMP : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText; // assign in Inspector

    private float elapsedTime;
    private bool isRunning;

    private void Start()
    {
        // Auto‑start so you can see it working immediately
        StartTimer();
    }

    private void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateUI();
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
        UpdateUI();
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = true; // restart immediately after reset
        UpdateUI();
    }

    public void StopTimer()
    {
        isRunning = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);

        timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
}
