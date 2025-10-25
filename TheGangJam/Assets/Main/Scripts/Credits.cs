using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public RectTransform creditsText;   
    public float scrollSpeed = 50f;    
    public bool scrollDown = true;      

    [Header("Scene Settings")]
    public string nextSceneName = "MainMenu"; //AfterCreditsScene
    public float creditsDuration = 20f;  

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
