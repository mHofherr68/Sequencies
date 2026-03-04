using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles the automatic transition from the Game Over screen to another scene
/// after a configurable delay.
/// 
/// Typical use case:
/// - Display a "Game Over" or "Winner" screen.
/// - Wait for a few seconds.
/// - Automatically return to a menu or restart scene.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("Return To Scene")]

#if UNITY_EDITOR
    [Tooltip("Scene that will be loaded after the delay (Editor only helper).")]
    [SerializeField] private SceneAsset sceneToLoad;
#endif

    [Tooltip("Name of the scene to load after the delay.")]
    [SerializeField, HideInInspector]
    private string sceneName;

    [Header("Timing")]
    [Tooltip("Time in seconds before the scene transition starts.")]
    [SerializeField] private float delay = 5f;

    [Header("Loading")]
    [Tooltip("If true, loads the scene asynchronously.")]
    [SerializeField] private bool loadAsync = true;

    private void Start()
    {
        StartCoroutine(ReturnRoutine());
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor helper that automatically copies the assigned SceneAsset
    /// name into the runtime sceneName field.
    /// </summary>
    private void OnValidate()
    {
        if (sceneToLoad != null)
            sceneName = sceneToLoad.name;
    }
#endif

    /// <summary>
    /// Waits for the configured delay and then loads the target scene.
    /// </summary>
    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(delay);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[GameOverController] No scene assigned.");
            yield break;
        }

        // Load scene immediately
        if (!loadAsync)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // Load scene asynchronously
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
            yield return null;
    }
}