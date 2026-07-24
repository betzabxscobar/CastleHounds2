using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "NuevaReceta",
    menuName = "Potion/Recipe"
)]
public class RecipeData : ScriptableObject
{

    public string recipeName;


    public List<string> ingredients = new List<string>();


    public ElementType rewardPower;

}