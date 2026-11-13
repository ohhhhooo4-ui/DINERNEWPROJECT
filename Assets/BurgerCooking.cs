using UnityEngine;

public class BurgerCookingSequence : MonoBehaviour
{
    public GameObject transformedObject;
    public ParticleSystem completionParticles;

    private enum State
    {
        WaitingForStoveTouch,
        WaitingForStoveExit,
        WaitingForBurgerFlipper,
        WaitingForFinalStoveTouch,
        Complete
    }

    private State currentState = State.WaitingForStoveTouch;

    private void OnTriggerEnter(Collider other)
    {
        switch (currentState)
        {
            case State.WaitingForStoveTouch:
                if (other.CompareTag("StoveTop"))
                {
                    currentState = State.WaitingForStoveExit;
                    Debug.Log("Step 1 Complete: Touched StoveTop");
                }
                break;

            case State.WaitingForBurgerFlipper:
                if (other.CompareTag("BurgerFlipper"))
                {
                    currentState = State.WaitingForFinalStoveTouch;
                    Debug.Log("Step 3 Complete: Touched BurgerFlipper");
                }
                break;

            case State.WaitingForFinalStoveTouch:
                if (other.CompareTag("StoveTop"))
                {
                    CompleteTransformation();
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentState == State.WaitingForStoveExit &&
            other.CompareTag("StoveTop"))
        {
            currentState = State.WaitingForBurgerFlipper;
            Debug.Log("Step 2 Complete: Left StoveTop");
        }
    }

    private void CompleteTransformation()
    {
        currentState = State.Complete;

        if (transformedObject != null)
        {
            Instantiate(transformedObject, transform.position, transform.rotation);
            Destroy(gameObject);
        }

        if (completionParticles != null)
        {
            completionParticles.Play();
        }

        Debug.Log("Transformation Complete!");
    }
}