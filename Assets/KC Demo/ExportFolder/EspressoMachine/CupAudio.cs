using UnityEngine;

public class CupAudio : MonoBehaviour
{
    public AudioSource cupAudio;

    private bool canPlay = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (!canPlay)
            return;

        if (collision.relativeVelocity.magnitude < 0.3f)
            return;

        cupAudio.Play();

        canPlay = false;

        Invoke(nameof(ResetSound), 0.2f);
    }

    private void ResetSound()
    {
        canPlay = true;
    }
}