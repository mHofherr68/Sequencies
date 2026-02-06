using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string level01SceneName = "SC_Level_01";

    public void NewGame()
    {
        SceneManager.LoadScene(level01SceneName);
    }

    public void LoadGame()
    {
        // TODO: später DB/API-Load
        Debug.Log("LoadGame: kommt später (DB/API).");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("QuitGame: funktioniert nur im Build (Editor kann nicht 'quitten').");
#else
        Application.Quit();
#endif
    }
}
