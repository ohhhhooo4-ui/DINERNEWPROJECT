using UnityEngine;
using Autohand;
using UnityEngine.Events;

public class TampingZone : MonoBehaviour
{
    public PortaFilter portaFilter;

    public UnityEvent onTampSuccess;

    public float tampDepth = 0.015f;

    private bool tamperInside;

    private Transform currentTamper;

    private Vector3 entryPosition;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tamper"))
        {
            tamperInside = true;

            currentTamper = other.transform;

            entryPosition = currentTamper.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tamper"))
        {
            tamperInside = false;

            currentTamper = null;
        }
    }

    private void Update()
    {
        if (!tamperInside)
            return;

        if (portaFilter.tamped)
            return;

        if (portaFilter.coffeeDose <= 0)
            return;

        float pressDistance =
            entryPosition.y -
            currentTamper.position.y;

        if (pressDistance >= tampDepth)
        {
            Debug.Log("TAMP SUCCESS");

            portaFilter.TampCoffee();

            onTampSuccess?.Invoke();

            Grabbable grabbable =
                currentTamper.GetComponent<Grabbable>();

            if (grabbable != null)
            {
                // Haptic call goes here
            }
        }
    }
}