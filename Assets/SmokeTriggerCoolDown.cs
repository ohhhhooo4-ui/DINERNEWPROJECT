using System.Collections;
using UnityEngine;

public class ParticleTriggerCooldown : MonoBehaviour
{
    [Header("Particle Object")]
    public GameObject particleObject;

    [Header("Settings")]
    public float cooldownSeconds = 2f;

    [Header("Optional Tag Filter")]
    public string requiredTag = "";

    private Coroutine cooldownRoutine;

    private void Start()
    {
        if (particleObject != null)
            particleObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) &&
            !other.CompareTag(requiredTag))
            return;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        if (particleObject != null)
            particleObject.SetActive(true);

        Debug.Log("Particles Activated");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) &&
            !other.CompareTag(requiredTag))
            return;

        cooldownRoutine = StartCoroutine(TurnOffAfterDelay());
    }

    private IEnumerator TurnOffAfterDelay()
    {
        yield return new WaitForSeconds(cooldownSeconds);

        if (particleObject != null)
            particleObject.SetActive(false);

        Debug.Log("Particles Deactivated");
    }
}