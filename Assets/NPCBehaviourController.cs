
using UnityEngine;
using System.Collections;

public class NPCBehaviourController : MonoBehaviour
{
    [Header("Destination")]
    public Transform destination;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float rotationSpeed = 5f;
    public float stoppingDistance = 0.15f;

    [Header("Animation")]
    public Animator animator;
    public string walkingParameter = "IsWalking";
    public string waveParameter = "Wave";
    public string thumbsUpParameter = "ThumbsUp";

    [Header("Interaction")]
    public bool waitForPlayer = true;
    public float thumbsUpDuration = 2f;

    [Header("Startup")]
    public bool startAutomatically = true;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private Coroutine behaviourRoutine;

    private bool isRunning;
    private bool playerActivated;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    private void Start()
    {
        if (startAutomatically)
            StartBehaviour();
    }

    public void StartBehaviour()
    {
        if (isRunning)
            return;

        if (destination == null || animator == null)
        {
            Debug.LogError("NPC is missing its destination or Animator!", this);
            return;
        }

        playerActivated = false;
        isRunning = true;

        behaviourRoutine = StartCoroutine(BehaviourLoop());
    }

    public void ActivatePlayerTrigger()
    {
        if (!isRunning || playerActivated)
            return;

        playerActivated = true;
    }

    private IEnumerator BehaviourLoop()
    {
        // 1. Walk to destination
        yield return MoveTo(destination.position);

        // 2. Face the player-facing direction
        animator.SetBool(walkingParameter, false);

        // 3. Keep waving until player activates trigger
        animator.ResetTrigger(waveParameter);
        animator.SetTrigger(waveParameter);

        while (waitForPlayer && !playerActivated)
        {
            // Wait for current wave animation to finish
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).IsName("Wave")
            );

            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
            );

            if (playerActivated)
                break;

            // Wave again
            animator.SetTrigger(waveParameter);

            yield return null;
        }

        // 4. Play thumbs up
        animator.ResetTrigger(waveParameter);
        animator.SetTrigger(thumbsUpParameter);

        yield return new WaitForSeconds(thumbsUpDuration);

        // 5. Walk back to starting position
        yield return MoveTo(startingPosition);

        animator.SetBool(walkingParameter, false);

        transform.rotation = startingRotation;

        isRunning = false;
        behaviourRoutine = null;
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        animator.SetBool(walkingParameter, true);

        while (Vector3.Distance(transform.position, targetPosition)
               > stoppingDistance)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                walkSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
        animator.SetBool(walkingParameter, false);
    }

    private void OnDisable()
    {
        if (behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
            behaviourRoutine = null;
        }

        isRunning = false;
    }
}
