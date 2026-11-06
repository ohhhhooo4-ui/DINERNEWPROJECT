
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [Header("Oil Particle System")]
    public ParticleSystem oilParticles;

    [Header("Container")]
    public Transform oilSurface;
    public float containerCapacity = 1000f;
    public float fillSpeed = 1f;

    [Header("Pouring")]
    public bool isPouring = false;
    public float pourAngle = 45f;

    private float currentOil = 0f;
    private ParticleSystem.Particle[] particles;
    private int previousParticleCount;

    void Start()
    {
        particles = new ParticleSystem.Particle[
            oilParticles.main.maxParticles
        ];

        if (oilSurface != null)
            oilSurface.localScale = new Vector3(1, 0, 1);
    }

    void Update()
    {
        // Start or stop pouring
        if (isPouring)
        {
            oilParticles.Play();
        }
        else
        {
            oilParticles.Stop();
        }

        // Detect bottle angle
        float angle = Vector3.Angle(
            transform.up,
            Vector3.up
        );

        isPouring = angle > pourAngle;

        UpdateFillLevel();
    }

    void UpdateFillLevel()
    {
        if (oilSurface == null) return;

        float fillAmount = Mathf.Clamp01(
            currentOil / containerCapacity
        );

        Vector3 scale = oilSurface.localScale;

        scale.y = fillAmount;

        oilSurface.localScale = scale;
    }

    public void AddOil(float amount)
    {
        currentOil = Mathf.Clamp(
            currentOil + amount,
            0,
            containerCapacity
        );
    }

    public void RemoveOil(float amount)
    {
        currentOil = Mathf.Max(0, currentOil - amount);
    }

    public float GetFillPercentage()
    {
        return currentOil / containerCapacity * 100f;
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("OilContainer"))
        {
            int count = oilParticles.GetParticles(particles);

            int entered = Mathf.Max(
                0,
                count - previousParticleCount
            );

            AddOil(entered * fillSpeed);

            previousParticleCount = count;
        }
    }
}