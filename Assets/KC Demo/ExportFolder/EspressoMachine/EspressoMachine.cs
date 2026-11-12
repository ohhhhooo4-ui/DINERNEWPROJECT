using UnityEngine;
using System.Collections;

public class EspressoMachine : MonoBehaviour
{
    public GroupHeadSocket socket;

    public CupDetectionZone cupZone;

    public ParticleSystem espressoFlow;

    public AudioSource brewAudio;

    public float brewTime = 20f;

    public float requiredDose = 18f;

    private bool brewing;

    public void StartBrew()
    {
        if (brewing)
            return;

        if (socket.currentPortaFilter == null)
        {
            Debug.Log("No PortaFilter Inserted");
            return;
        }

        if (socket.currentPortaFilter.coffeeDose < requiredDose)
        {
            Debug.Log("Not Enough Coffee In PortaFilter");
            return;
        }

        if (!socket.currentPortaFilter.tamped)
        {
            Debug.Log("Coffee Needs Tamping");
            return;
        }

        if (cupZone.currentCup == null)
        {
            Debug.Log("No Cup Detected");
            return;
        }

        if (cupZone.currentCup.coffeeAmount >= cupZone.currentCup.maxCoffeeAmount - 0.1f)
        {
            Debug.Log("Cup Is Full");
            return;
        }

        StartCoroutine(BrewShot());
    }

    private IEnumerator BrewShot()
    {
        brewing = true;

        Debug.Log("Brewing Started");

        if (brewAudio != null)
        {
            brewAudio.Play();
        }

        if (espressoFlow != null)
            espressoFlow.Play();

        CoffeeCup currentCup = cupZone.currentCup;

        float elapsedTime = 0f;

        while (elapsedTime < brewTime)
        {
            currentCup.coffeeAmount +=
                (currentCup.maxCoffeeAmount / brewTime)
                * Time.deltaTime;
            Debug.Log(currentCup.coffeeAmount);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        currentCup.coffeeAmount =
            Mathf.Min(
                currentCup.coffeeAmount,
                currentCup.maxCoffeeAmount);

        if (espressoFlow != null)
            espressoFlow.Stop();

        if (brewAudio != null)
        {
            brewAudio.Stop();
        }

        socket.currentPortaFilter.coffeeDose = 0f;

        socket.currentPortaFilter.tamped = false;

        socket.currentPortaFilter.coffeeGroundsLoosePivot.SetActive(false);

        socket.currentPortaFilter.coffeeGroundsTampedPivot.SetActive(false);

        brewing = false;

        Debug.Log("Brewing Complete");
    }


}