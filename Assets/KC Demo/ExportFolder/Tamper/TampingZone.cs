using UnityEngine;

public class TampingZone : MonoBehaviour
{
    public PortaFilter portaFilter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tamper"))
        {
            if (portaFilter.coffeeDose <= 0f)
            {
                Debug.Log("No Coffee To Tamp");
                return;
            }

            if (portaFilter.tamped)
                return;

            portaFilter.TampCoffee();
        }
    }
}