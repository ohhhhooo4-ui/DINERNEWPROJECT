using UnityEngine;

public class MopCleaner : MonoBehaviour
{
    public float requiredSpeed = 0.5f;
    public float cleanCooldown = 0.3f;

    private Vector3 lastPosition;
    private float lastCleanTime;

    private void Update()
    {
        lastPosition = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < lastCleanTime + cleanCooldown)
            return;

        float speed =
            Vector3.Distance(transform.position, lastPosition) /
            Time.deltaTime;

        if (speed < requiredSpeed)
            return;

        DirtSpot dirt = other.GetComponent<DirtSpot>();

        if (dirt != null)
        {
            dirt.Clean();
            lastCleanTime = Time.time;
        }
    }
}