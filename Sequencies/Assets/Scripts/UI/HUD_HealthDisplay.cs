using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple HUD controller for radial (filled) health indicators.
/// 
/// Features:
/// - Supports a player HP fill and an enemy HP fill.
/// - Optional quantization (e.g., 10 steps -> 100%, 90%, 80%, ...).
/// - Optional visibility rules: hide player/enemy ring if the assigned target is null.
/// 
/// Notes:
/// - This script is "passive": it does not fetch health automatically.
///   Other systems (e.g., PlayerHealth / EnemyHealth) push values into it via SetPlayerHP / SetEnemyHP.
/// </summary>
public class HUD_HealthDisplay : MonoBehaviour
{
    [Header("UI Images (Radial Filled)")]
    [Tooltip("UI Image (Filled / Radial) representing player health.")]
    [SerializeField] private Image playerFill;

    [Tooltip("UI Image (Filled / Radial) representing enemy health.")]
    [SerializeField] private Image enemyFill;

    [Header("Quantization")]
    [Tooltip("Number of discrete steps used for display. Example: 10 -> 100%, 90%, 80%, ...")]
    [Range(1, 20)]
    [SerializeField] private int steps = 10;

    [Header("Visibility")]
    [Tooltip("Hide the enemy ring as long as no enemy target is assigned.")]
    [SerializeField] private bool hideEnemyWhenNull = true;

    [Tooltip("Hide the player ring as long as no player target is assigned (usually false).")]
    [SerializeField] private bool hidePlayerWhenNull = false;

    [Header("Optional Targets (later)")]
    [Tooltip("Optional reference used only for visibility logic (player ring).")]
    [SerializeField] private MonoBehaviour playerTarget;

    [Tooltip("Optional reference used only for visibility logic (enemy ring).")]
    [SerializeField] private MonoBehaviour enemyTarget;

    /// <summary>
    /// Cached player value in normalized range 0..1.
    /// </summary>
    private float player01 = 1f;

    /// <summary>
    /// Cached enemy value in normalized range 0..1.
    /// </summary>
    private float enemy01 = 1f;

    private void Awake()
    {
        ApplyVisibility();
        ApplyAll();
    }

    private void OnValidate()
    {
        ApplyVisibility();
        ApplyAll();
    }

    // --------------------------------------------------
    // Targets (optional, used for visibility only)
    // --------------------------------------------------

    /// <summary>
    /// Assigns a player target reference used for visibility rules.
    /// </summary>
    public void SetPlayerTarget(MonoBehaviour target)
    {
        playerTarget = target;
        ApplyVisibility();
    }

    /// <summary>
    /// Assigns an enemy target reference used for visibility rules.
    /// </summary>
    public void SetEnemyTarget(MonoBehaviour target)
    {
        enemyTarget = target;
        ApplyVisibility();
    }

    /// <summary>
    /// Clears the current enemy target reference (used for visibility rules).
    /// </summary>
    public void ClearEnemy()
    {
        enemyTarget = null;
        ApplyVisibility();
    }

    // --------------------------------------------------
    // Public API (HP)
    // --------------------------------------------------

    /// <summary>
    /// Sets player health as a normalized value (0..1).
    /// </summary>
    public void SetPlayerPercent01(float value01)
    {
        player01 = Mathf.Clamp01(value01);
        ApplyPlayer();
    }

    /// <summary>
    /// Sets enemy health as a normalized value (0..1).
    /// </summary>
    public void SetEnemyPercent01(float value01)
    {
        enemy01 = Mathf.Clamp01(value01);
        ApplyEnemy();
    }

    /// <summary>
    /// Sets player health by current/max values.
    /// </summary>
    public void SetPlayerHP(int current, int max)
    {
        SetPlayerPercent01(To01(current, max));
    }

    /// <summary>
    /// Sets enemy health by current/max values.
    /// </summary>
    public void SetEnemyHP(int current, int max)
    {
        SetEnemyPercent01(To01(current, max));
    }

    // --------------------------------------------------
    // Internals
    // --------------------------------------------------

    /// <summary>
    /// Applies configured visibility rules for player/enemy UI elements.
    /// </summary>
    private void ApplyVisibility()
    {
        if (playerFill != null && hidePlayerWhenNull)
            playerFill.gameObject.SetActive(playerTarget != null);

        if (enemyFill != null && hideEnemyWhenNull)
            enemyFill.gameObject.SetActive(enemyTarget != null);
    }

    /// <summary>
    /// Applies both cached values to the UI.
    /// </summary>
    private void ApplyAll()
    {
        ApplyPlayer();
        ApplyEnemy();
    }

    /// <summary>
    /// Applies cached player health to UI (including quantization).
    /// </summary>
    private void ApplyPlayer()
    {
        if (playerFill == null) return;
        playerFill.fillAmount = Quantize(player01);
    }

    /// <summary>
    /// Applies cached enemy health to UI (including quantization).
    /// </summary>
    private void ApplyEnemy()
    {
        if (enemyFill == null) return;
        enemyFill.fillAmount = Quantize(enemy01);
    }

    /// <summary>
    /// Quantizes a normalized value (0..1) into discrete display steps.
    /// </summary>
    private float Quantize(float t)
    {
        t = Mathf.Clamp01(t);
        if (steps <= 1) return t;

        float stepSize = 1f / steps;
        t = Mathf.Round(t / stepSize) * stepSize;
        return Mathf.Clamp01(t);
    }

    /// <summary>
    /// Converts current/max to a normalized value (0..1).
    /// </summary>
    private static float To01(int current, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Clamp01((float)current / max);
    }
}