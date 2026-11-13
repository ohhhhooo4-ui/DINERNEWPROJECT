using UnityEngine;
using Autohand;

public class PaperNote : MonoBehaviour
{
     NotePile pile;

    private bool taken = false;

    private void Start()
    {
        pile = GetComponentInParent<NotePile>();
    }


    public void FirstGrab()
    {
        if (taken == false)
        {
            Debug.Log(gameObject.name + " grabbed the FIRST TIME");
            pile.PaperTaken();
            taken = true;
        }
    }
}