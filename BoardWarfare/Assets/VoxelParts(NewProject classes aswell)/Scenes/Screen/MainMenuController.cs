using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public AudioSource uiSource;  
    public AudioClip uiClip;

    public void ChangeScene(string sceneName)
    {
        PlayClickSound();      
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        PlayClickSound();       
        Application.Quit();
    }

    public void PlayClickSound()
    {
        if (uiSource != null && uiClip != null)
        {
            uiSource.PlayOneShot(uiClip);
        }
    }
}