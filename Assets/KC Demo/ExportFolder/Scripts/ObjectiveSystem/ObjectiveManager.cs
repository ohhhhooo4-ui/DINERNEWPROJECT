using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public TextMeshProUGUI objectiveText;

    public GameObject completionText;

    public PortaFilter portaFilter;

    public CoffeeCup tutorialCup;

    public CupDetectionZone cupDetectionZone;

    public MilkPitcher milkPitcher;

    private int currentObjective;

    private string[] objectives =
    {
        "Insert PortaFilter Into Grinder",
        "Grind 18g Of Coffee",
        "Tamp Coffee",
        "Insert PortaFilter Into Espresso Machine",
        "Place Cup Under Group Head",
        "Pull Espresso Shot",
        "Pour Milk Into Pitcher",
        "Steam Milk",
        "Pour Milk Into Espresso"
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateObjectiveText();
    }

    private void Update()
    {
        if (currentObjective == 0)
        {
            if (portaFilter != null &&
                portaFilter.insertedIntoGrinder)
            {
                CompleteObjective();
            }
        }

        if (currentObjective == 1)
        {
            if (portaFilter != null &&
                portaFilter.coffeeGround)
            {
                CompleteObjective();
            }
        }

        if (currentObjective == 2)
        {
            if (portaFilter != null &&
                portaFilter.tamped)
            {
                CompleteObjective();
            }
        }

        if (currentObjective == 3)
        {
            Debug.Log("Checking Machine Objective: " + portaFilter.insertedIntoMachine);

            if (portaFilter != null &&
                portaFilter.insertedIntoMachine)
            {
                Debug.Log("Machine Objective Complete");

                CompleteObjective();
            }
        }

        if (currentObjective == 4)
        {
            if (cupDetectionZone != null &&
                cupDetectionZone.currentCup != null)
            {
                CompleteObjective();
            }
        }

        if (currentObjective == 5)
        {
            if (cupDetectionZone != null &&
                cupDetectionZone.currentCup != null &&
                cupDetectionZone.currentCup.hasEspresso)
            {
                Debug.Log("Has Espresso: " + cupDetectionZone.currentCup.hasEspresso);
                CompleteObjective();
            }
        }

        if (currentObjective == 6)
        {
            if (milkPitcher != null &&
                milkPitcher.milkAmount > 0)
            {
                CompleteObjective();
            }
        }

        if (currentObjective == 7)
        {
            if (milkPitcher != null &&
                milkPitcher.milkTemperature >= 60f)
            {
                CompleteObjective();
            }
        }

        if (currentObjective == 8)
        {
            if (tutorialCup != null &&
                tutorialCup.latteComplete)
            {
                CompleteObjective();
            }
        }

    }


    public void CompleteObjective()
    {
        Debug.Log(
            "CompleteObjective Called. Current Objective = " +
            currentObjective);

        currentObjective++;

        if (currentObjective >= objectives.Length)
        {
            objectiveText.gameObject.SetActive(false);

            completionText.SetActive(true);

            return;
        }

        UpdateObjectiveText();
    }


    private void UpdateObjectiveText()
    {
        Debug.Log(
      "Current Objective Index: " +
      currentObjective);

        objectiveText.text =
            "CURRENT OBJECTIVE\n\n" +
            objectives[currentObjective];
    }
}