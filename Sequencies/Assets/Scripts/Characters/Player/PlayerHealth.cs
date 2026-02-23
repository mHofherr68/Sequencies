/*using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("HUD")]
    [Tooltip("Optional. Wenn leer, sucht es beim Start automatisch ein HUD_HealthDisplay in der Szene.")]
    [SerializeField] private HUD_HealthDisplay hud;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    private void Start()
    {
        // Auto-find (deadline-safe)
        if (hud == null)
            hud = FindFirstObjectByType<HUD_HealthDisplay>();

        PushToHUD();
    }

    public void Damage(int amount)
    {
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);

        if (currentHP != before)
            PushToHUD();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);

        if (currentHP != before)
            PushToHUD();
    }

    public void SetHP(int newHP)
    {
        int before = currentHP;
        currentHP = Mathf.Clamp(newHP, 0, maxHP);

        if (currentHP != before)
            PushToHUD();
    }

    private void PushToHUD()
    {
        if (hud == null) return;

        // HUD bekommt HP/MaxHP -> füllt deinen Kreis in 10er Schritten (je nach HUD-Setup)
        hud.SetPlayerHP(currentHP, maxHP);
    }
}*/
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("HUD")]
    [Tooltip("Optional. Wenn leer, sucht es beim Start automatisch ein HUD_HealthDisplay in der Szene.")]
    [SerializeField] private HUD_HealthDisplay hud;

    [Header("Death -> Level Change")]
    [Tooltip("Wenn true: bei HP=0 wird eine Szene geladen.")]
    [SerializeField] private bool loadSceneOnDeath = true;

#if UNITY_EDITOR
    [Tooltip("Szene per Drag&Drop. (Build-safe: zur Laufzeit wird nur der Szenen-Name verwendet.)")]
    [SerializeField] private SceneAsset sceneToLoadOnDeath;
#endif

    [SerializeField, HideInInspector]
    private string sceneName;

    [Tooltip("Wenn true: Szene wird async geladen und nach optionalem Delay aktiviert.")]
    [SerializeField] private bool loadAsync = true;

    [Tooltip("Extra Delay bevor geladen wird (z.B. für SFX/FX).")]
    [SerializeField] private float extraDelay = 0.2f;

    private bool isDead;
    private Coroutine deathRoutine;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneToLoadOnDeath != null)
            sceneName = sceneToLoadOnDeath.name;
    }
#endif

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    private void Start()
    {
        if (hud == null)
            hud = FindFirstObjectByType<HUD_HealthDisplay>();

        PushToHUD();

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    public void Damage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);

        if (currentHP != before)
            PushToHUD();

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);

        if (currentHP != before)
            PushToHUD();
    }

    public void SetHP(int newHP)
    {
        if (isDead) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(newHP, 0, maxHP);

        if (currentHP != before)
            PushToHUD();

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    private void PushToHUD()
    {
        if (hud == null) return;
        hud.SetPlayerHP(currentHP, maxHP);
    }

    private void HandleDeathOnce()
    {
        if (isDead) return;
        isDead = true;

        if (!loadSceneOnDeath) return;

        if (deathRoutine != null) StopCoroutine(deathRoutine);
        deathRoutine = StartCoroutine(DeathLoadRoutine());
    }

    private IEnumerator DeathLoadRoutine()
    {
        if (extraDelay > 0f)
            yield return new WaitForSeconds(extraDelay);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[PlayerHealth] sceneName is empty. Assign a SceneAsset in the inspector (Editor) or set sceneName manually.");
            yield break;
        }

        if (!loadAsync)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null) yield break;

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;
    }
}

