
using UnityEngine;

public class BunSliceOnKnife : MonoBehaviour
{
    [Header("Sliced Bun Version")]
    public GameObject slicedBun;

    [Header("Cut Requirements")]
    public string knifeTag = "Knife";
    public float requiredCutSpeed = 1.0f;

    private bool sliced = false;

    private void OnTriggerEnter(Collider other)
    {
        if (sliced)
            return;

        if (!other.CompareTag(knifeTag))
            return;

        Rigidbody knifeRb = other.attachedRigidbody;

        if (knifeRb == null)
            return;

        // Check knife movement speed
        float knifeSpeed = knifeRb.linearVelocity.magnitude;

        if (knifeSpeed < requiredCutSpeed)
            return;

        SliceBun();
    }

    private void SliceBun()
    {
        if (slicedBun == null)
        {
            Debug.LogWarning("Sliced Bun prefab is missing!");
            return;
        }

        sliced = true;

        // Spawn sliced bun in the same position and rotation
        Instantiate(
            slicedBun,
            transform.position,
            transform.rotation
        );

        // Remove original bun
        Destroy(gameObject);
    }
}