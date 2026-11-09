using UnityEngine;

public class PortaFilter : MonoBehaviour
{
    public GameObject coffeeGroundsLoosePivot;

    public GameObject coffeeGroundsTampedPivot;

    public float coffeeDose = 0f;

    public bool tamped = false;

    private Vector3 originalLooseScale;

    private void Start()
    {
        coffeeGroundsLoosePivot.SetActive(false);

        coffeeGroundsTampedPivot.SetActive(false);

        originalLooseScale =
            coffeeGroundsLoosePivot.transform.localScale;
    }

    private void Update()
    {
        if (!tamped)
        {
            float fillAmount =
                Mathf.Clamp01(coffeeDose / 18f);

            coffeeGroundsLoosePivot.transform.localScale =
                new Vector3(
                    originalLooseScale.x,
                    originalLooseScale.y * fillAmount,
                    originalLooseScale.z
                );
        }

        if (tamped)
        {
            coffeeGroundsLoosePivot.SetActive(false);

            coffeeGroundsTampedPivot.SetActive(true);
        }

    }

    public void TampCoffee()
    {
        tamped = true;

        Debug.Log("Switching Visuals");

        coffeeGroundsLoosePivot.SetActive(false);

        coffeeGroundsTampedPivot.SetActive(true);

        Debug.Log(
            "Loose Active: " +
            coffeeGroundsLoosePivot.activeSelf);

        Debug.Log(
            "Tamped Active: " +
            coffeeGroundsTampedPivot.activeSelf);
    }
}
