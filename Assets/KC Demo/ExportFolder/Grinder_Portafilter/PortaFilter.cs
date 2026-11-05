using UnityEngine;

public class PortaFilter : MonoBehaviour
{

    public float coffeeDose = 0f;

    public bool tamped = false;

    public Transform coffeeGroundsVisual;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = coffeeGroundsVisual.localScale;

        // Start empty
        coffeeGroundsVisual.localScale =
            new Vector3(originalScale.x, 0f, originalScale.z);
    }

    private void FixedUpdate()
    {
        float fillAmount = Mathf.Clamp01(coffeeDose / 18f);

        coffeeGroundsVisual.localScale =
            new Vector3(1f, fillAmount, 1f);
    }

    private void Update()
    {
        float fillAmount = Mathf.Clamp01(coffeeDose / 18f);

        coffeeGroundsVisual.localScale =
            new Vector3(originalScale.x,originalScale.y * fillAmount,originalScale.z );
    }
}
