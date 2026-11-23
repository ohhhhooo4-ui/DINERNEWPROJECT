using UnityEngine;

public class PanCookEgg : MonoBehaviour
{
    public GameObject sunnySideEggPrefab;
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EggBroken"))
        {
            Debug.Log("Pan is catching object " + other.gameObject.name + "("+ other.gameObject.tag + ")");
            Instantiate(
                    sunnySideEggPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

             //Destroy(other.gameObject);
            
        }
    }
}
