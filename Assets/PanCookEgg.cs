using UnityEngine;

public class PanCookEgg : MonoBehaviour
{
    public GameObject sunnySideEggPrefab;
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EggBroken"))
        {
            Instantiate(
                    sunnySideEggPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

             //Destroy(other.gameObject);
            
        }
    }
}
