using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public RectTransform creditsText;   // assign your credits text object
    public float scrollSpeed = 50f;     // pixels per second
    public bool scrollDown = true;      // true = down, false = up

    [Header("Scene Settings")]
    public string nextSceneName = "MainMenu"; // scene to load after credits
    public float creditsDuration = 20f;       // how long before switching

    private float timer = 0f;

    private void Update()
    {
        if (creditsText != null)
        {
            float direction = scrollDown ? -1f : 1f;
            creditsText.anchoredPosition += new Vector2(0f, direction * scrollSpeed * Time.deltaTime);
        }

        timer += Time.deltaTime;
        if (timer >= creditsDuration)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
