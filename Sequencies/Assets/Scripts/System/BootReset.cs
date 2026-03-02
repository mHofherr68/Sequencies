using UnityEngine;
using UnityEngine.SceneManagement;

public class BootReset : MonoBehaviour
{
    [Header("Reset on this scene name")]
    [SerializeField] private string resetOnScene = "SC_MainMenu";

    private void Awake()
    {
        // Nur im MainMenu resetten (nicht aus Versehen in jedem Level)
        if (SceneManager.GetActiveScene().name != resetOnScene)
            return;

        // Alle DontDestroyOnLoad-Roots zerstören, außer dieses BootReset-Objekt
        var ddolScene = gameObject.scene; // das ist bereits die aktive Scene
        // Trick: DontDestroyOnLoad Objekte sind in einer separaten DDOL Scene,
        // daher finden wir sie über "Resources.FindObjectsOfTypeAll".
        var all = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (var go in all)
        {
            if (go == null) continue;
            if (!go.scene.IsValid()) continue;                 // Prefab asset etc.
            if (go.scene.name != "DontDestroyOnLoad") continue; // Nur DDOL Scene
            if (go == this.gameObject) continue;

            Destroy(go);
        }
    }
}