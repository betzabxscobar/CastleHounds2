using System.Collections.Generic;
using UnityEngine;

public class PotionManager : MonoBehaviour
{
    public static PotionManager Instance;

    [Header("Ingredientes de la escena")]
    public List<IngredientDrag> ingredients;

    [Header("Receta")]
    public List<string> recipe;

    [Header("Victoria")]
    public GameObject victoryCanvas;


    private List<string> current = new List<string>();



    private void Awake()
    {
        Instance = this;
    }



    public void AddIngredient(string ingredient)
    {
        current.Add(ingredient);

        Debug.Log("Añadido: " + ingredient);


        CheckRecipe();
    }



    void CheckRecipe()
    {

        // Todavía faltan ingredientes
        if (current.Count < recipe.Count)
            return;



        // Revisar orden
        for (int i = 0; i < recipe.Count; i++)
        {

            if (current[i] != recipe[i])
            {
                WrongRecipe();
                return;
            }

        }


        CompleteRecipe();

    }



    void CompleteRecipe()
    {
        Debug.Log("POCIÓN COMPLETADA");


        if (victoryCanvas != null)
            victoryCanvas.SetActive(true);


        // Por ahora dejamos solo el mensaje
        // luego conectamos regreso de escena

    }



    void WrongRecipe()
    {
        Debug.Log("RECETA INCORRECTA");


        ResetPotion();

    }



    void ResetPotion()
    {

        // Vaciar mezcla actual
        current.Clear();



        // Mostrar ingredientes otra vez
        foreach (IngredientDrag ingredient in ingredients)
        {

            if (ingredient != null)
            {
                ingredient.ResetIngredient();
            }

        }


        Debug.Log("Ingredientes reiniciados");

    }



    public void ClearPotion()
    {
        current.Clear();
    }
}