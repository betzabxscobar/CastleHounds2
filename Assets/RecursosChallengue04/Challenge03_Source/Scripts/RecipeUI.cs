using TMPro;
using UnityEngine;


public class RecipeUI : MonoBehaviour
{

    public TextMeshProUGUI recipeTitle;
    public TextMeshProUGUI recipeText;



    public void ShowRecipe(RecipeData recipe)
    {

        if (recipe == null)
        {
            Debug.LogError(
                "No existe receta para mostrar"
            );

            return;
        }



        if (recipeTitle != null)
        {
            recipeTitle.text =
            recipe.recipeName;
        }



        if (recipeText != null)
        {

            string text = "";


            foreach (string ingredient in recipe.ingredients)
            {
                text += ingredient + "\n";
            }


            recipeText.text = text;

        }

    }

}