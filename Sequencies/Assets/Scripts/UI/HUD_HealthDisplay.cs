/*using UnityEngine;
using UnityEngine.UI;

public class HUD_HealthDisplay : MonoBehaviour
{
    [Header("UI Images (Radial Filled)")]
    [SerializeField] private Image playerFill;
    [SerializeField] private Image enemyFill;

    [Header("Quantization")]
    [Tooltip("10 = 100%, 90%, 80% ...")]
    [Range(1, 20)]
    [SerializeField] private int steps = 10;

    [Header("Empty State")]
    [Tooltip("Wenn true: Enemy-Kreis wird versteckt, solange kein Enemy zugewiesen ist.")]
    [SerializeField] private bool hideEnemyWhenNull = true;

    [Tooltip("Wenn true: Player-Kreis wird versteckt, solange kein Player zugewiesen ist.")]
    [SerializeField] private bool hidePlayerWhenNull = false;

    // Targets (optional): irgend ein Component, damit du später Player/Enemy referenzieren kannst
    // OHNE dass wir jetzt schon ein Health-System brauchen.
    [Header("Optional Targets (later)")]
    [SerializeField] private MonoBehaviour playerTarget;
    [SerializeField] private MonoBehaviour enemyTarget;

    // Intern gespeicherte Werte (0..1)
    private float player01 = 1f;
    private float enemy01 = 1f;

    private void Awake()
    {
        ApplyVisibility();
        ApplyAll();
    }

    private void OnValidate()
    {
        // Damit du im Editor nicht alles kaputt machst
        ApplyVisibility();
        ApplyAll();
    }

    // -------------------------
    // Public API (für später)
    // -------------------------

    public void SetPlayerTarget(MonoBehaviour target)
    {
        playerTarget = target;
        ApplyVisibility();
    }

    public void SetEnemyTarget(MonoBehaviour target)
    {
        enemyTarget = target;
        ApplyVisibility();
    }

    public void SetPlayerPercent01(float value01)
    {
        player01 = Mathf.Clamp01(value01);
        ApplyPlayer();
    }

    public void SetEnemyPercent01(float value01)
    {
        enemy01 = Mathf.Clamp01(value01);
        ApplyEnemy();
    }

    public void SetPlayerHP(int current, int max)
    {
        SetPlayerPercent01(MaxSafe01(current, max));
    }

    public void SetEnemyHP(int current, int max)
    {
        SetEnemyPercent01(MaxSafe01(current, max));
    }

    public void ClearEnemy()
    {
        enemyTarget = null;
        // Du kannst entscheiden: beim Clear entweder ausblenden oder auf 100% lassen
        ApplyVisibility();
    }

    // -------------------------
    // Intern
    // -------------------------

    private float Quantize(float t)
    {
        t = Mathf.Clamp01(t);
        if (steps <= 1) return t;

        float stepSize = 1f / steps;
        t = Mathf.Round(t / stepSize) * stepSize;
        return Mathf.Clamp01(t);
    }

    private static float MaxSafe01(int current, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Clamp01((float)current / max);
    }

    private void ApplyVisibility()
    {
        if (playerFill != null && hidePlayerWhenNull)
            playerFill.gameObject.SetActive(playerTarget != null);

        if (enemyFill != null && hideEnemyWhenNull)
            enemyFill.gameObject.SetActive(enemyTarget != null);
    }

    private void ApplyAll()
    {
        ApplyPlayer();
        ApplyEnemy();
    }

    private void ApplyPlayer()
    {
        if (playerFill == null) return;
        playerFill.fillAmount = Quantize(player01);
    }

    private void ApplyEnemy()
    {
        if (enemyFill == null) return;
        enemyFill.fillAmount = Quantize(enemy01);
    }
}*/
using UnityEngine;
using UnityEngine.UI;

public class HUD_HealthDisplay : MonoBehaviour
{
    [Header("UI Images (Radial Filled)")]
    [SerializeField] private Image playerFill;
    [SerializeField] private Image enemyFill;

    [Header("Quantization")]
    [Tooltip("10 = 100%, 90%, 80% ...")]
    [Range(1, 20)]
    [SerializeField] private int steps = 10;

    [Header("Visibility")]
    [Tooltip("Enemy-Kreis verstecken, solange kein Enemy gesetzt ist.")]
    [SerializeField] private bool hideEnemyWhenNull = true;

    [Tooltip("Player-Kreis verstecken, solange kein Player gesetzt ist (normalerweise false).")]
    [SerializeField] private bool hidePlayerWhenNull = false;

    [Header("Optional Targets (later)")]
    [SerializeField] private MonoBehaviour playerTarget;
    [SerializeField] private MonoBehaviour enemyTarget;

    // Stored values 0..1
    private float player01 = 1f;
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

    // -------------------------
    // Targets (optional)
    // -------------------------
    public void SetPlayerTarget(MonoBehaviour target)
    {
        playerTarget = target;
        ApplyVisibility();
    }

    public void SetEnemyTarget(MonoBehaviour target)
    {
        enemyTarget = target;
        ApplyVisibility();
    }

    public void ClearEnemy()
    {
        enemyTarget = null;
        ApplyVisibility();
    }

    // -------------------------
    // Public API (HP)
    // -------------------------
    public void SetPlayerPercent01(float value01)
    {
        player01 = Mathf.Clamp01(value01);
        ApplyPlayer();
    }

    public void SetEnemyPercent01(float value01)
    {
        enemy01 = Mathf.Clamp01(value01);
        ApplyEnemy();
    }

    public void SetPlayerHP(int current, int max)
    {
        SetPlayerPercent01(To01(current, max));
    }

    public void SetEnemyHP(int current, int max)
    {
        SetEnemyPercent01(To01(current, max));
    }

    // -------------------------
    // Internals
    // -------------------------
    private void ApplyVisibility()
    {
        if (playerFill != null && hidePlayerWhenNull)
            playerFill.gameObject.SetActive(playerTarget != null);

        if (enemyFill != null && hideEnemyWhenNull)
            enemyFill.gameObject.SetActive(enemyTarget != null);
    }

    private void ApplyAll()
    {
        ApplyPlayer();
        ApplyEnemy();
    }

    private void ApplyPlayer()
    {
        if (playerFill == null) return;
        playerFill.fillAmount = Quantize(player01);
    }

    private void ApplyEnemy()
    {
        if (enemyFill == null) return;
        enemyFill.fillAmount = Quantize(enemy01);
    }

    private float Quantize(float t)
    {
        t = Mathf.Clamp01(t);
        if (steps <= 1) return t;

        float stepSize = 1f / steps;
        t = Mathf.Round(t / stepSize) * stepSize;
        return Mathf.Clamp01(t);
    }

    private static float To01(int current, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Clamp01((float)current / max);
    }
}
