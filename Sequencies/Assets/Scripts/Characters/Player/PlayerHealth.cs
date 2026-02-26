/*using System.Collections;
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
    [SerializeField] private HUD_HealthDisplay hud;

    [Header("Death -> Level Change")]
    [SerializeField] private bool loadSceneOnDeath = true;

#if UNITY_EDITOR

    [SerializeField] private SceneAsset sceneToLoadOnDeath;
#endif

    [SerializeField, HideInInspector]
    private string sceneName;

    [SerializeField] private bool loadAsync = true;
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

    // NEW: persist HP across scene loads (runtime only)
    // -1 means "not initialized yet"
    private static int savedHP = -1;

    [Header("HUD")]
    [SerializeField] private HUD_HealthDisplay hud;

    [Header("Death -> Level Change")]
    [SerializeField] private bool loadSceneOnDeath = true;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneToLoadOnDeath;
#endif

    [SerializeField, HideInInspector]
    private string sceneName;

    [SerializeField] private bool loadAsync = true;
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

        // NEW: restore saved HP if we already have one
        if (savedHP >= 0)
            currentHP = Mathf.Clamp(savedHP, 0, maxHP);
        else
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
        {
            savedHP = currentHP; // NEW: persist
            PushToHUD();
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
            savedHP = currentHP; // NEW: persist
            PushToHUD();
        }
    }

    public void SetHP(int newHP)
    {
        if (isDead) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(newHP, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP; // NEW: persist
            PushToHUD();
        }

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    // OPTIONAL: call this if you ever want to reset HP to full manually
    public void ResetToFull()
    {
        if (isDead) return;

        currentHP = maxHP;
        savedHP = currentHP;
        PushToHUD();
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

        // NEW: on death, clear saved HP so next run/scene can start clean (optional)
        // If you want the player to stay dead across scene loads, remove this line.
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
