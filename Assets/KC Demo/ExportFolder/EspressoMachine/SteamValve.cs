using UnityEngine;

public class SteamValve : MonoBehaviour
{
    public HingeJoint hinge;

    public ParticleSystem steamParticles;

    public AudioSource steamAudio;

    public float activationAngle = -30f;

    private bool steamOn;

    private void Update()
    {
        float angle = hinge.angle;

        if (angle <= activationAngle && !steamOn)
        {
            StartSteam();
        }

        if (angle > activationAngle && steamOn)
        {
            StopSteam();
        }
    }

    private void StartSteam()
    {
        steamOn = true;

        if (steamParticles != null)
            steamParticles.Play();

        if (steamAudio != null)
            steamAudio.Play();

        Debug.Log("Steam ON");
    }

    private void StopSteam()
    {
        steamOn = false;

        if (steamParticles != null)
            steamParticles.Stop();

        if (steamAudio != null)
            steamAudio.Stop();

        Debug.Log("Steam OFF");
    }
}
