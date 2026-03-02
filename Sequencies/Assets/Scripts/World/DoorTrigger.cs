/*using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Animator animator;
    [SerializeField] private string sceneToLoad = "SC_Level_02";
    [SerializeField] private float extraDelay = 0.2f;

    [Header("Prompt UI (InventoryMessage -> MessageText)")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private string promptText = "Tür öffnen? \"E\"";
    [SerializeField] private bool clearTextOnExit = true;

    [Header("Input (New Input System)")]
    [SerializeField] private Key interactKey = Key.E;

    [Header("Audio")]
    [SerializeField] private string doorSoundKey = "Door";

    [Header("Async Loading")]
    [SerializeField] private bool loadAsync = true;

    [Header("Explosion (optional)")]
    [SerializeField] private bool triggerExplosion = false;
    [SerializeField] private GameObject explosionPrefab;
    [Tooltip("Position relativ zum Player. Hinter dem Player z.B. X negativ.")]
    [SerializeField] private Vector3 explosionOffset = new Vector3(-0.5f, 0f, 0f);

    private bool playerInside = false;
    private bool triggered = false;

    // cached player controller while inside trigger (no Find spam)
    private PlayerController cachedPlayer;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    private void Update()
    {
        if (triggered) return;
        if (!playerInside) return;

        if (Keyboard.current == null) return;

        KeyControl key = Keyboard.current[interactKey];
        if (key != null && key.wasPressedThisFrame)
        {
            triggered = true;

            // freeze player movement immediately
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

        // Cache PlayerController (works with HurtBox child colliders too)
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

        // play door sound at the moment the animation is triggered
        if (!string.IsNullOrEmpty(doorSoundKey))
            FX_SoundSystem.I?.PlayHit(doorSoundKey, firstHit: true);

        animator.SetTrigger("Open");

        // wait until we are actually in the open animation state
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Door_01Opens")
        );

        // wait until open animation finished
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        // Door is fully open -> optional explosion
        TrySpawnExplosion();

        // delay so explosion can be seen (or for timing)
        if (extraDelay > 0f)
            yield return new WaitForSeconds(extraDelay);

        // load scene
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

    private void TrySpawnExplosion()
    {
        if (!triggerExplosion) return;
        if (explosionPrefab == null) return;
        if (cachedPlayer == null) return;

        Vector3 spawnPos = cachedPlayer.transform.position + explosionOffset;
        Instantiate(explosionPrefab, spawnPos, Quaternion.identity);
    }
}*/
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Animator animator;
    [SerializeField] private string sceneToLoad = "SC_Level_02";
    [SerializeField] private float extraDelay = 0.2f;

    [Header("Prompt UI (InventoryMessage -> MessageText)")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private string promptText = "Tür öffnen? \"E\"";
    [SerializeField] private bool clearTextOnExit = true;

    [Header("Input (New Input System)")]
    [SerializeField] private Key interactKey = Key.E;

    [Header("Audio")]
    [SerializeField] private string doorSoundKey = "Door";
    [SerializeField] private string explosionSoundKey = "Explosion";

    [Header("Async Loading")]
    [SerializeField] private bool loadAsync = true;

    [Header("Explosion (optional)")]
    [SerializeField] private bool triggerExplosion = false;
    [SerializeField] private GameObject explosionPrefab;
    [Tooltip("Position relativ zum Player. Hinter dem Player z.B. X negativ.")]
    [SerializeField] private Vector3 explosionOffset = new Vector3(-0.5f, 0f, 0f);

    [Header("Player Despawn")]
    [SerializeField] private float playerDespawnDelay = 1.5f;

    private bool playerInside = false;
    private bool triggered = false;

    private PlayerController cachedPlayer;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    private void Update()
    {
        if (triggered) return;
        if (!playerInside) return;

        if (Keyboard.current == null) return;

        KeyControl key = Keyboard.current[interactKey];
        if (key != null && key.wasPressedThisFrame)
        {
            triggered = true;

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

        animator.SetTrigger("Open");

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Door_01Opens")
        );

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        // Explosion + sound
        TrySpawnExplosion();

        // Keep player visible for a fixed moment, then despawn
        if (playerDespawnDelay > 0f)
            yield return new WaitForSeconds(playerDespawnDelay);

        if (cachedPlayer != null)
        {
            Destroy(cachedPlayer.gameObject);
            cachedPlayer = null;
        }

        // Remaining delay before loading the new scene
        if (extraDelay > 0f)
            yield return new WaitForSeconds(extraDelay);

        // Scene load
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
