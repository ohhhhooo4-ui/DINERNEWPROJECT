using UnityEngine;

public class DirtSpawner : MonoBehaviour
{
    public GameObject dirtPrefab;
    public Transform[] spawnPoints;

    public float spawnInterval = 60f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            int randomIndex = Random.Range(0, spawnPoints.Length);

            Instantiate(
                dirtPrefab,
                spawnPoints[randomIndex].position,
                Quaternion.identity);

            // Reduce cleanliness when dirt appears
            CleanlinessManager.Instance.LoseCleanliness(2f);
        }
    }
}