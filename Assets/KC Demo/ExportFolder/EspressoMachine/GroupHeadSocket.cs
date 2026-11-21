using UnityEngine;

public class GroupHeadSocket : MonoBehaviour
{
    public PortaFilter currentPortaFilter;

    public void OnPortaFilterPlaced(GameObject obj)
    {
        Debug.Log("GROUP HEAD EVENT FIRED");

        currentPortaFilter =
            obj.GetComponentInParent<PortaFilter>();

        Debug.Log(
            "PortaFilter Found: " +
            (currentPortaFilter != null));

        if (currentPortaFilter != null)
        {
            currentPortaFilter.insertedIntoMachine = true;

            Debug.Log(
                "InsertedIntoMachine TRUE");
        }
    }

    public void OnPortaFilterRemoved(GameObject obj)
    {
        if (currentPortaFilter != null)
        {
            currentPortaFilter.insertedIntoMachine = false;
        }

        currentPortaFilter = null;

        Debug.Log("PortaFilter Removed");
    }
}