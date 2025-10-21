using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Exit_script : MonoBehaviour
{
    public Button quitbutton;
    public Button creditsButton;
    public Button startbutton;
    public string sceneName = "MainScene";
    public string sceneName2 = "Credits";

    void Start()
    {
        // Register the listeners ONCE
        quitbutton.onClick.AddListener(Quit);
        startbutton.onClick.AddListener(StartGame);
        creditsButton.onClick.AddListener(Goon);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  // stop Play Mode in Editor
#else
        Application.Quit();                   // quit built game
#endif
    }

    void StartGame()
    {
        SceneManager.LoadScene(sceneName);
    }

    void Goon()
    {
        SceneManager.LoadScene(sceneName2);
    }
}