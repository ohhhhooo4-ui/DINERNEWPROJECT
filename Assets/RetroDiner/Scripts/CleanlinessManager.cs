using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CleanlinessManager : MonoBehaviour
{
    public static CleanlinessManager Instance;

    [Header("UI")]
    public Slider cleanlinessSlider;
    public TMP_Text cleanlinessText;

    [Header("Values")]
    public float maxCleanliness = 100f;
    public float cleanliness = 100f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        cleanliness = Mathf.Clamp(cleanliness, 0f, maxCleanliness);

        if (cleanlinessSlider != null)
            cleanlinessSlider.value = cleanliness / maxCleanliness;

        if (cleanlinessText != null)
            cleanlinessText.text = $"Cleanliness {cleanliness:0}%";
    }

    public void LoseCleanliness(float amount)
    {
        cleanliness -= amount;
    }

    public void GainCleanliness(float amount)
    {
        cleanliness += amount;
    }
}