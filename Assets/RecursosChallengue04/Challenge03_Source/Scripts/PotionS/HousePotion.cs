using UnityEngine;
using UnityEngine.SceneManagement;


public class HousePotion : MonoBehaviour
{

    public RecipeData houseRecipe;


    public string potionScene = "PotionScene";



    public void EnterHouse()
    {

        if (houseRecipe == null)
        {
            Debug.LogError(
                "Esta casa no tiene receta asignada"
            );

            return;
        }



        if (PotionDataManager.Instance == null)
        {
            Debug.LogError(
                "No existe PotionDataManager"
            );

            return;
        }



        PotionDataManager.Instance.SetRecipe(
            houseRecipe
        );


        SceneManager.LoadScene(
            potionScene
        );

    }

}