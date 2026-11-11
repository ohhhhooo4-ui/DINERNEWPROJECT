using UnityEngine;

public class FloatingCharacter : MonoBehaviour
{
    [Header("Looping Movement")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 1f;
    public float rotationAmount = 5f;
    public float rotationSpeed = 0.8f;

    [Header("One-Time Position Change")]
    public Vector3 positionChange = new Vector3(2f, 0f, 0f);
    public float positionChangeDuration = 3f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 targetPosition;
    private float elapsedTime;

    void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        targetPosition = startPosition + positionChange;
    }

    void Update()
    {
        // One-time position change (does not loop)
        if (elapsedTime < positionChangeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / positionChangeDuration
            );

            transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );
        }

        // Continuous up and down floating
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        Vector3 basePosition = elapsedTime >= positionChangeDuration
            ? targetPosition
            : transform.localPosition;

        transform.localPosition = basePosition + Vector3.up * y;

        // Continuous back and forth rotation
        float angle = Mathf.Sin(Time.time * rotationSpeed)
            * rotationAmount;

        transform.localRotation = startRotation *
            Quaternion.Euler(0f, 0f, angle);
    }
}