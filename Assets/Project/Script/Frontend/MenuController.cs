using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private void Click()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }

    public void Jugar()
    {
        Debug.Log("PRESIONASTE JUGAR");
        Click();
        SceneManager.LoadScene("SeleccionAvatar");
    }

    public void IrOpciones()
    {
        Debug.Log("PRESIONASTE OPCIONES");
        Click();
        SceneManager.LoadScene("Opciones");
    }

    public void IrCreditos()
    {
        Debug.Log("PRESIONASTE CREDITOS");
        Click();
        SceneManager.LoadScene("Creditos");
    }

    public void EntrarAlJuego()
    {
        Click();
        SceneManager.LoadScene("Demo");
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        Click();
        SceneManager.LoadScene("Demo");
    }

    public void VolverMenu()
    {
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
