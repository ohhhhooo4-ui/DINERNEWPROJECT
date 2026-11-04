using UnityEngine;

public class NotePile : MonoBehaviour
{
    public GameObject paperPrefab;
    public Transform spawnPoint;

    public void SpawnPaper()
    {
        GameObject paper = Instantiate(
            paperPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        PaperNote note = paper.GetComponent<PaperNote>();

        if (note != null)
        {
            note.pile = this;
        }
    }

    private void Start()
    {
        SpawnPaper();
    }
}