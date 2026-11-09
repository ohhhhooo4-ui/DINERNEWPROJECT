using UnityEngine;
using Autohand;

public class CupDetectionZone : MonoBehaviour
{
    public PlacePoint placePoint;

    public CoffeeCup currentCup;

    private void Update()
    {
        if (placePoint.placedObject != null)
        {
            currentCup =
                placePoint
                .placedObject
                .GetComponent<CoffeeCup>();
        }
        else
        {
            currentCup = null;
        }
    }
}