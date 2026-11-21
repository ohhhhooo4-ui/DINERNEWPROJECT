using UnityEngine;
using Autohand;

public class CounterPlateSnap : MonoBehaviour
{
    [Header("Plate Settings")]
    public string plateTag = "Plate";

    [Header("Effects")]
    public ParticleSystem confettiEffect;

    [Header("Snap Settings")]
    public bool disablePlateGrabAfterSnap = true;

    private bool occupied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (occupied)
            return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        GameObject plate = rb.gameObject;

        if (!plate.CompareTag(plateTag))
            return;

        // Stop movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Snap plate to this counter snap point
        plate.transform.position = transform.position;
        plate.transform.rotation = transform.rotation;

        // Parent plate to counter
        plate.transform.SetParent(transform);

        // Disable grabbing if desired
        if (disablePlateGrabAfterSnap)
        {
            Grabbable grab = plate.GetComponent<Grabbable>();

            if (grab != null)
                grab.enabled = false;
        }

        // Play confetti AFTER snapping
        if (confettiEffect != null)
        {
            confettiEffect.Clear();
            confettiEffect.Play();
        }

        occupied = true;

        Debug.Log("Plate snapped to counter and confetti played!");
    }
}