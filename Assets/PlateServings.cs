using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

public class PlateRecipeTrigger : MonoBehaviour
{
    [System.Serializable]
    public class IngredientRequirement
    {
        public string tagName;
        public int amount = 1;
    }

    [System.Serializable]
    public class Recipe
    {
        public string recipeName;
        public List<IngredientRequirement> ingredients = new();
        public GameObject resultPrefab;
    }

    [Header("Allowed Tags")]
    public List<string> allowedTags = new()
    {
        "CookedBacon",
        "FriedEgg",
        "Burger",
        "CheeseBurger",
        "CheeseBurgerSalad",
        "DoubleCheeseBurger"
    };

    [Header("Recipes")]
    public List<Recipe> recipes = new();

    public List<GameObject> trackedObjects = new();

    bool isCompleted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCompleted == true) return;

        GameObject validObject = GetValidTaggedParent(other.transform);

        if (validObject == null)
            return;

        if (!trackedObjects.Contains(validObject))
        {
            trackedObjects.Add(validObject);

            Debug.Log($"Added: {validObject.name} | Tag: {validObject.tag}");

            CheckRecipes();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isCompleted == true) return;

        GameObject validObject = GetValidTaggedParent(other.transform);

        if (validObject == null)
            return;

        trackedObjects.Remove(validObject);
        //Debug.Log("Removing tracked object: " + validObject.name);
    }

    private GameObject GetValidTaggedParent(Transform start)
    {
        Transform current = start;

        if (current != null)
        {
            if (allowedTags.Contains(current.tag))
            {
                return current.gameObject;
            }
            else if (allowedTags.Contains(current.parent.tag))
            {
                return current.parent.gameObject;
            }
        }

        return null;
    }

    private void CheckRecipes()
    {
        //Debug.Log("Checking Recipes...");

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null || recipe.recipeName == "") continue;
            Debug.Log("Check recipe completeness for " + recipe.recipeName);
            if (RecipeMatches(recipe))
            {
                Debug.Log("MATCH FOUND: " + recipe.recipeName);
                CompleteRecipe(recipe);
                return;
            }
        }
    }

    private bool RecipeMatches(Recipe recipe)
    {
        //Debug.Log("Checking recipe matches for " + recipe.recipeName);
        foreach (IngredientRequirement ingredient in recipe.ingredients)
        {
            int found = 0;

            foreach (GameObject obj in trackedObjects)
            {
                //Debug.Log("Tracked object tag: " + obj.tag);
                if (obj != null && obj.tag == ingredient.tagName)
                {
                    found++;
                }
            }

            if (found < ingredient.amount)
            {
                return false;
            }
        }
        //Debug.Log("Recipe matches tracked objects for :" + recipe.recipeName);
        return true;
    }

    private void CompleteRecipe(Recipe recipe)
    {
        isCompleted = true;
        //Debug.Log("Recipe completed!");

        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        foreach (GameObject obj in trackedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        // Destroy the plate itself
        Destroy(gameObject);

        trackedObjects.Clear();

        Instantiate(
            recipe.resultPrefab,
            spawnPosition,
            spawnRotation
        );


        Debug.Log("Created: " + recipe.resultPrefab.name);
    }
}