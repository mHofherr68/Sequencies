/*using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    // Persist across scene loads (runtime only). -1 = not initialized yet.
    private static int savedHP = -1;

    [Header("Arrive Damage")]
    [Tooltip("Wieviel HP der Ghost verliert, wenn er ein Target erreicht (z.B. 10 bei maxHP=100 => -10%).")]
    [SerializeField] private int arriveDamage = 10;

    [Header("HUD (auto-find by name)")]
    [Tooltip("Optional. Wenn leer, sucht er automatisch ein UI Image namens 'HP_Enemys'.")]
    [SerializeField] private Image hpEnemysFill;

    [Header("Death -> Winner Scene")]
    [SerializeField] private bool loadSceneOnDeath = true;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneToLoadOnDeath;
#endif

    [SerializeField, HideInInspector]
    private string sceneName = "Winner";

    [SerializeField] private bool loadAsync = true;
    [SerializeField] private float extraDelay = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

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

        if (savedHP >= 0)
            currentHP = Mathf.Clamp(savedHP, 0, maxHP);
        else
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    private void Start()
    {
        // Auto-find UI fill once (ghost is prefab instance)
        if (hpEnemysFill == null)
            hpEnemysFill = FindHpEnemysFill();

        PushToHUD();

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    // ------------------------------------------------------
    // Public API
    // ------------------------------------------------------
    public void ApplyArriveDamage()
    {
        if (arriveDamage <= 0) return;
        Damage(arriveDamage);
    }

    public void Damage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP;
            PushToHUD();

            if (debugLog)
                Debug.Log($"[EnemyHealth] Damage {amount}: {before} -> {currentHP}", this);
        }

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
        {
            savedHP = currentHP;
            PushToHUD();

            if (debugLog)
                Debug.Log($"[EnemyHealth] Heal {amount}: {before} -> {currentHP}", this);
        }
    }

    public void SetHP(int newHP)
    {
        if (isDead) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(newHP, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP;
            PushToHUD();

            if (debugLog)
                Debug.Log($"[EnemyHealth] SetHP: {before} -> {currentHP}", this);
        }

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    public void ResetToFull()
    {
        if (isDead) return;

        currentHP = maxHP;
        savedHP = currentHP;
        PushToHUD();
    }

    public static void ClearSavedHP()
    {
        savedHP = -1;
    }

    // ------------------------------------------------------
    // HUD
    // ------------------------------------------------------
    private void PushToHUD()
    {
        if (hpEnemysFill == null) return;
        hpEnemysFill.fillAmount = (maxHP <= 0) ? 0f : (float)currentHP / maxHP;
    }

    private Image FindHpEnemysFill()
    {
        // One-time scan, cheap enough
        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == "HP_Enemys")
                return images[i];
        }

        Debug.LogWarning("[EnemyHealth] UI Image 'HP_Enemys' nicht gefunden. Bitte UI-Objekt exakt so benennen oder hpEnemysFill im Inspector setzen.");
        return null;
    }

    // ------------------------------------------------------
    // Death -> Winner scene
    // ------------------------------------------------------
    private void HandleDeathOnce()
    {
        if (isDead) return;
        isDead = true;

        // clear for next run
        savedHP = -1;

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
            Debug.LogWarning("[EnemyHealth] sceneName is empty. Set it to 'Winner' or assign a SceneAsset in the inspector (Editor).");
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
}*/
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    // Persist across scene loads (runtime only). -1 = not initialized yet.
    private static int savedHP = -1;
    private static int savedMaxHP = 100;

    [Header("Arrive Damage")]
    [Tooltip("Wieviel HP der Ghost verliert, wenn er ein Target erreicht (z.B. 10 bei maxHP=100 => -10%).")]
    [SerializeField] private int arriveDamage = 10;

    [Header("HUD (auto-find by name)")]
    [Tooltip("Optional. Wenn leer, sucht er automatisch ein UI Image namens 'HP_Enemys'.")]
    [SerializeField] private Image hpEnemysFill;

    [Header("Death -> Winner Scene")]
    [SerializeField] private bool loadSceneOnDeath = true;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneToLoadOnDeath;
#endif

    [SerializeField, HideInInspector]
    private string sceneName = "Winner";

    [SerializeField] private bool loadAsync = true;
    [SerializeField] private float extraDelay = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private bool isDead;
    private Coroutine deathRoutine;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;

    // ---- Static access for scene-start UI bootstrap ----
    public static int SavedHP => savedHP;
    public static int SavedMaxHP => Mathf.Max(1, savedMaxHP);

    public static void PushSavedToUIIfPresent()
    {
        // If not initialized yet, we intentionally do nothing
        if (savedHP < 0) return;

        Image fill = FindHpEnemysFillStatic();
        if (fill == null) return;

        float f = (float)Mathf.Clamp(savedHP, 0, SavedMaxHP) / SavedMaxHP;
        fill.fillAmount = f;
    }

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

        // keep maxHP globally known (for UI boot in scenes without ghost)
        savedMaxHP = maxHP;

        if (savedHP >= 0)
            currentHP = Mathf.Clamp(savedHP, 0, maxHP);
        else
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    private void Start()
    {
        if (hpEnemysFill == null)
            hpEnemysFill = FindHpEnemysFillInstance();

        PushToHUD();

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    // ------------------------------------------------------
    // Public API
    // ------------------------------------------------------
    public void ApplyArriveDamage()
    {
        if (arriveDamage <= 0) return;
        Damage(arriveDamage);
    }

    public void Damage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP;
            savedMaxHP = maxHP;
            PushToHUD();

            if (debugLog)
                Debug.Log($"[EnemyHealth] Damage {amount}: {before} -> {currentHP}", this);
        }

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
        {
            savedHP = currentHP;
            savedMaxHP = maxHP;
            PushToHUD();

            if (debugLog)
                Debug.Log($"[EnemyHealth] Heal {amount}: {before} -> {currentHP}", this);
        }
    }

    public void SetHP(int newHP)
    {
        if (isDead) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(newHP, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP;
            savedMaxHP = maxHP;
            PushToHUD();

            if (debugLog)
                Debug.Log($"[EnemyHealth] SetHP: {before} -> {currentHP}", this);
        }

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    public void ResetToFull()
    {
        if (isDead) return;

        currentHP = maxHP;
        savedHP = currentHP;
        savedMaxHP = maxHP;
        PushToHUD();
    }

    public static void ClearSavedHP()
    {
        savedHP = -1;
    }

    // ------------------------------------------------------
    // HUD
    // ------------------------------------------------------
    private void PushToHUD()
    {
        if (hpEnemysFill == null) return;
        hpEnemysFill.fillAmount = (maxHP <= 0) ? 0f : (float)currentHP / maxHP;
    }

    private Image FindHpEnemysFillInstance()
    {
        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == "HP_Enemys")
                return images[i];
        }

        Debug.LogWarning("[EnemyHealth] UI Image 'HP_Enemys' nicht gefunden. Bitte UI-Objekt exakt so benennen oder hpEnemysFill im Inspector setzen.");
        return null;
    }

    private static Image FindHpEnemysFillStatic()
    {
        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == "HP_Enemys")
                return images[i];
        }
        return null;
    }

    // ------------------------------------------------------
    // Death -> Winner scene
    // ------------------------------------------------------
    private void HandleDeathOnce()
    {
        if (isDead) return;
        isDead = true;

        // clear for next run
        savedHP = -1;

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
            Debug.LogWarning("[EnemyHealth] sceneName is empty. Set it to 'Winner' or assign a SceneAsset in the inspector (Editor).");
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