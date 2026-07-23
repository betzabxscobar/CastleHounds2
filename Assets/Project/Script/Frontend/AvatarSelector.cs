using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AvatarSelector : MonoBehaviour
{
    [System.Serializable]
    public class AvatarPresentation
    {
        public string nombre;
        [Range(0, 1)] public float vida = 0.75f;
        [Range(0, 1)] public float ataque = 0.65f;
        [Range(0, 1)] public float velocidad = 0.8f;
        public GameObject modeloPreview;
        public Vector3 posicionLocal;
        public Vector3 rotacionLocal = new Vector3(0f, 180f, 0f);
        public Vector3 escalaLocal = Vector3.one;
        [Min(0.1f)] public float alturaPreview = 3.2f;
    }

    public const string AvatarSeleccionadoKey = "AvatarSeleccionado";

    public Image avatarImagen;
    public Sprite[] avatares;
    [Header("Presentacion 3D (solo esta escena)")]
    public Transform avatarSpawnPoint;
    public AvatarPresentation[] presentaciones;
    public GameObject modeloInicialEnEscena;
    public TMP_Text avatarNombre;
    public Image vidaBarra;
    public Image ataqueBarra;
    public Image velocidadBarra;

    private int avatarActual = 0;
    private bool cargandoHistoria;
    private GameObject modeloActual;

    void Start()
    {
        if (avatares == null || avatares.Length == 0)
        {
            Debug.LogError("AvatarSelector no tiene avatares configurados.", this);
            enabled = false;
            return;
        }

        modeloActual = modeloInicialEnEscena;
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
        if (avatares == null || avatares.Length == 0)
        {
            return;
        }

        avatarActual = Mathf.Clamp(avatarActual, 0, avatares.Length - 1);

        if (avatarImagen != null)
        {
            avatarImagen.sprite = avatares[avatarActual];
        }

        AvatarPresentation presentacion = ObtenerPresentacion();
        string nombre = presentacion != null && !string.IsNullOrWhiteSpace(presentacion.nombre)
            ? presentacion.nombre
            : avatares[avatarActual].name;

        if (avatarNombre != null)
        {
            avatarNombre.text = nombre.ToUpperInvariant();
        }

        ActualizarBarra(vidaBarra, presentacion != null ? presentacion.vida : 0.75f);
        ActualizarBarra(ataqueBarra, presentacion != null ? presentacion.ataque : 0.65f);
        ActualizarBarra(velocidadBarra, presentacion != null ? presentacion.velocidad : 0.8f);
        ActualizarModelo(presentacion);
    }

    private AvatarPresentation ObtenerPresentacion()
    {
        return presentaciones != null && avatarActual < presentaciones.Length
            ? presentaciones[avatarActual]
            : null;
    }

    private static void ActualizarBarra(Image barra, float valor)
    {
        if (barra != null)
        {
            barra.fillAmount = Mathf.Clamp01(valor);
        }
    }

    private void ActualizarModelo(AvatarPresentation presentacion)
    {
        if (modeloActual != null)
        {
            Destroy(modeloActual);
        }

        if (avatarSpawnPoint == null || presentacion == null || presentacion.modeloPreview == null)
        {
            return;
        }

        modeloActual = Instantiate(presentacion.modeloPreview, avatarSpawnPoint);
        modeloActual.name = $"Preview_{presentacion.nombre}";
        modeloActual.transform.localPosition = presentacion.posicionLocal;
        modeloActual.transform.localEulerAngles = presentacion.rotacionLocal;
        modeloActual.transform.localScale = presentacion.escalaLocal;

        foreach (MonoBehaviour behaviour in modeloActual.GetComponentsInChildren<MonoBehaviour>(true))
        {
            behaviour.enabled = false;
        }

        Animator animator = modeloActual.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        AjustarModeloALaPlataforma(modeloActual, presentacion.alturaPreview, presentacion.posicionLocal);
    }

    private static void AjustarModeloALaPlataforma(GameObject modelo, float alturaObjetivo, Vector3 desplazamiento)
    {
        Renderer[] renderers = modelo.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        if (bounds.size.y <= 0.001f)
        {
            return;
        }

        float factor = alturaObjetivo / bounds.size.y;
        modelo.transform.localScale *= factor;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float baseLocal = modelo.transform.parent.InverseTransformPoint(bounds.min).y;
        float centerLocalX = modelo.transform.parent.InverseTransformPoint(bounds.center).x;
        modelo.transform.localPosition += new Vector3(
            desplazamiento.x - centerLocalX,
            desplazamiento.y - baseLocal,
            desplazamiento.z);
    }

    private void ReproducirClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }
}
