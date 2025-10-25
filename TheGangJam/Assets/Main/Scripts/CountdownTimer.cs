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

    [Header("Audio")]
    public AudioSource audioSource;     
    public AudioClip normalLoop;          
    public AudioClip urgentLoop;          
    public float urgentThreshold = 20f;   // seconds left before switching

    private bool isRunning = true;
    private bool hasStarted = false;
    private bool isUrgent = false;

    private void Start()
    {
        initialMaxTime = maxTime;
        currentTime = maxTime;

        UpdateUI();

        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (clock != null)
            clock.gameObject.SetActive(false);

        // Start normal loop if assigned
        if (audioSource != null && normalLoop != null)
        {
            audioSource.clip = normalLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
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

        // Check if we need to switch to urgent loop
        if (!isUrgent && currentTime <= urgentThreshold)
        {
            SwitchToUrgentLoop();
        }

        UpdateUI();
    }

    private void SwitchToUrgentLoop()
    {
        isUrgent = true;
        if (audioSource != null && urgentLoop != null)
        {
            audioSource.Stop();
            audioSource.clip = urgentLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
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

        if (audioSource != null)
            audioSource.Stop();
    }

    public void ResetTimerOnDeath()
    {
        maxTime = initialMaxTime;
        currentTime = maxTime;
        isRunning = true;
        isUrgent = false;
        UpdateUI();

        // Restart normal loop
        if (audioSource != null && normalLoop != null)
        {
            audioSource.Stop();
            audioSource.clip = normalLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
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
        if (audioSource != null) audioSource.Pause();
    }

    public void ResumeTimer()
    {
        isRunning = true;
        if (audioSource != null) audioSource.UnPause();
    }

    public void ResetToMaxTime()
    {
        currentTime = maxTime;
        isRunning = true;
        isUrgent = false;
        UpdateUI();

        // Restart normal loop
        if (audioSource != null && normalLoop != null)
        {
            audioSource.Stop();
            audioSource.clip = normalLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
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
        if (audioSource != null) audioSource.Stop();
    }
}
