using UnityEngine;
using Autohand;

public class GrinderSocket : MonoBehaviour
{
    public PortaFilter currentPortafilter;

    public void OnPlaced(PlacePoint point, Grabbable grab)
    {
        currentPortafilter =
            grab.GetComponent<PortaFilter>();

        if (currentPortafilter != null)
        {
            currentPortafilter.insertedIntoGrinder = true;
        }
    }

    public void OnRemoved(PlacePoint point, Grabbable grab)
    {
        if (currentPortafilter != null)
        {
            currentPortafilter.insertedIntoGrinder = false;
        }

        currentPortafilter = null;
    }
}