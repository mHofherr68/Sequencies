using UnityEngine;

/// <summary>
/// Initializes global runtime settings for the application.
/// 
/// Responsibilities:
/// - Ensures only one instance of this object exists (singleton-style guard).
/// - Persists across scene loads.
/// - Applies basic performance settings such as target frame rate and VSync configuration.
/// </summary>
public class GameSettings : MonoBehaviour
{
    /// <summary>
    /// Internal singleton reference to prevent multiple instances.
    /// </summary>
    private static GameSettings instance;

    private void Awake()
    {
        // Singleton guard
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Persist across scene loads
        DontDestroyOnLoad(gameObject);

        // Disable VSync and enforce a fixed frame rate
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}