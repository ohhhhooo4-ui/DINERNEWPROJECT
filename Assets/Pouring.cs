
using UnityEngine;

public class Pouring : MonoBehaviour
{
    [Header("Pour Settings")]
    public ParticleSystem pouringParticles;
    public float pourAngle = 45f;

    private bool isPouring = false;

    void Start()
    {
        if (pouringParticles != null)
        {
            pouringParticles.Stop();
        }
    }

    void Update()
    {
        // Get the bottle's up direction in world space
        Vector3 bottleUp = transform.up;

        // Compare it to the world's up direction
        float angle = Vector3.Angle(bottleUp, Vector3.up);

        bool shouldPour = angle > pourAngle;

        if (shouldPour && !isPouring)
        {
            isPouring = true;
            pouringParticles.Play();
        }
        else if (!shouldPour && isPouring)
        {
            isPouring = false;
            pouringParticles.Stop();
        }
    }
}
