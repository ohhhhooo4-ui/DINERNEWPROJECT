using UnityEngine;

public class TriggerParticleActivator : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        if (particles == null)
        {
            Debug.LogError("Particle System is not assigned!", this);
            return;
        }

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (particles != null)
            particles.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (particles != null)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}