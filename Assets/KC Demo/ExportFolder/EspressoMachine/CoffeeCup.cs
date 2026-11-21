using UnityEngine;

public class CoffeeCup : MonoBehaviour
{
    public float coffeeAmount = 0f;

    public float milkAmount = 0f;

    public float maxCoffeeAmount = 40f;

    public float maxMilkAmount = 100f;

    public bool hasEspresso;

    public bool hasMilk;

    public bool latteComplete;

    public Transform coffeeLiquidVisual;

    public Renderer coffeeLiquidRenderer;

    public Color espressoColor =
        new Color(0.15f, 0.08f, 0.03f);

    public Color latteColor =
        new Color(0.65f, 0.5f, 0.35f);

    private Vector3 originalVisualScale;

    private void Start()
    {
        originalVisualScale =
            coffeeLiquidVisual.localScale;
    }

    private void Update()
    {
        float totalLiquid =
            coffeeAmount + milkAmount;

        float maxLiquid =
            maxCoffeeAmount + maxMilkAmount;


        float fillAmount =
           Mathf.Clamp01(
           totalLiquid / maxLiquid);

        if (totalLiquid <= 0f)
        {
            coffeeLiquidVisual.localScale =
                new Vector3(
                    originalVisualScale.x,
                    0f,
                    originalVisualScale.z
                );
        }
        else
        {
            coffeeLiquidVisual.localScale =
                new Vector3(
                    originalVisualScale.x,
                    originalVisualScale.y * fillAmount,
                    originalVisualScale.z
                );
        }


        if (totalLiquid > 0)
        {
            float milkRatio =
                milkAmount / totalLiquid;

            coffeeLiquidRenderer.material.color =
                Color.Lerp(
                    espressoColor,
                    latteColor,
                    milkRatio);
        }
    }

    public bool IsLatte()
    {
        return hasEspresso &&
               hasMilk;
    }
}