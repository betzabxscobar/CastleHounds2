using UnityEngine;


public class PotionCauldron : MonoBehaviour
{

    public Transform ingredientPoint;


    public AudioSource audioSource;


    public AudioSource bubblingAudio;




    private void Start()
    {

        if (bubblingAudio != null)
        {
            bubblingAudio.Play();
        }

    }





    private void OnTriggerEnter(Collider other)
    {

        IngredientDrag ingredient =
        other.GetComponent<IngredientDrag>();



        if (ingredient != null)
        {


            if (audioSource != null && audioSource.clip != null)
            {

                audioSource.PlayOneShot(
                    audioSource.clip
                );

            }



            ingredient.EnterCauldron(
                ingredientPoint
            );


        }

    }

}