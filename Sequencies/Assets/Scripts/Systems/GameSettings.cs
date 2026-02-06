/*using UnityEngine;

public class GameSettings : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }
}*/
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private static GameSettings instance;

    private void Awake()
    {
        Debug.Log("GameSettings initialized");                  //Temporär
        // Falls schon eins existiert ? dieses zerstören
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Dieses wird die einzige Instanz
        instance = this;

        // Bleibt beim Scenewechsel erhalten
        DontDestroyOnLoad(gameObject);

        // Deine Settings:
        QualitySettings.vSyncCount = 0;   // VSync aus
        Application.targetFrameRate = 60; // stabile 60 FPS
    }
}
