using UnityEngine;

public class MilkFillZone : MonoBehaviour
{
    public MilkPitcher pitcher;

    public float pourRate = 100f;

    private void OnTriggerStay(Collider other)
    {
        MilkCarton carton =
            other.GetComponentInParent<MilkCarton>();

        if (carton == null)
            return;

        Debug.Log("Carton Detected");

        if (!carton.IsPouring())
            return;

        Debug.Log("Pouring Milk");

        pitcher.milkAmount +=
            pourRate * Time.deltaTime;

        pitcher.milkAmount =
            Mathf.Clamp(
                pitcher.milkAmount,
                0f,
                pitcher.maxMilkAmount);
    }
}