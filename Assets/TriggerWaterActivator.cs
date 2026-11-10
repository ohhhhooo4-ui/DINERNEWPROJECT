using UnityEngine;

public class TriggerWaterActivator : MonoBehaviour
{
    public ParticleSystem particles;
    public string playerTag = "Player";

    private bool activated = false;

    private void Start()
    {
        if (particles != null)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (particles != null)
        {
            activated = true;
            particles.Play();
            Debug.Log("Water particles activated!");
        }
    }
}