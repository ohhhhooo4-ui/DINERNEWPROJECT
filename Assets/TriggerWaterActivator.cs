using UnityEngine;

public class TriggerParticleActivator : MonoBehaviour
{
    public ParticleSystem particles;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            particles.Play();
        }
    }
}