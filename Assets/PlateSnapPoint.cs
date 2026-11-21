using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class PlateSnapPoint : MonoBehaviour
{
    [Header("Accepted Tags")]
    public List<string> acceptedTags = new List<string>();

    [Header("Effects")]
    public ParticleSystem placeEffect;

    private bool occupied = false;

    private void OnTriggerEnter(Collider other)
    {
        // Ignore hand colliders
        if (other.CompareTag("Hands"))
            return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        GameObject targetObject = rb.gameObject;

        Debug.Log("Plate detected: " + targetObject.name);

        if (occupied)
            return;

        if (!acceptedTags.Contains(targetObject.tag))
        {
            Debug.Log("Tag not accepted: " + targetObject.tag);
            return;
        }

        // Disable grabbing on the food only
        Grabbable grab = targetObject.GetComponent<Grabbable>();
        if (grab != null)
            grab.enabled = false;

        // Stop physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Snap to plate position
        targetObject.transform.position = transform.position;

        // Copy plate rotation
        targetObject.transform.rotation = transform.rotation;

        // Parent while keeping snapped position/rotation
        targetObject.transform.SetParent(transform);

        // Play effect
        if (placeEffect != null)
        {
            placeEffect.Clear();
            placeEffect.Play();
            Debug.Log("Place effect played");
        }
        else
        {
            Debug.LogWarning("Place Effect is not assigned!");
        }

        occupied = true;
    }
}