using TMPro;
using UnityEngine;

public class SatisfactionSystem : MonoBehaviour
{
    [Header("References")]
    public CleanlinessManager cleanlinessManager;

    [Header("UI")]
    public TMP_Text satisfactionText;

    void Update()
    {
        if (cleanlinessManager == null || satisfactionText == null)
            return;

        float satisfaction = GetSatisfaction();

        satisfactionText.text =
            "Satisfaction: " + Mathf.RoundToInt(satisfaction) + "%";

        satisfactionText.color =
            Color.Lerp(Color.red, Color.green, satisfaction / 100f);
    }

    float GetSatisfaction()
    {
        // Direct link to cleanliness
        return cleanlinessManager.cleanliness;

        // Alternative:
        // return Mathf.Clamp(cleanlinessManager.cleanliness * 1.25f - 25f, 0, 100);
    }
}