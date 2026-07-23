using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


public class PotionManager : MonoBehaviour
{
    public RecipeUI recipeUI;

    public static PotionManager Instance;

    public UnityEvent<ElementType> OnPotionCompleted;


    private RecipeData currentRecipe;



    [Header("Ingredientes de la escena")]
    public List<IngredientDrag> ingredients;



    [Header("Canvas")]
    public GameObject victoryCanvas;



    [Header("Texto de victoria")]
    public TextMeshProUGUI victoryText;



    private List<string> current = new List<string>();





    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }





    private void Start()
    {
        LoadRecipe();
    }





    // -------------------------------
    // CARGAR RECETA DE LA CASA
    // -------------------------------

    void LoadRecipe()
    {

        if (PotionDataManager.Instance == null)
        {
            Debug.LogError(
                "No existe PotionDataManager"
            );

            return;
        }



        currentRecipe =
        PotionDataManager.Instance.GetRecipe();




        if (currentRecipe == null)
        {
            Debug.LogError(
                "La casa no envio ninguna receta"
            );

            return;
        }



        ShowRecipe();

    }





    // -------------------------------
    // MOSTRAR RECETA
    // -------------------------------

    void ShowRecipe()
    {

        if (recipeUI != null)
        {

            recipeUI.ShowRecipe(
                currentRecipe
            );

        }



        Debug.Log(
            "Receta cargada: "
            + currentRecipe.recipeName
        );

    }





    // -------------------------------
    // INGREDIENTE AL CALDERO
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

        if (currentRecipe == null)
            return;



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
    // RECETA COMPLETADA
    // -------------------------------

    void CompleteRecipe()
    {

        Debug.Log(
            "Completaste: "
            + currentRecipe.recipeName
        );



        Debug.Log(
            "Poder obtenido: "
            + currentRecipe.rewardPower
        );




        // Guardar poder para la pelea
        if (PotionPowerManager.Instance != null)
        {

            PotionPowerManager.Instance.SetPower(
                currentRecipe.rewardPower
            );

        }




        // Avisar a otros sistemas
        OnPotionCompleted?.Invoke(
            currentRecipe.rewardPower
        );




        current.Clear();



        Victory();

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
    // VICTORIA
    // -------------------------------

    void Victory()
    {

        Debug.Log(
            "MINIJUEGO COMPLETADO"
        );



        if (victoryCanvas != null)
        {

            victoryCanvas.SetActive(true);

        }




        if (victoryText != null)
        {

            victoryText.text =
            "¡Poción creada!\n\n" +
            "Obtuviste el poder:\n" +
            currentRecipe.rewardPower;

        }




        Invoke(
            nameof(ReturnScene),
            3f
        );

    }





    void ReturnScene()
    {

        Debug.Log(
            "Poder listo para combate"
        );


        // Aquí después se conectará
        // la carga de la pelea

    }

}