using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu controller handling basic UI actions:
/// - Start a new game (load the first level).
/// - Placeholder for a future load system.
/// - Quit the application.
/// 
/// Intended to be connected to UI Buttons in the Main Menu.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [Tooltip("Scene name of the first level loaded when starting a new game.")]
    [SerializeField] private string level01SceneName = "SC_Level_01";

    /// <summary>
    /// Starts a new game by loading the configured level scene.
    /// </summary>
    public void NewGame()
    {
        SceneManager.LoadScene(level01SceneName);
    }

    /// <summary>
    /// Placeholder for a future load-game system.
    /// </summary>
    public void LoadGame()
    {
        // Planned: load save data from database / API.
        Debug.Log("LoadGame: coming later (DB/API).");
    }

    /// <summary>
    /// Quits the application.
    /// 
    /// Note:
    /// - In the Unity Editor this only logs a message,
    ///   because the editor cannot be closed programmatically.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("QuitGame: works only in a build (Editor cannot quit).");
#else
        Application.Quit();
#endif
    }
}