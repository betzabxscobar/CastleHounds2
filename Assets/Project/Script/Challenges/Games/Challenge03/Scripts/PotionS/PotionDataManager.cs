using UnityEngine;


public class PotionDataManager : MonoBehaviour
{

    public static PotionDataManager Instance;


    public RecipeData selectedRecipe;



    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }



    public void SetRecipe(RecipeData recipe)
    {
        selectedRecipe = recipe;
    }



    public RecipeData GetRecipe()
    {
        return selectedRecipe;
    }

}