using UnityEngine;

public class PotionCauldron : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("Entró algo al caldero: " + other.name);


        IngredientDrag ingredient =
        other.GetComponent<IngredientDrag>();


        if (ingredient != null)
        {
            ingredient.PlaceInCauldron();
        }

    }
}