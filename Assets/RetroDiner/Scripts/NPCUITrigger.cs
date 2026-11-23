using UnityEngine;

public class NPCUITrigger : MonoBehaviour
{
    public NPCOrderUI orderUI;

    private void OnTriggerEnter(Collider other)
    {
        if (orderUI != null)
            orderUI.SetPlayerNear(true);

        //Debug.Log("Player entered NPC trigger");
    }

    private void OnTriggerExit(Collider other)
    {
        if (orderUI != null)
        {
            orderUI.SetPlayerNear(false);
            orderUI.HideOrder();
        }

        //Debug.Log("Player left NPC trigger");
    }
}