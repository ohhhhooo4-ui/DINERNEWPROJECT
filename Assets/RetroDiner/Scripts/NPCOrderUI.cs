using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class NPCOrderUI : MonoBehaviour
{
    public GameObject interactionUI;
    public GameObject orderPanel;
    public TMP_Text orderText;

    private bool playerNear = false;
    private bool buttonReleased = true;
    private bool orderTaken = false;

    private string currentOrder = "";

    private string[] orders =
    {
       "Burger Without Cheese",
        "Coffee",
        "Fries",
        "Double Cheeseburger",
        "Cheeseburger and Fries"

    };

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (orderPanel != null)
            orderPanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerNear)
            return;

        InputDevice rightHand =
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool primaryButton;

        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton))
        {
            if (primaryButton && buttonReleased && !orderTaken)
            {
                ShowOrder();
                buttonReleased = false;
            }

            if (!primaryButton)
            {
                buttonReleased = true;
            }
        }
    }

    public void SetPlayerNear(bool value)
    {
        playerNear = value;

        if (playerNear)
        {
            if (!orderTaken && interactionUI != null)
                interactionUI.SetActive(true);
        }
        else
        {
            orderTaken = false;
            currentOrder = "";

            if (interactionUI != null)
                interactionUI.SetActive(false);

            HideOrder();
        }
    }

    private void ShowOrder()
    {
        orderTaken = true;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (orderPanel != null)
            orderPanel.SetActive(true);

        if (string.IsNullOrEmpty(currentOrder))
        {
            currentOrder = orders[Random.Range(0, orders.Length)];
        }

        if (orderText != null)
        {
            orderText.text = "Order: " + currentOrder;
        }
    }

    public void HideOrder()
    {
        if (orderPanel != null)
            orderPanel.SetActive(false);
    }
}