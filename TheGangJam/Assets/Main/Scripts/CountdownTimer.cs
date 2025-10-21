using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float maxTime = 60f;
    public float currentTime;
    private float initialMaxTime;

    [Header("UI")]
    public TMP_Text timerText;
    public GameObject clock;

    private bool isRunning = true;
    private bool hasStarted = false;

    private void Start()
    {
        initialMaxTime = maxTime;
        currentTime = maxTime;

        UpdateUI();

        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (clock != null)
            clock.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!hasStarted) return;
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

        UniversalDeath deathManager = FindFirstObjectByType<UniversalDeath>();
        if (deathManager != null)
        {
            deathManager.KillPlayer();
        }
        else
        {
            Debug.LogWarning("No UniversalDeathManager found in scene!");
        }
    }

    public void ResetTimerOnDeath()
    {
        maxTime = initialMaxTime;
        currentTime = maxTime;
        isRunning = true;
        UpdateUI();
    }

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
        if (clock != null)
            clock.gameObject.SetActive(true);

        hasStarted = true;
    }

    public void StopTimer()
    {
        isRunning = false;
        hasStarted = false;
    }
}
