using UnityEngine;


public class PotionPowerManager : MonoBehaviour
{

    public static PotionPowerManager Instance;


    public ElementType currentPower;



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



    public void SetPower(ElementType power)
    {

        currentPower = power;


        Debug.Log(
            "Poder guardado: " + currentPower
        );

    }



    public ElementType GetPower()
    {

        return currentPower;

    }



    public bool HasPower()
    {

        return currentPower != ElementType.None;

    }

}