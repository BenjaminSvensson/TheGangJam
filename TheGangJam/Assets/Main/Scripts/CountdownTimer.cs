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

    [Header("Death Manager")]
    public UniversalDeath deathManager; // assign in Inspector

    private bool isPaused = false;
    private bool hasTriggeredDeath = false;

    private void Awake()
    {
        currentTime = maxTime;
        UpdateText();
    }

    private void Update()
    {
        if (isPaused || hasTriggeredDeath) return;

        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0f) currentTime = 0f;
            UpdateText();
        }

        if (currentTime <= 0f && !hasTriggeredDeath)
        {
            hasTriggeredDeath = true;
            if (deathManager != null)
            {
                deathManager.KillPlayer();
            }
            else
            {
                Debug.LogWarning("No UniversalDeathManager assigned to CountdownTimer!");
            }
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
        hasTriggeredDeath = false; // reset death trigger
        UpdateText();
    }
}
