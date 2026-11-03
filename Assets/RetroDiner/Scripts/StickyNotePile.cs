using UnityEngine;

public class StickyNotePile : MonoBehaviour
{
    public GameObject stickyNotePrefab;
    public Transform spawnPoint;

    private GameObject currentNote;

    void Start()
    {
        SpawnNote();
    }

    void Update()
    {
        if (currentNote == null)
        {
            SpawnNote();
        }
    }

    void SpawnNote()
    {
        currentNote = Instantiate(
            stickyNotePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}