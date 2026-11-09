using UnityEngine;

public class GroupHeadSocket : MonoBehaviour
{
    public PortaFilter currentPortaFilter;

    public void OnPortaFilterPlaced(GameObject obj)
    {
        currentPortaFilter =
            obj.GetComponent<PortaFilter>();

        Debug.Log("PortaFilter Locked In");
    }

    public void OnPortaFilterRemoved(GameObject obj)
    {
        currentPortaFilter = null;

        Debug.Log("PortaFilter Removed");
    }
}