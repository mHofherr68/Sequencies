using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages player health and HUD updates.
/// 
/// Responsibilities:
/// - Stores and updates player HP (damage, heal, set).
/// - Persists HP across scene loads (runtime only) using a static saved value.
/// - Updates the HUD_HealthDisplay.
/// - Optionally loads a configured scene when the player dies.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]

    [Tooltip("Maximum player HP.")]
    [SerializeField] private int maxHP = 100;

    [Tooltip("Current player HP.")]
    [SerializeField] private int currentHP = 100;

    // Persist HP across scene loads (runtime only). -1 = not initialized yet.
    private static int savedHP = -1;

    [Header("HUD")]

    [Tooltip("Reference to the player HUD health display. If empty, it will be auto-found at runtime.")]
    [SerializeField] private HUD_HealthDisplay hud;

    [Header("Death -> Level Change")]

    [Tooltip("If true, loads a new scene when the player dies.")]
    [SerializeField] private bool loadSceneOnDeath = true;

#if UNITY_EDITOR
    [Tooltip("Scene to load on death (Editor only). Writes the scene name into 'sceneName'.")]
    [SerializeField] private SceneAsset sceneToLoadOnDeath;
#endif

    [SerializeField, HideInInspector]
    private string sceneName;

    [Tooltip("If true, loads the death scene asynchronously.")]
    [SerializeField] private bool loadAsync = true;

    [Tooltip("Additional delay before the death scene load starts.")]
    [SerializeField] private float extraDelay = 0.2f;

    private bool isDead;
    private Coroutine deathRoutine;

    /// <summary>Maximum HP value.</summary>
    public int MaxHP => maxHP;

    /// <summary>Current HP value.</summary>
    public int CurrentHP => currentHP;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keep sceneName in sync with the assigned SceneAsset (Editor only).
        if (sceneToLoadOnDeath != null)
            sceneName = sceneToLoadOnDeath.name;
    }
#endif

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);

        // Restore persisted HP if available, otherwise clamp the inspector value.
        if (savedHP >= 0)
            currentHP = Mathf.Clamp(savedHP, 0, maxHP);
        else
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    private void Start()
    {
        // Auto-find HUD if not assigned.
        if (hud == null)
            hud = FindFirstObjectByType<HUD_HealthDisplay>();

        PushToHUD();

        // Safety: handle already-dead state.
        if (currentHP <= 0)
            HandleDeathOnce();
    }

    /// <summary>
    /// Applies damage to the player.
    /// Updates HUD and triggers death if HP reaches zero.
    /// </summary>
    /// <param name="amount">Damage amount (must be > 0).</param>
    public void Damage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP; // persist across scenes
            PushToHUD();
        }

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    /// <summary>
    /// Heals the player.
    /// Updates HUD and persists HP.
    /// </summary>
    /// <param name="amount">Heal amount (must be > 0).</param>
    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP; // persist across scenes
            PushToHUD();
        }
    }

    /// <summary>
    /// Sets HP directly (clamped between 0 and maxHP).
    /// Updates HUD and triggers death if HP reaches zero.
    /// </summary>
    /// <param name="newHP">New HP value.</param>
    public void SetHP(int newHP)
    {
        if (isDead) return;

        int before = currentHP;
        currentHP = Mathf.Clamp(newHP, 0, maxHP);

        if (currentHP != before)
        {
            savedHP = currentHP; // persist across scenes
            PushToHUD();
        }

        if (currentHP <= 0)
            HandleDeathOnce();
    }

    /// <summary>
    /// Resets player HP to full and updates HUD.
    /// </summary>
    public void ResetToFull()
    {
        if (isDead) return;

        currentHP = maxHP;
        savedHP = currentHP;
        PushToHUD();
    }

    /// <summary>
    /// Pushes the current HP values to the HUD.
    /// </summary>
    private void PushToHUD()
    {
        if (hud == null) return;
        hud.SetPlayerHP(currentHP, maxHP);
    }

    /// <summary>
    /// Handles death exactly once.
    /// Clears persisted HP and optionally triggers scene loading.
    /// </summary>
    private void HandleDeathOnce()
    {
        if (isDead) return;
        isDead = true;

        // Clear saved HP for next run/scene (optional design choice).
        savedHP = -1;

        if (!loadSceneOnDeath) return;

        if (deathRoutine != null) StopCoroutine(deathRoutine);
        deathRoutine = StartCoroutine(DeathLoadRoutine());
    }

    /// <summary>
    /// Loads the configured death scene (sync or async).
    /// </summary>
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