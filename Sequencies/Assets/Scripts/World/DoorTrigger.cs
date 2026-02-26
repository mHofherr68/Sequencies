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

    [Header("Async Loading")]
    [SerializeField] private bool loadAsync = true;

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

            // freeze player movement immediately (before animation/loading)
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

        // Play door sound at the moment the animation is triggered
        if (!string.IsNullOrEmpty(doorSoundKey))
            FX_SoundSystem.I?.PlayHit(doorSoundKey, firstHit: true);

        animator.SetTrigger("Open");

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Door_01Opens")
        );

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        yield return new WaitForSeconds(extraDelay);

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
}
