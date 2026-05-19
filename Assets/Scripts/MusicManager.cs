using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string currentSceneName;

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += PlayMusic;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= PlayMusic;
    }
    void PlayMusic(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == currentSceneName)
            return;
        
        currentSceneName = scene.name;

        AudioClip clipToPlay = null;

        if (currentSceneName == "Menu")
        {
            clipToPlay = menuTheme;
        }
        else if (currentSceneName == "Game")
        {
            clipToPlay = mainTheme;
        }

        if (clipToPlay != null)
        {
            AudioManager.instance.PlayMusic(clipToPlay, 2);
        }
    }

}
