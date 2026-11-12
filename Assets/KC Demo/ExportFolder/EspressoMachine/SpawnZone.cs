using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    private int cupsInside;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CoffeeCup>() != null)
        {
            cupsInside++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CoffeeCup>() != null)
        {
            cupsInside--;
        }
    }

    public bool HasCup()
    {
        return cupsInside > 0;
    }
}

