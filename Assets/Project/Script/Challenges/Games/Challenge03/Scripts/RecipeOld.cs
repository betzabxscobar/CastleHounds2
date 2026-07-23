using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RecipeOld
{
    public string recipeName;

    public List<string> ingredients;

    [Header("Poder obtenido")]
    public ElementType rewardPower;
}