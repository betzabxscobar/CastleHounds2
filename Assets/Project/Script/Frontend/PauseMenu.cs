using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool CambiandoDeEscena { get; private set; }

    public GameObject panelPausa;

    private bool juegoPausado = false;

    void Start()
    {
        CambiandoDeEscena = false;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                ContinuarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    public void PausarJuego()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        Time.timeScale = 0f;
        juegoPausado = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }

    public void ContinuarJuego()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        Time.timeScale = 1f;
        juegoPausado = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }

    public void CambiarMusica()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
            AudioManager.Instance.ToggleMusica();
        }
    }

    public void CambiarSonidos()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
            AudioManager.Instance.ToggleSonidos();
        }
    }

    public void CambiarPantallaCompleta()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }

        Screen.fullScreen = !Screen.fullScreen;
    }

    public void VolverAlMenu()
    {
        if (CambiandoDeEscena)
        {
            return;
        }

        // Evita que el HUD reaparezca durante el fotograma en el que se
        // restablece el tiempo y se carga el menú principal.
        CambiandoDeEscena = true;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }

        SceneManager.LoadScene("MenuPrincipal");
    }
}
