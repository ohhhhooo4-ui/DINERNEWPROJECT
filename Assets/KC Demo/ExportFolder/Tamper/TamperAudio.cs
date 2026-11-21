using UnityEngine;

public class TamperAudio : MonoBehaviour
{
    public AudioSource tamperAudio;

    private bool canPlay = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (!canPlay)
            return;

        if (collision.relativeVelocity.magnitude < 0.5f)
            return;

        tamperAudio.Play();

        canPlay = false;

        Invoke(nameof(ResetSound), 0.25f);
    }

    private void ResetSound()
    {
        canPlay = true;
    }
}

