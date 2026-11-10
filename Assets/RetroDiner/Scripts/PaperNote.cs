using UnityEngine;
using Autohand;

public class PaperNote : MonoBehaviour
{
    public NotePile pile;

    private Grabbable grabbable;
    private Rigidbody rb;
    private bool hasBeenTaken;

    private void Start()
    {
        grabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (hasBeenTaken)
            return;

        if (grabbable != null && grabbable.HeldCount() > 0)
        {
            hasBeenTaken = true;

            // Spawn replacement paper immediately
            if (pile != null)
            {
                pile.PaperTaken();
            }

            // This paper is now independent from the pile
            transform.SetParent(null);
        }
    }
}