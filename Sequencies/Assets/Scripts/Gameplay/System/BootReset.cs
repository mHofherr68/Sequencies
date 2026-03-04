using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Boot-time cleanup helper for a specific scene (usually the Main Menu).
/// 
/// Purpose:
/// - Ensures the game starts from a clean state by removing all objects that were kept alive via
///   DontDestroyOnLoad (DDOL) from previous runs / play sessions.
/// - This prevents duplicate "persistent systems" (audio, spawners, managers, etc.) after returning
///   to the Main Menu.
///
/// How it works:
/// - Only runs when the active scene name matches <see cref="resetOnScene"/>.
/// - Searches all loaded GameObjects (including hidden/internal) and destroys every object that
///   lives in the special scene "DontDestroyOnLoad", except this BootReset instance itself.
/// </summary>
public class BootReset : MonoBehaviour
{
    [Header("Reset Trigger")]
    [Tooltip("Only in this scene will DontDestroyOnLoad objects be cleared (e.g., the Main Menu scene).")]
    [SerializeField] private string resetOnScene = "SC_MainMenu";

    private void Awake()
    {
        // Only reset when we are in the configured scene (prevents accidental resets in gameplay scenes).
        if (SceneManager.GetActiveScene().name != resetOnScene)
            return;

        // DontDestroyOnLoad objects are moved into a special internal scene named "DontDestroyOnLoad".
        // We locate them via Resources.FindObjectsOfTypeAll because they do not appear through normal
        // scene traversal APIs in the usual way.
        var all = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (var go in all)
        {
            if (go == null) continue;

            // Skip assets/prefabs that are not actual scene instances.
            if (!go.scene.IsValid()) continue;

            // Only target objects inside the internal DDOL scene.
            if (go.scene.name != "DontDestroyOnLoad") continue;

            // Do not destroy this BootReset object.
            if (go == this.gameObject) continue;

            Destroy(go);
        }
    }
}