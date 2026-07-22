using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PotionManager : MonoBehaviour
{
    public RecipeUI recipeUI;

    public static PotionManager Instance;


    public enum Difficulty
    {
        Facil,
        Medio,
        Dificil
    }


    [Header("Nivel actual")]
    public Difficulty currentDifficulty = Difficulty.Facil;



    [Header("Ingredientes de la escena")]
    public List<IngredientDrag> ingredients;



    [Header("Recetas fáciles")]
    public List<Recipe> easyRecipes;


    [Header("Recetas medias")]
    public List<Recipe> mediumRecipes;


    [Header("Recetas difíciles")]
    public List<Recipe> hardRecipes;



    [Header("Canvas")]
    public GameObject victoryCanvas;



    private List<string> current = new List<string>();


    private Recipe currentRecipe;



    private void Awake()
    {
        Instance = this;
    }



    private void Start()
    {
        GenerateRecipe();
    }



    // -------------------------------
    // GENERAR RECETA
    // -------------------------------

    void GenerateRecipe()
    {

        List<Recipe> selectedList = null;


        switch (currentDifficulty)
        {

            case Difficulty.Facil:
                selectedList = easyRecipes;
                break;


            case Difficulty.Medio:
                selectedList = mediumRecipes;
                break;


            case Difficulty.Dificil:
                selectedList = hardRecipes;
                break;

        }



        if (selectedList.Count == 0)
        {
            Debug.LogError("No hay recetas para este nivel");
            return;
        }



        currentRecipe =
        selectedList[
            Random.Range(0, selectedList.Count)
        ];
        recipeUI.ShowRecipe(currentRecipe);


        Debug.Log(
            "Nueva receta: "
            + currentRecipe.recipeName
        );


    }




    // -------------------------------
    // INGREDIENTE EN CALDERO
    // -------------------------------

    public void AddIngredient(string ingredient)
    {

        current.Add(ingredient);


        Debug.Log(
            "Añadido: " + ingredient
        );


        CheckRecipe();

    }





    // -------------------------------
    // COMPROBAR RECETA
    // -------------------------------

    void CheckRecipe()
    {

        if (current.Count < currentRecipe.ingredients.Count)
            return;



        for (int i = 0; i < currentRecipe.ingredients.Count; i++)
        {

            if (current[i] != currentRecipe.ingredients[i])
            {
                WrongRecipe();
                return;
            }

        }


        CompleteRecipe();

    }





    // -------------------------------
    // RECETA CORRECTA
    // -------------------------------

    void CompleteRecipe()
    {

        Debug.Log(
            "Completaste: "
            + currentRecipe.recipeName
        );


        current.Clear();



        if (currentDifficulty == Difficulty.Facil)
        {

            currentDifficulty =
            Difficulty.Medio;


            GenerateRecipe();

        }


        else if (currentDifficulty == Difficulty.Medio)
        {

            currentDifficulty =
            Difficulty.Dificil;


            GenerateRecipe();

        }


        else
        {

            Victory();

        }

    }





    // -------------------------------
    // RECETA INCORRECTA
    // -------------------------------

    void WrongRecipe()
    {

        Debug.Log(
            "RECETA INCORRECTA"
        );


        ResetPotion();


        // Nueva receta del mismo nivel
        GenerateRecipe();

    }




    // -------------------------------
    // REINICIAR
    // -------------------------------

    void ResetPotion()
    {

        current.Clear();



        foreach (IngredientDrag ingredient in ingredients)
        {

            if (ingredient != null)
            {
                ingredient.ResetIngredient();
            }

        }


    }





    // -------------------------------
    // VICTORIA FINAL
    // -------------------------------

    void Victory()
    {

        Debug.Log(
            "MINIJUEGO COMPLETADO"
        );


        if (victoryCanvas != null)
            victoryCanvas.SetActive(true);



        Invoke(
            nameof(ReturnScene),
            3f
        );

    }




    void ReturnScene()
    {

        SceneManager.LoadScene("Demo");

    }



}