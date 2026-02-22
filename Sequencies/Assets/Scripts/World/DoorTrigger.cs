/*using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string sceneToLoad = "SC_Level_02";
    [SerializeField] private float extraDelay = 0.2f;

    [Header("Async Loading")]
    [Tooltip("Wenn true: Szene wird im Hintergrund geladen und nach der Türanimation aktiviert.")]
    [SerializeField] private bool loadAsync = true;

    private bool triggered = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(OpenAndLoad());
    }

    private IEnumerator OpenAndLoad()
    {
        if (animator == null)
            yield break;

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
}*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string sceneToLoad = "SC_Level_02";
    [SerializeField] private float extraDelay = 0.2f;

    [Header("Audio")]
    [Tooltip("Sound-Key für FX_SoundSystem (z.B. 'Door').")]
    [SerializeField] private string doorSoundKey = "Door";

    [Header("Async Loading")]
    [Tooltip("Wenn true: Szene wird im Hintergrund geladen und nach der Türanimation aktiviert.")]
    [SerializeField] private bool loadAsync = true;

    private bool triggered = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(OpenAndLoad());
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
