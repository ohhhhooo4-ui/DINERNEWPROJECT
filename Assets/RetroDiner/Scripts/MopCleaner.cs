using UnityEngine;

public class MopCleaner : MonoBehaviour
{
    private float lastCleanTime;

    private void OnTriggerStay(Collider other)
    {
        DirtSpot dirt = other.GetComponent<DirtSpot>();

        if (dirt != null && Time.time > lastCleanTime + 0.25f)
        {
            dirt.Clean();
            lastCleanTime = Time.time;
        }
    }
}