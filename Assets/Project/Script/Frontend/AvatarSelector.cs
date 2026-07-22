using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarSelector : MonoBehaviour
{
    public const string AvatarSeleccionadoKey = "AvatarSeleccionado";

    public Image avatarImagen;
    public Sprite[] avatares;

    private int avatarActual = 0;
    private bool cargandoHistoria;

    void Start()
    {
        if (avatares == null || avatares.Length == 0)
        {
            Debug.LogError("AvatarSelector no tiene avatares configurados.", this);
            enabled = false;
            return;
        }

        avatarActual = Mathf.Clamp(PlayerPrefs.GetInt(AvatarSeleccionadoKey, 0), 0, avatares.Length - 1);
        MostrarAvatar();
    }

    public void SiguienteAvatar()
    {
        if (avatares == null || avatares.Length == 0)
        {
            return;
        }

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
        if (avatares == null || avatares.Length == 0)
        {
            return;
        }

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
        if (cargandoHistoria)
        {
            return;
        }

        cargandoHistoria = true;
        ReproducirClick();

        PlayerPrefs.SetInt(AvatarSeleccionadoKey, avatarActual);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Historia");
    }

    private void MostrarAvatar()
    {
        if (avatarImagen != null && avatares != null && avatares.Length > 0)
        {
            avatarActual = Mathf.Clamp(avatarActual, 0, avatares.Length - 1);
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
