using UnityEngine;

public class ShowUIOnGrab : MonoBehaviour
{
    public GameObject uiObject;

    private void Start()
    {
        if (uiObject != null)
            uiObject.SetActive(false);
    }

    public void ShowUI()
    {
        if (uiObject != null)
            uiObject.SetActive(true);
    }

    public void HideUI()
    {
        if (uiObject != null)
            uiObject.SetActive(false);
    }
}