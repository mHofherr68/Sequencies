using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Door interaction trigger:
/// - Shows a prompt when the player is inside the trigger.
/// - On key press: freezes player movement, plays door SFX, optionally spawns a noise light,
///   plays door open animation, optionally triggers an explosion, then loads the next scene.
/// - If the door is configured as an "explosion door", it can:
///   - set the player's HP UI fill to 0 (UI-only),
///   - despawn the player after a delay,
///   - play an explosion sound and spawn an explosion prefab.
/// </summary>
public class DoorTrigger : MonoBehaviour
{
    [Header("Door")]
    [Tooltip("Animator controlling the door animations (typically on the parent). If not assigned, it will be auto-resolved from the parent.")]
    [SerializeField] private Animator animator;

    [Tooltip("Scene name to load after the door finishes opening (and optional delays).")]
    [SerializeField] private string sceneToLoad = "SC_Level_02";

    [Tooltip("Additional delay before scene loading (e.g., to let effects finish playing).")]
    [SerializeField] private float extraDelay = 0.2f;

    [Header("Prompt UI (InventoryMessage -> MessageText)")]
    [Tooltip("TMP text element used for showing the interaction prompt.")]
    [SerializeField] private TMP_Text messageText;

    [Tooltip("Prompt text displayed while the player is inside the trigger.")]
    [SerializeField] private string promptText = "Tür öffnen? \"E\"";

    [Tooltip("If true, the text is cleared when hiding the prompt.")]
    [SerializeField] private bool clearTextOnExit = true;

    [Header("Input (New Input System)")]
    [Tooltip("Key required to interact with the door.")]
    [SerializeField] private Key interactKey = Key.E;

    [Header("Audio")]
    [Tooltip("Sound key sent to FX_SoundSystem when the door interaction starts.")]
    [SerializeField] private string doorSoundKey = "Door";

    [Tooltip("Sound key sent to FX_SoundSystem when the explosion is triggered.")]
    [SerializeField] private string explosionSoundKey = "Explosion";

    [Header("Noise Light")]
    [Tooltip("Optional prefab (e.g., Point Light 2D + lifetime script). Spawned at the player position when the door is used.")]
    [SerializeField] private GameObject noiseLightPrefab;

    [Header("Async Loading")]
    [Tooltip("If true, loads the next scene asynchronously.")]
    [SerializeField] private bool loadAsync = true;

    [Header("Explosion (optional)")]
    [Tooltip("If true, this door triggers an explosion sequence (explosion prefab + SFX + optional player despawn + UI HP fill set to 0).")]
    [SerializeField] private bool triggerExplosion = false;

    [Tooltip("Explosion prefab to spawn when triggerExplosion is enabled.")]
    [SerializeField] private GameObject explosionPrefab;

    [Tooltip("Explosion spawn offset relative to the player position.")]
    [SerializeField] private Vector3 explosionOffset = new Vector3(-0.5f, 0f, 0f);

    [Header("Player Despawn")]
    [Tooltip("Delay before despawning the player. Only used when triggerExplosion is enabled.")]
    [SerializeField] private float playerDespawnDelay = 1.5f;

    [Header("Player HP UI")]
    [Tooltip("Optional UI Image fill (HP_Player). If assigned and triggerExplosion is enabled, fillAmount will be set to 0.")]
    [SerializeField] private Image hpPlayerFill;

    private bool playerInside = false;
    private bool triggered = false;

    // Cached reference while player is in the trigger (avoid repeated lookups)
    private PlayerController cachedPlayer;

    private void Awake()
    {
        // Auto-resolve animator on parent if not set
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    private void Update()
    {
        // Only allow one interaction
        if (triggered) return;

        // Only respond when player is inside the trigger
        if (!playerInside) return;

        // New Input System keyboard guard
        if (Keyboard.current == null) return;

        KeyControl key = Keyboard.current[interactKey];
        if (key != null && key.wasPressedThisFrame)
        {
            triggered = true;

            // Freeze player movement immediately
            if (cachedPlayer != null)
                cachedPlayer.SetMovementEnabled(false);

            HidePrompt();
            StartCoroutine(OpenAndLoad());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // Works even if collider is on a child (e.g., hurtbox)
        cachedPlayer = other.GetComponentInParent<PlayerController>();

        ShowPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (!triggered)
            HidePrompt();

        cachedPlayer = null;
    }

    private void ShowPrompt()
    {
        if (messageText == null) return;

        messageText.gameObject.SetActive(true);
        messageText.text = promptText;
    }

    private void HidePrompt()
    {
        if (messageText == null) return;

        if (clearTextOnExit)
            messageText.text = string.Empty;

        messageText.gameObject.SetActive(false);
    }

    private IEnumerator OpenAndLoad()
    {
        if (animator == null)
            yield break;

        // Door sound
        if (!string.IsNullOrEmpty(doorSoundKey))
            FX_SoundSystem.I?.PlayHit(doorSoundKey, firstHit: true);

        // Spawn noise light at player position (optional)
        if (noiseLightPrefab != null && cachedPlayer != null)
            Instantiate(noiseLightPrefab, cachedPlayer.transform.position, Quaternion.identity);

        // Trigger door open animation
        animator.SetTrigger("Open");

        // Wait until we are in the correct open animation state
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Door_01Opens")
        );

        // Wait until the open animation has finished
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        // Optional explosion (with SFX)
        TrySpawnExplosion();

        // Explosion door: set HP UI fill to 0 (UI-only)
        if (triggerExplosion && hpPlayerFill != null)
            hpPlayerFill.fillAmount = 0f;

        // Only despawn player for explosion doors
        if (triggerExplosion)
        {
            if (playerDespawnDelay > 0f)
                yield return new WaitForSeconds(playerDespawnDelay);

            if (cachedPlayer != null)
            {
                Destroy(cachedPlayer.gameObject);
                cachedPlayer = null;
            }
        }

        // Extra delay before loading scene (optional)
        if (extraDelay > 0f)
            yield return new WaitForSeconds(extraDelay);

        // Load next scene
        if (!loadAsync)
        {
            SceneManager.LoadScene(sceneToLoad);
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        if (op == null) yield break;

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;
    }

    /// <summary>
    /// Spawns the explosion prefab and plays the explosion sound.
    /// Only runs when triggerExplosion is enabled.
    /// </summary>
    private void TrySpawnExplosion()
    {
        if (!triggerExplosion) return;
        if (explosionPrefab == null) return;
        if (cachedPlayer == null) return;

        if (!string.IsNullOrEmpty(explosionSoundKey))
            FX_SoundSystem.I?.PlayHit(explosionSoundKey, firstHit: true);

        Vector3 spawnPos = cachedPlayer.transform.position + explosionOffset;
        Instantiate(explosionPrefab, spawnPos, Quaternion.identity);
    }
}