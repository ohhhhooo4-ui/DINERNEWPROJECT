using UnityEngine;

public class CupSpawner : MonoBehaviour
{
    public GameObject coffeeMugPrefab;

    public Transform spawnPoint;

    public SpawnZone spawnZone;

    private bool spawning;

    private void Start()
    {
        SpawnCup();
    }

    private void Update()
    {
        if (!spawnZone.HasCup() && !spawning)
        {
            SpawnCup();
        }
    }

    private void SpawnCup()
    {
        Instantiate(
            coffeeMugPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        Debug.Log("New Cup Spawned");
    }
}
