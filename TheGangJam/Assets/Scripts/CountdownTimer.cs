using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTime = 60f;
    private float currentTime;
    private float currentMaxTime;

    [Header("UI (TextMesh Pro)")]
    public TMP_Text countdownText;

    private void Start()
    {
        currentMaxTime = startTime;
        currentTime = currentMaxTime;
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;

        if (countdownText != null)
        {
            countdownText.text = Mathf.Ceil(currentTime).ToString();
        }

        if (currentTime <= 0f)
        {
            ResetScene();
        }
    }

    private void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 👇 Call this when colliding with a checkpoint
    public void ResetToMaxTime()
    {
        currentTime = currentMaxTime;
    }
}
