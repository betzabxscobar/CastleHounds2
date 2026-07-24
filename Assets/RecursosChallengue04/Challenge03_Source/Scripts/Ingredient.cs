using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public string ingredientName;


    private void OnMouseDown()
    {
        PotionManager.Instance.AddIngredient(ingredientName);
    }
}