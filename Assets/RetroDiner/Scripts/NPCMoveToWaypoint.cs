using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCMoveToWaypoint : MonoBehaviour
{
    public Transform waypoint;

    private NavMeshAgent agent;
    private Animator anim;
    private bool arrived;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (waypoint != null)
            agent.SetDestination(waypoint.position);
    }

    void Update()
    {
        if (arrived)
            return;

        // Update walking animation
        if (anim != null)
            anim.SetFloat("Speed", agent.velocity.magnitude);

        // Stop when destination reached
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f))
        {
            arrived = true;
            agent.isStopped = true;

            if (anim != null)
                anim.SetFloat("Speed", 0f);
        }
    }
}