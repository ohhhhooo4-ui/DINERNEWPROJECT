using UnityEngine;
using Autohand;

public class PaperNote : MonoBehaviour
{
    public NotePile pile;

    private Grabbable grabbable;
    private bool taken;

    private void Start()
    {
        grabbable = GetComponent<Grabbable>();
    }

    private void Update()
    {
        if (taken)
            return;

        if (grabbable != null && grabbable.HeldCount() > 0)
        {
            taken = true;

            if (pile != null)
            {
                pile.PaperTaken();
            }
        }
    }
}