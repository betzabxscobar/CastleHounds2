using System.Collections.Generic;
using UnityEngine;

public class PotionManager : MonoBehaviour
{

    public static PotionManager Instance;


    public List<string> recipe;

    private List<string> currentMix = new List<string>();


    void Awake()
    {
        Instance = this;
    }



    public void AddIngredient(string ingredient)
    {

        currentMix.Add(ingredient);

        Debug.Log("Añadido: " + ingredient);


        CheckRecipe();

    }



    void CheckRecipe()
    {

        if (currentMix.Count != recipe.Count)
            return;



        for (int i = 0; i < recipe.Count; i++)
        {

            if (currentMix[i] != recipe[i])
            {
                WrongRecipe();
                return;
            }

        }


        CompletePotion();

    }



    void CompletePotion()
    {
        Debug.Log("POCIÓN COMPLETADA");

        // activar animación
        // sonido
        // UI victoria

    }



    void WrongRecipe()
    {
        Debug.Log("Receta incorrecta");

        currentMix.Clear();

        // reiniciar caldero

    }



}