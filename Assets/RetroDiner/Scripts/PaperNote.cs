using UnityEngine;
using Autohand;

public class PaperNote : MonoBehaviour
{
    [HideInInspector]
    public NotePile pile;

    private bool spawnedReplacement = false;

    private Grabbable grabbable;

    private void Start()
    {
        grabbable = GetComponent<Grabbable>();

        if (grabbable != null)
        {
            grabbable.OnGrabEvent += OnGrabbed;
        }
    }

    private void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.OnGrabEvent -= OnGrabbed;
        }
    }

    private void OnGrabbed(Hand hand, Grabbable grabbed)
    {
        if (spawnedReplacement)
            return;

        spawnedReplacement = true;

        if (pile != null)
        {
            pile.SpawnPaper();
        }
    }
}