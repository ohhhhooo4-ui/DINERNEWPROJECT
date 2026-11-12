using UnityEngine;

public class NotePile : MonoBehaviour
{
    public Transform spawnPoint;

    private GameObject currentPaper;
    private GameObject originalPaper;

    private void Start()
    {
        PaperNote existingPaper = GetComponentInChildren<PaperNote>();

        if (existingPaper != null)
        {
            currentPaper = existingPaper.gameObject;
            originalPaper = existingPaper.gameObject;

            existingPaper.pile = this;
        }
        else
        {
            Debug.LogError("No paper found under NotePile!");
        }
    }

    public void PaperTaken()
    {
        if (currentPaper == null)
            return;

        currentPaper = null;

        Invoke(nameof(SpawnPaper), 0.1f);
    }

    private void SpawnPaper()
    {
        if (currentPaper != null || originalPaper == null)
            return;

        currentPaper = Instantiate(
            originalPaper,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Scale from your screenshot
        currentPaper.transform.localScale = new Vector3(
            0.9586343f,
            1.183686f,
            0.9542204f
        );

        PaperNote note = currentPaper.GetComponent<PaperNote>();

        if (note != null)
        {
            note.pile = this;
        }
    }
}