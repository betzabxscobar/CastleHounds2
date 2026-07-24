using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectorFotosAvatar : MonoBehaviour
{
    [System.Serializable]
    public class AvatarFoto
    {
        public string nombre;
        public Sprite foto;
        [Range(0, 1)] public float vida = 0.75f;
        [Range(0, 1)] public float ataque = 0.65f;
        [Range(0, 1)] public float velocidad = 0.8f;
    }

    [Header("Lista de avatares")]
    [SerializeField] private AvatarFoto[] avatares;

    [Header("Elementos de la interfaz")]
    [SerializeField] private Image imagenCentral;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private Image barraVida;
    [SerializeField] private Image barraAtaque;
    [SerializeField] private Image barraVelocidad;

    private int indiceActual;
    private bool cargandoHistoria;

    private void Start()
    {
        if (avatares == null || avatares.Length == 0)
        {
            Debug.LogError("No hay fotos configuradas.");
            return;
        }

        indiceActual = Mathf.Clamp(PlayerPrefs.GetInt(AvatarSelector.AvatarSeleccionadoKey, 0), 0, avatares.Length - 1);
        MostrarAvatar();
    }

    public void Siguiente()
    {
        if (avatares == null || avatares.Length == 0)
            return;

        ReproducirClick();

        indiceActual = (indiceActual + 1) % avatares.Length;
        MostrarAvatar();
    }

    public void Anterior()
    {
        if (avatares == null || avatares.Length == 0)
            return;

        ReproducirClick();

        indiceActual =
            (indiceActual - 1 + avatares.Length) % avatares.Length;

        MostrarAvatar();
    }

    public void Seleccionar()
    {
        if (cargandoHistoria || avatares == null || avatares.Length == 0)
        {
            return;
        }

        cargandoHistoria = true;
        ReproducirClick();

        PlayerPrefs.SetInt(AvatarSelector.AvatarSeleccionadoKey, indiceActual);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Historia");
    }

    private void MostrarAvatar()
    {
        AvatarFoto avatar = avatares[indiceActual];

        imagenCentral.gameObject.SetActive(true);
        imagenCentral.sprite = avatar.foto;
        imagenCentral.color = Color.white;
        imagenCentral.preserveAspect = true;

        if (textoNombre != null)
        {
            textoNombre.text = avatar.nombre.ToUpper();
        }

        ActualizarBarra(barraVida, avatar.vida);
        ActualizarBarra(barraAtaque, avatar.ataque);
        ActualizarBarra(barraVelocidad, avatar.velocidad);
    }

    private static void ActualizarBarra(Image barra, float valor)
    {
        if (barra != null)
        {
            barra.fillAmount = Mathf.Clamp01(valor);
        }
    }

    private void ReproducirClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }

    public int ObtenerIndiceSeleccionado()
    {
        return indiceActual;
    }
}
