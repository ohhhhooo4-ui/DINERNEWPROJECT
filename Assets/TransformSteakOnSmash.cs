
using UnityEngine;
using NaughtyAttributes;
using Autohand.Demo;

public class SmashTransform : MonoBehaviour
{
    [Header("Transformation Settings")]

    [Required]
    [Tooltip("Object to spawn when this object is smashed")]
    public GameObject replacementObject;

    [Tooltip("Transform replacement to the smashed object's position")]
    public bool copyPosition = true;

    [Tooltip("Transform replacement to the smashed object's rotation")]
    public bool copyRotation = true;

    [Tooltip("Copy the smashed object's scale")]
    public bool copyScale = true;

    [Header("Original Object Settings")]

    public bool destroyOriginal = true;

    [ShowIf(nameof(destroyOriginal))]
    public float destroyDelay = 0f;

    private Smash smash;
    private bool hasTransformed;

    private void Awake()
    {
        smash = GetComponent<Smash>();

        if (smash == null)
        {
            Debug.LogError("SmashTransform requires a Smash component!", this);
            enabled = false;
            return;
        }

        // Subscribe to the smash event
        smash.OnSmash.AddListener(TransformObject);
    }

    private void OnDestroy()
    {
        if (smash != null)
            smash.OnSmash.RemoveListener(TransformObject);
    }

    private void TransformObject()
    {
        if (hasTransformed)
            return;

        if (replacementObject == null)
        {
            Debug.LogWarning("No replacement object assigned!", this);
            return;
        }

        hasTransformed = true;

        Transform original = transform;

        Vector3 spawnPosition = copyPosition
            ? original.position
            : replacementObject.transform.position;

        Quaternion spawnRotation = copyRotation
            ? original.rotation
            : replacementObject.transform.rotation;

        GameObject spawnedObject = Instantiate(
            replacementObject,
            spawnPosition,
            spawnRotation
        );

        if (copyScale)
            spawnedObject.transform.localScale = original.lossyScale;

        if (destroyOriginal)
        {
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
