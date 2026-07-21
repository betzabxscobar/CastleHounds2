using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarSelector : MonoBehaviour
{
    public Image avatarImagen;
    public Sprite[] avatares;

    private int avatarActual = 0;

    void Start()
    {
        MostrarAvatar();
    }

    public void SiguienteAvatar()
    {
        ReproducirClick();

        avatarActual++;

        if (avatarActual >= avatares.Length)
        {
            avatarActual = 0;
        }

        MostrarAvatar();
    }

    public void AnteriorAvatar()
    {
        ReproducirClick();

        avatarActual--;

        if (avatarActual < 0)
        {
            avatarActual = avatares.Length - 1;
        }

        MostrarAvatar();
    }

    public void SeleccionarAvatar()
    {
        ReproducirClick();

        PlayerPrefs.SetInt("AvatarSeleccionado", avatarActual);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Demo");
    }

    private void MostrarAvatar()
    {
        if (avatarImagen != null && avatares.Length > 0)
        {
            avatarImagen.sprite = avatares[avatarActual];
        }
    }

    private void ReproducirClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }
}
