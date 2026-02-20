/*using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Door01Trigger : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string sceneToLoad = "SC_Level_02";
    [SerializeField] private float extraDelay = 0.2f;

    private bool triggered = false;

    private void Reset()
    {
        animator = GetComponent<Animator>();
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
        animator.SetTrigger("Open");

        // Warten bis Open-State aktiv ist
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Door_01Opens")
        );

        // Warten bis Animation fertig (normalizedTime >= 1)
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        yield return new WaitForSeconds(extraDelay);

        SceneManager.LoadScene(sceneToLoad);
    }
}*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;                 // leer lassen -> wird automatisch gefunden
    [SerializeField] private string sceneToLoad = "SC_Level_02";
    [SerializeField] private float extraDelay = 0.2f;

    private bool triggered = false;

    private void Awake()
    {
        // Selbstheilung: Script hängt typischerweise am Child-Trigger, Animator sitzt am Parent "Door"
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

        // Warten bis Open-State aktiv ist
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Door_01Opens")
        );

        // Warten bis Animation fertig (normalizedTime >= 1)
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        yield return new WaitForSeconds(extraDelay);

        SceneManager.LoadScene(sceneToLoad);
    }
}
