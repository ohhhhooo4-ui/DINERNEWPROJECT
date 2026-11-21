using UnityEngine;

public class SteamDetectionZone : MonoBehaviour
{
    public MilkPitcher currentPitcher;

    private void Start()
    {
        currentPitcher = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        MilkPitcher pitcher =
        other.GetComponent<MilkPitcher>();

        if (pitcher != null)
        {
            currentPitcher = pitcher;

            Debug.Log("Pitcher Detected By: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MilkPitcher pitcher =
        other.GetComponent<MilkPitcher>();

        if (pitcher == currentPitcher)
        {
            currentPitcher = null;

            Debug.Log("Pitcher Removed By: " + other.name);
        }
    }
}