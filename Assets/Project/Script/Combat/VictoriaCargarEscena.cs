using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoriaCargarEscena : MonoBehaviour
{
    [SerializeField] private string nombreEscenaVictoria = "Ganaste";
    [SerializeField] private float retrasoSegundos = 1.5f;

    private bool cargaProgramada;

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
        if (cargaProgramada)
        {
            return;
        }

        cargaProgramada = true;

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaVictoria);
    }
}
