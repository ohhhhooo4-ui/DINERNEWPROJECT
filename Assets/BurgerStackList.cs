using System.Collections.Generic;
using UnityEngine;

public class BurgerAssembler : MonoBehaviour
{
    [Header("Finished Burgers")]
    public GameObject regularBurgerPrefab;
    public GameObject cheeseBurgerPrefab;
    public GameObject cheeseBurgerWithSaladPrefab;
    public GameObject doubleCheeseBurgerPrefab;

    public List<string> ingredients = new List<string>();
    public List<string> validIngredientTags = new();

    private void Start()
    {
        ingredients.Add("BottomBun");
    }

    private void OnTriggerEnter(Collider other)
    {
        string tagName = other.tag;
        if (validIngredientTags.Contains(tagName) == false) return;

        if (!ingredients.Contains(tagName) ||
            tagName == "CookedBurger" ||
            tagName == "Cheese")
        {
            ingredients.Add(tagName);
            other.transform.parent = transform;

            CheckRecipe();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (ingredients.Contains(other.gameObject.tag))
        {
            ingredients.Remove(other.gameObject.tag);
            other.transform.parent = null;
        }
    }

    void CheckRecipe()
    {
        int burgers = Count("CookedBurger");
        int cheese = Count("Cheese");
        int salad = Count("Salad");
        int tops = Count("TopBun");

        if (tops == 0)
            return;

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        if (burgers == 1 && cheese == 0 && salad == 0)
        {
            SpawnBurger(regularBurgerPrefab, pos, rot);
        }
        else if (burgers == 1 && cheese == 1 && salad == 0)
        {
            SpawnBurger(cheeseBurgerPrefab, pos, rot);
        }
        else if (burgers == 1 && cheese == 0 && salad == 1)
        {
            SpawnBurger(cheeseBurgerWithSaladPrefab, pos, rot);
        }
        else if (burgers == 2 && cheese == 2 && salad == 0)
        {
            SpawnBurger(doubleCheeseBurgerPrefab, pos, rot);
        }
    }

    int Count(string ingredient)
    {
        int count = 0;

        foreach (string item in ingredients)
        {
            if (item == ingredient)
                count++;
        }

        return count;
    }

    void SpawnBurger(GameObject burgerPrefab, Vector3 pos, Quaternion rot)
    {
        Instantiate(burgerPrefab, pos, rot);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Destroy(gameObject);
    }
}