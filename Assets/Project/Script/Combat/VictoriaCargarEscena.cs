using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoriaCargarEscena : MonoBehaviour
{
    [SerializeField] private string nombreEscenaVictoria = "Ganaste";
    [SerializeField] private float retrasoSegundos = 1.5f;

    private void OnEnable()
    {
        GameEvents.OnEnemyDefeated += ProgramarCargaEscena;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDefeated -= ProgramarCargaEscena;
    }

    private void ProgramarCargaEscena()
    {
        if (retrasoSegundos <= 0f)
        {
            CargarEscenaVictoria();
        }
        else
        {
            Invoke(nameof(CargarEscenaVictoria), retrasoSegundos);
        }
    }

    private void CargarEscenaVictoria()
    {
        SceneManager.LoadScene(nombreEscenaVictoria);
    }
}
