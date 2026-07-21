using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Muestra mensajes temporales en pantalla (HUD).
/// Canvas: Screen Space - Overlay. Estructura: Canvas > PanelMensaje > TextoMensaje.
/// </summary>
public class MessageHUD : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Canvas canvasHUD;
    [SerializeField] private TextMeshProUGUI textoMensaje;
    [SerializeField] private CanvasGroup panelGroup;

    [Header("Configuración")]
    [SerializeField] private float duracionDefault = 3f;
    [SerializeField] private float tiempoFadeIn = 0.2f;
    [SerializeField] private float tiempoFadeOut = 0.4f;

    private Coroutine _rutinaOcultar;

    private void Awake()
    {
        if (canvasHUD != null)
        {
            canvasHUD.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasHUD.sortingOrder = 100;
        }

        OcultarInstantaneo();
    }

    /// <summary>
    /// Muestra un mensaje en pantalla durante la duración indicada.
    /// </summary>
    public void ShowMessage(string mensaje, float duracion = -1f)
    {
        if (duracion <= 0f) duracion = duracionDefault;

        if (_rutinaOcultar != null)
        {
            StopCoroutine(_rutinaOcultar);
        }

        textoMensaje.text = mensaje;
        canvasHUD.gameObject.SetActive(true);

        _rutinaOcultar = StartCoroutine(FadeCoroutine(duracion));
    }

    /// <summary>
    /// Oculta el mensaje inmediatamente sin transición.
    /// </summary>
    public void OcultarInstantaneo()
    {
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
        }

        canvasHUD.gameObject.SetActive(false);
        textoMensaje.text = string.Empty;
    }

    private IEnumerator FadeCoroutine(float duracion)
    {
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < tiempoFadeIn)
            {
                elapsed += Time.unscaledDeltaTime;
                panelGroup.alpha = Mathf.Clamp01(elapsed / tiempoFadeIn);
                yield return null;
            }

            panelGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(duracion);

        if (panelGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < tiempoFadeOut)
            {
                elapsed += Time.unscaledDeltaTime;
                panelGroup.alpha = 1f - Mathf.Clamp01(elapsed / tiempoFadeOut);
                yield return null;
            }

            panelGroup.alpha = 0f;
        }

        canvasHUD.gameObject.SetActive(false);
        _rutinaOcultar = null;
    }
}
