using UnityEngine;

public class CoffeeGrinder : MonoBehaviour
{
    public GrinderSocket socket;

    public float doseRate = 4f;
    public float targetDose = 18f;

    private bool grinding;
    public ParticleSystem coffeeParticles;
    public AudioSource grinderAudio;

    private void Update()
    {
        if (!grinding)
            return;

        if (socket.currentPortafilter == null)
            return;

        // Add coffee while grinding
        socket.currentPortafilter.coffeeDose +=
            doseRate * Time.deltaTime;

        // Stop automatically at target dose
        if (socket.currentPortafilter.coffeeDose >= targetDose)
        {
            socket.currentPortafilter.coffeeDose = targetDose;
            StopGrinding();
        }
    }

    public void StartGrinding()
    {
        if (socket.currentPortafilter == null)
        {
            Debug.Log("No Portafilter Inserted");
            return;
        }

        // Optional: reset the basket before grinding
        if (socket.currentPortafilter.coffeeDose > 0f)
            return;
        socket.currentPortafilter.coffeeGroundsLoosePivot.SetActive(true);

        socket.currentPortafilter.coffeeGroundsTampedPivot.SetActive(false);

        grinding = true;

        if (grinderAudio != null)
        {
            grinderAudio.Play();
        }

        //coffeeParticles.Play();

        Debug.Log("Grinding Started");
    }

    public void StopGrinding()
    {
        grinding = false;

        if (grinderAudio != null)
        {
            grinderAudio.Stop();
        }

        //coffeeParticles.Stop();

        Debug.Log("18g Dose Complete");
    }
}