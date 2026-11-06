using UnityEngine;
using System.Collections;

public class NPC_ControlerQue: MonoBehaviour
{
    [Header("Customer Path")]
    public Transform[] approachWaypoints;

    [Header("Exit Path")]
    public Transform[] exitWaypoints;

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

    private Coroutine behaviourRoutine;
    private bool playerActivated;
    private bool isRunning;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
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

        if (animator == null)
        {
            Debug.LogError("Animator missing!", this);
            return;
        }

        playerActivated = false;
        isRunning = true;

        behaviourRoutine = StartCoroutine(BehaviourLoop());
    }

    public void ActivatePlayerTrigger()
    {
        playerActivated = true;
    }

    private IEnumerator BehaviourLoop()
    {
        // Walk through all approach waypoints
        if (approachWaypoints != null)
        {
            foreach (Transform waypoint in approachWaypoints)
            {
                if (waypoint != null)
                    yield return MoveTo(waypoint.position);
            }
        }

        // Stop walking
        animator.SetBool(walkingParameter, false);

        // Start waving loop
        animator.SetBool(waveParameter, true);

        // Wait until player interaction
        if (waitForPlayer)
        {
            yield return new WaitUntil(() => playerActivated);
        }

        // Stop waving
        animator.SetBool(waveParameter, false);

        // Thumbs up
        animator.SetTrigger(thumbsUpParameter);

        yield return new WaitForSeconds(thumbsUpDuration);

        // Walk out through exit path
        if (exitWaypoints != null)
        {
            foreach (Transform waypoint in exitWaypoints)
            {
                if (waypoint != null)
                    yield return MoveTo(waypoint.position);
            }
        }

        animator.SetBool(walkingParameter, false);

        isRunning = false;
        behaviourRoutine = null;
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        animator.SetBool(walkingParameter, true);

        while (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

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

        yield return null;
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