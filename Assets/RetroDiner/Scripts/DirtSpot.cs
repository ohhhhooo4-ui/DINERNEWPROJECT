using UnityEngine;

public class DirtSpot : MonoBehaviour
{
    public int cleaningNeeded = 5;

    private int startingCleaningNeeded;

    private void Start()
    {
        startingCleaningNeeded = cleaningNeeded;
    }

    public void Clean()
    {
        cleaningNeeded--;

        float scale =
            (float)cleaningNeeded /
            startingCleaningNeeded;

        transform.localScale =
            Vector3.one * Mathf.Max(scale, 0.2f);

        if (cleaningNeeded <= 0)
        {
            // Restore cleanliness
            CleanlinessManager.Instance.GainCleanliness(2f);

            Destroy(gameObject);
        }
    }
}