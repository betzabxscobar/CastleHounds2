using UnityEngine;
using UnityEngine.SceneManagement;

public class PotionSceneManager : MonoBehaviour
{
    public static PotionSceneManager Instance;

    public string previousScene;


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


    public void ReturnToPreviousScene()
    {
        SceneManager.LoadScene(previousScene);
    }
}