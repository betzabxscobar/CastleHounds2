using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private bool cargandoEscena;

    private void Click()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }

    public void Jugar()
    {
        if (cargandoEscena) return;
        cargandoEscena = true;
        Debug.Log("PRESIONASTE JUGAR");
        Click();
        ChallengeProgressManager.ResetStoredProgress();
        SceneManager.LoadScene("SeleccionAvatar");
    }

    public void IrOpciones()
    {
        if (cargandoEscena) return;
        cargandoEscena = true;
        Debug.Log("PRESIONASTE OPCIONES");
        Click();
        SceneManager.LoadScene("Opciones");
    }

    public void IrCreditos()
    {
        if (cargandoEscena) return;
        cargandoEscena = true;
        Debug.Log("PRESIONASTE CREDITOS");
        Click();
        SceneManager.LoadScene("Creditos");
    }

    public void EntrarAlJuego()
    {
        if (cargandoEscena) return;
        cargandoEscena = true;
        Click();
        SceneManager.LoadScene("Demo");
    }

    public void Reintentar()
    {
        if (cargandoEscena) return;
        cargandoEscena = true;
        Time.timeScale = 1f;
        Click();
        ChallengeProgressManager.ResetStoredProgress();
        SceneManager.LoadScene("Demo");
    }

    public void VolverMenu()
    {
        if (cargandoEscena) return;
        cargandoEscena = true;
        Time.timeScale = 1f;
        Click();
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void CambiarMusica()
    {
        Click();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusica();
        }
    }

    public void CambiarSonidos()
    {
        Click();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSonidos();
        }
    }

    public void CambiarPantallaCompleta()
    {
        Click();
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void SalirJuego()
    {
        Click();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
