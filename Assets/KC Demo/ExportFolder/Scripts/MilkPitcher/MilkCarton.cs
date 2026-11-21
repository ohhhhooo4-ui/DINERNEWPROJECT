using UnityEngine;

public class MilkCarton : MonoBehaviour
{
    public ParticleSystem milkStream;
    public AudioSource milkPourAudio;
    public float pourAngle = 120f;

    private void Update()
    {
        if (IsPouring())
        {
            if (milkStream != null && !milkStream.isPlaying)
            {
                milkStream.Play();
            }

            if (milkPourAudio != null &&
        !milkPourAudio.isPlaying)
            {
                milkPourAudio.Play();
            }
        }
        else
        {
            if (milkStream != null && milkStream.isPlaying)
            {
                milkStream.Stop();
            }

            if (milkPourAudio != null &&
        milkPourAudio.isPlaying)
            {
                milkPourAudio.Stop();
            }
        }
    }

    public bool IsPouring()
    {
        return transform.eulerAngles.z > pourAngle;
    }
}