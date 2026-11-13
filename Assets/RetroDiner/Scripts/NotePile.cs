using UnityEngine;

public class NotePile : MonoBehaviour
{
    public Transform spawnPoint;

    public GameObject paperPrefab;

    public void PaperTaken()
    {
        Invoke(nameof(SpawnPaper), 1f);
       
    }

    private void SpawnPaper()
    {
        Instantiate(
            paperPrefab,
            spawnPoint
        );

    }
}