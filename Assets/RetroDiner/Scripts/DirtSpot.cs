using UnityEngine;

public class DirtSpot : MonoBehaviour
{
    public int cleaningNeeded = 5;

    private int startingCleaningNeeded;
    private Vector3 originalScale;

    private void Start()
    {
        startingCleaningNeeded = cleaningNeeded;
        originalScale = transform.localScale;
    }

    public void Clean()
    {
        cleaningNeeded--;

        float scalePercent = (float)cleaningNeeded / startingCleaningNeeded;

        // Shrink relative to original size
        transform.localScale = originalScale * Mathf.Max(scalePercent, 0.2f);

        if (cleaningNeeded <= 0)
        {
            if (CleanlinessManager.Instance != null)
            {
                CleanlinessManager.Instance.GainCleanliness(2f);
            }

            Destroy(gameObject);
        }
    }
}