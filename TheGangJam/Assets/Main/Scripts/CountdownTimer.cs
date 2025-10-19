using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float maxTime = 60f;         // starting max time
    public float currentTime;           // current countdown value
    private float initialMaxTime;       // stored at Start

    [Header("UI")]
    public TMP_Text timerText;          // assign a TMP text in your UI

    private bool isRunning = true;
    private bool hasStarted = false;

    private void Start()
    {
        // Store the original max time
        initialMaxTime = maxTime;

        // Start with full time
        currentTime = maxTime;

        UpdateUI();

        if (timerText != null)
            timerText.gameObject.SetActive(false); // hide at start
    }

    private void Update()
    {
        if (!hasStarted) return; // don’t tick until started

        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            OnTimerEnd();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void OnTimerEnd()
    {
        Debug.Log("Timer ended! Triggering death...");

        // Call into your death system
        UniversalDeath  deathManager = FindFirstObjectByType<UniversalDeath>();
        if (deathManager != null)
        {
            deathManager.KillPlayer(); // make sure this method exists in your manager
        }
        else
        {
            Debug.LogWarning("No UniversalDeathManager found in scene!");
        }
    }

    /// <summary>
    /// Call this when the player dies to reset timer to original values.
    /// </summary>
    public void ResetTimerOnDeath()
    {
        maxTime = initialMaxTime;
        currentTime = maxTime;
        isRunning = true;
        UpdateUI();
    }

    /// <summary>
    /// Adds bonus time (used by powerups).
    /// </summary>
    public void AddBonusTime(float bonus)
    {
        maxTime += bonus;
        currentTime += bonus;
        if (currentTime > maxTime)
            currentTime = maxTime;

        UpdateUI();
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public void ResetToMaxTime()
    {
        currentTime = maxTime;
        isRunning = true;
        UpdateUI();
    }

    public void StartTimer()
    {
        if (timerText != null)
            timerText.gameObject.SetActive(true);

        hasStarted = true;
    }

}
