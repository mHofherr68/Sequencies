using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameOverController : MonoBehaviour
{
    [Header("Return To Scene")]

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneToLoad;
#endif

    [SerializeField, HideInInspector]
    private string sceneName;

    [Header("Timing")]
    [SerializeField] private float delay = 5f;

    [Header("Loading")]
    [SerializeField] private bool loadAsync = true;

    private void Start()
    {
        StartCoroutine(ReturnRoutine());
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneToLoad != null)
            sceneName = sceneToLoad.name;
    }
#endif

    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(delay);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[GameOverController] No scene assigned.");
            yield break;
        }

        if (!loadAsync)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
            yield return null;
    }
}