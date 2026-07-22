using TMPro;
using UnityEngine;


public class RecipeUI : MonoBehaviour
{

    public TextMeshProUGUI recipeText;


    public void ShowRecipe(Recipe recipe)
    {

        string text =
        "Preparar:\n\n";


        foreach (string ingredient in recipe.ingredients)
        {
            text += ingredient + "\n";
        }


        recipeText.text = text;

    }

}