using UnityEngine;

public class CoffeeCup : MonoBehaviour
{
    public float coffeeAmount = 0f;

    public float maxCoffeeAmount = 40f;

    public Transform coffeeLiquidPivot;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = coffeeLiquidPivot.localScale;
    }

    private void Update()
    {
        float fillAmount =
       Mathf.Clamp01(
           coffeeAmount /
           maxCoffeeAmount);

        float visualFill =
            Mathf.Lerp(
                0.25f,
                1f,
                fillAmount);

        coffeeLiquidPivot.localScale =
            new Vector3(
                originalScale.x,
                originalScale.y * visualFill,
                originalScale.z
            );
    }
}