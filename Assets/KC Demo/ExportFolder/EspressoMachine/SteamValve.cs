using UnityEngine;

public class SteamValve : MonoBehaviour
{
    public HingeJoint hinge;

    public ParticleSystem steamParticles;

    public ParticleSystem pitcherSteamParticles;

    public AudioSource steamAudio;

    public SteamDetectionZone steamZone;

    public float activationAngle = -30f;

    public float heatingRate = 6f;

    private bool steamOn;

    private void Update()
    {
        float angle = hinge.angle;

        if (steamZone.currentPitcher != null)
        {
            Debug.Log("Pitcher Detected");
        }
        else
        {
            Debug.Log("No Pitcher");
        }

        if (angle <= activationAngle && !steamOn)
        {
            StartSteam();
        }

        if (angle > activationAngle && steamOn)
        {
            StopSteam();
        }

        if (steamOn)
        {
            if (steamZone.currentPitcher != null)
            {
                if (steamParticles.isPlaying)
                    steamParticles.Stop();

                if (!pitcherSteamParticles.isPlaying)
                    pitcherSteamParticles.Play();

                steamZone.currentPitcher.milkTemperature +=
                heatingRate * Time.deltaTime;

                if (steamZone.currentPitcher.milkTemperature > 75f)
                {
                    steamZone.currentPitcher.milkBurnt = true;
                }
            }
            else
            {
                if (!steamParticles.isPlaying)
                    steamParticles.Play();

                if (pitcherSteamParticles.isPlaying)
                    pitcherSteamParticles.Stop();
            }
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

        if (pitcherSteamParticles != null)
            pitcherSteamParticles.Stop();

        if (steamAudio != null)
            steamAudio.Stop();

        Debug.Log("Steam OFF");
    }
}
