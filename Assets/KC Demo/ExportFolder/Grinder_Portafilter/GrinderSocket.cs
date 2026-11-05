using UnityEngine;
using Autohand;

public class GrinderSocket : MonoBehaviour
{
    public PortaFilter currentPortafilter;

    public void OnPlaced(PlacePoint point, Grabbable grab)
    {
        currentPortafilter =
            grab.GetComponent<PortaFilter>();
    }

    public void OnRemoved(PlacePoint point, Grabbable grab)
    {
        currentPortafilter = null;
    }
}