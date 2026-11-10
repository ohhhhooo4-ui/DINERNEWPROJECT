using UnityEngine;

public class NotePile : MonoBehaviour
{
    public GameObject paperPrefab;
    public Transform spawnPoint;

    private GameObject currentPaper;

    private void Start()
    {
        PaperNote existingPaper = GetComponentInChildren<PaperNote>();

        if (existingPaper != null)
        {
            currentPaper = existingPaper.gameObject;
            existingPaper.pile = this;
        }
        else
        {
            SpawnPaper();
        }
    }

    public void SpawnPaper()
    {
        if (currentPaper != null)
            return;

        currentPaper = Instantiate(
            paperPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        currentPaper.transform.localScale =
            paperPrefab.transform.localScale;

        PaperNote note =
            currentPaper.GetComponent<PaperNote>();

        if (note != null)
        {
            note.pile = this;
        }
    }

    public void PaperTaken()
    {
        currentPaper = null;
        SpawnPaper();
    }
}