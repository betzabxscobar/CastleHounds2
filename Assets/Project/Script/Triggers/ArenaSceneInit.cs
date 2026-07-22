using System.Collections;
using UnityEngine;

/// <summary>
/// Este script va en la escena "_DemoScene" (la arena de batalla).
/// Dispara el inicio de pelea automaticamente al cargar esa escena.
/// </summary>
public class ArenaSceneInit : MonoBehaviour
{
    private const string DogTag = "Player";
    private const string DogObjectName = "Player_Dog_Model";

    [SerializeField] private float retrasoSegundos = 0f;

    private void Start()
    {
        if (retrasoSegundos <= 0f)
        {
            GameEvents.RaiseFightStart();
        }
        else
        {
            Invoke(nameof(DispararInicioPelea), retrasoSegundos);
        }

        StartCoroutine(PrepareDogForBattleRoutine());
    }

    private void DispararInicioPelea()
    {
        GameEvents.RaiseFightStart();
    }

    // GameObject.FindWithTag("Player") no garantiza cual objeto devuelve si hay
    // mas de uno con ese tag en la escena (en _DemoScene, ademas del perro real
    // hay un objeto visual sin logica de jugador que tambien esta tageado
    // "Player"). Por eso se prioriza el candidato que realmente tenga
    // PlayerController; el nombre "Player_Dog_Model" queda como respaldo.
    private static GameObject FindDog()
    {
        GameObject byTag = GameObject.FindWithTag(DogTag);
        if (byTag != null && byTag.GetComponent<PlayerController>() != null)
        {
            return byTag;
        }

        GameObject byName = GameObject.Find(DogObjectName);
        if (byName != null)
        {
            return byName;
        }

        return byTag;
    }

    private IEnumerator PrepareDogForBattleRoutine()
    {
        // Se espera al menos un frame para dar tiempo a que todos los objetos de
        // la escena recien cargada terminen su Awake/OnEnable antes de tocarlos.
        yield return null;

        GameObject dog = FindDog();

        if (dog == null)
        {
            Debug.LogWarning("[ArenaInit] No se encontro el perro en la escena (ni por tag Player ni por nombre Player_Dog_Model).");
            yield break;
        }

        Time.timeScale = 1f;

        if (!dog.activeSelf)
        {
            dog.SetActive(true);
        }

        PlayerController playerController = dog.GetComponent<PlayerController>();
        CharacterController characterController = dog.GetComponent<CharacterController>();
        DogHealth dogHealth = dog.GetComponent<DogHealth>();

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (playerController != null)
        {
            // Limpia entrada/velocidad residual de la escena anterior y vuelve a
            // calcular el modo arena antes de reactivar la entrada.
            playerController.ResetControllerStateForScene();
        }

        if (dogHealth == null)
        {
            Debug.LogWarning("[ArenaInit] No se encontro DogHealth en el perro.");
        }

        Debug.Log("ArenaSceneInit: perro preparado correctamente para la batalla.");
    }
}
