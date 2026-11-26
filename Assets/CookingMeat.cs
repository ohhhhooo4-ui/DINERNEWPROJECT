using System.Collections.Generic;
using UnityEngine;

public class RootParentChildActivator : MonoBehaviour
{
    [Header("Tag To Detect")]
    public string targetTag = "Uncooked";

    [Header("Child Object Name")]
    public string childObjectName = "Steam";

    [Header("Debug")]
    public bool debugLogs = true;

    // Tracks root objects currently inside
    private readonly HashSet<Transform> activeRoots = new HashSet<Transform>();

    private void Start()
    {
        Debug.Log("[Activator] Started on " + gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Find the tagged parent or root
        Transform taggedTransform = GetTaggedParent(other.transform);

        if (taggedTransform == null)
        {
            if (debugLogs)
                Debug.Log("[Activator] No tagged parent found for: " + other.name);

            return;
        }

        Transform root = taggedTransform.root;

        if (activeRoots.Contains(root))
        {
            if (debugLogs)
                Debug.Log("[Activator] Already tracking: " + root.name);

            return;
        }

        activeRoots.Add(root);

        if (debugLogs)
            Debug.Log("[Activator] ENTER -> " + root.name);

        Transform child = FindChildRecursive(root, childObjectName);

        if (child != null)
        {
            child.gameObject.SetActive(true);

            if (debugLogs)
                Debug.Log("[Activator] Activated: " + child.name +
                          " on " + root.name);
        }
        else
        {
            Debug.LogWarning(
                "[Activator] Child '" +
                childObjectName +
                "' not found under " +
                root.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Transform taggedTransform = GetTaggedParent(other.transform);

        if (taggedTransform == null)
            return;

        Transform root = taggedTransform.root;

        if (!activeRoots.Contains(root))
            return;

        activeRoots.Remove(root);

        if (debugLogs)
            Debug.Log("[Activator] EXIT -> " + root.name);

        Transform child = FindChildRecursive(root, childObjectName);

        if (child != null)
        {
            child.gameObject.SetActive(false);

            if (debugLogs)
                Debug.Log("[Activator] Deactivated: " + child.name +
                          " on " + root.name);
        }
    }

    private Transform GetTaggedParent(Transform start)
    {
        Transform current = start;

        while (current != null)
        {
            if (current.CompareTag(targetTag))
                return current;

            current = current.parent;
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
                return child;
        }

        return null;
    }
}