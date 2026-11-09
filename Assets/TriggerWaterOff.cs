using UnityEngine;

public class TriggerWaterOff : MonoBehaviour
{
    [Header("Water Particle System")]
    public ParticleSystem waterParticles;

    private void OnTriggerEnter(Collider other)
    {
        if (waterParticles != null)
        {
            waterParticles.Stop();
        }
    }
}