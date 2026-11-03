using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;

    public Transform target;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.SetDestination(target.position);
    }

    void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}