using TMPro;
using UnityEngine;


public class RecipeUI : MonoBehaviour
{

    public TextMeshProUGUI recipeTitle;
    public TextMeshProUGUI recipeText;



    public void ShowRecipe(Recipe recipe)
    {

        // Título de la poción
        recipeTitle.text = recipe.recipeName;



        // Ingredientes
        string text = "";


        foreach (string ingredient in recipe.ingredients)
        {
            text += ingredient + "\n";
        }


        recipeText.text = text;

    }

}