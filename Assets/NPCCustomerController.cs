using System.Collections;
using UnityEngine;

public class NPCCustomerController : MonoBehaviour
{
    [Header("Path To Counter")]
    public Transform[] counterWaypoints;

    [Header("Path Away")]
    public Transform[] exitWaypoints;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 8f;
    public float stoppingDistance = 0.15f;

    [Header("References")]
    public Animator animator;

    private bool hasBeenServed;
    private bool running;

    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        StartCustomer();
    }

    public void StartCustomer()
    {
        if (running)
            return;

        running = true;
        StartCoroutine(CustomerRoutine());
    }

    public void ServeCustomer()
    {
        hasBeenServed = true;
    }

    private IEnumerator CustomerRoutine()
    {
        // Walk to counter
        animator.Play("WalkingWithoutBag");

        foreach (Transform point in counterWaypoints)
        {
            yield return MoveTo(point.position);
        }

        // Start ordering loop
        animator.Play("StandingOrderNoBag");

        // Wait until player serves customer
        yield return new WaitUntil(() => hasBeenServed);

        // Thumbs up
        animator.Play("StandThumbsUp");

        yield return new WaitForSeconds(2f);

        // Leave shop
        animator.Play("WalkingWithoutBag 0");

        foreach (Transform point in exitWaypoints)
        {
            yield return MoveTo(point.position);
        }

        animator.Play("Idle 0");

        // Optional
        Destroy(gameObject, 2f);
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.magnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);

            yield return null;
        }
    }
}