using UnityEngine;

public class MilkPourZone : MonoBehaviour
{
    public CoffeeCup cup;

    private void OnTriggerStay(Collider other)
    {
        MilkPitcher pitcher =
            other.GetComponentInParent<MilkPitcher>();

        if (pitcher == null)
            return;

        if (!pitcher.IsPouring())
            return;

        if (pitcher.milkAmount <= 0f)
            return;

        float transferRate = 20f;

        float amount =
            transferRate *
            Time.deltaTime;

        pitcher.milkAmount -= amount;

        cup.milkAmount += amount;

        cup.hasMilk = true;

        if (cup.IsLatte() &&
    !cup.latteComplete)
        {
            cup.latteComplete = true;

            Debug.Log("Latte Complete");

            ObjectiveManager.Instance.CompleteObjective();
        }


        cup.milkAmount =
            Mathf.Clamp(
                cup.milkAmount,
                0f,
                cup.maxMilkAmount);
    }
}