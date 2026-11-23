using UnityEngine;

public class ShowUIOnTrigger : MonoBehaviour
{
    public GameObject uiObject;
    public string handTag = "Hands";

    private void Start()
    {
        if (uiObject != null)
            uiObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(handTag))
        {
            if (uiObject != null)
                uiObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(handTag))
        {
            if (uiObject != null)
                uiObject.SetActive(false);
        }
    }
}