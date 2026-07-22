using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IntroManager : MonoBehaviour
{
    public Image imagenHistoria;
    public TMP_Text textoHistoria;

    public Sprite[] imagenes;

    [TextArea(3, 10)]
    public string[] textos;

    public float velocidadTexto = 0.03f;

    public float duracionFade = 1f;
    public float duracionFadeImagenInicio = 2f;
    public float duracionFadeTextoInicio = 1.5f;
    public float tiempoAntesDeCambiarAutomatico = 5f;

    public AudioSource audioEscritura;
    public AudioSource audioPagina;
    public AudioSource audioInicio;

    public AudioClip typewriter;
    public AudioClip enterSound;
    public AudioClip sonidoInicio;

    [Range(0f, 1f)]
    public float volumenEscritura = 0.35f;

    [Range(0f, 1f)]
    public float volumenPagina = 0.7f;

    [Range(0f, 1f)]
    public float volumenInicio = 0.8f;

    public string siguienteNivel = "Demo";

    private int indice;
    private bool cambiando;
    private bool introFinished;
    private bool introCancelled;
    private Coroutine escribirCoroutine;
    private Coroutine historiaCoroutine;

    private void Start()
    {
        if (!ValidateRequiredReferences())
        {
            enabled = false;
            return;
        }

        indice = 0;
        PlayInitialSound();
        SetAlpha(imagenHistoria, 0f);
        SetAlpha(textoHistoria, 0f);
        imagenHistoria.sprite = imagenes[indice];

        historiaCoroutine = StartCoroutine(IniciarHistoria());
    }

    private void Update()
    {
        if (introCancelled || introFinished || cambiando)
        {
            return;
        }

        if (EnterPresionado())
        {
            StartCoroutine(CambiarPaginaOFinalizar());
        }
    }

    public void CancelIntro()
    {
        if (introFinished)
        {
            return;
        }

        introCancelled = true;
        cambiando = false;
        StopAudio();

        if (escribirCoroutine != null)
        {
            StopCoroutine(escribirCoroutine);
            escribirCoroutine = null;
        }

        if (historiaCoroutine != null)
        {
            StopCoroutine(historiaCoroutine);
            historiaCoroutine = null;
        }

        StopAllCoroutines();
    }

    public void LoadNextSceneOnce()
    {
        if (introFinished)
        {
            return;
        }

        introFinished = true;
        introCancelled = true;
        Time.timeScale = 1f;
        StopAudio();

        if (string.IsNullOrWhiteSpace(siguienteNivel))
        {
            Debug.LogError("IntroManager no tiene configurada la escena siguiente.", this);
            return;
        }

        SceneManager.LoadScene(siguienteNivel);
    }

    private IEnumerator IniciarHistoria()
    {
        yield return StartCoroutine(FadeImagen(0f, 1f, duracionFadeImagenInicio));
        yield return StartCoroutine(FadeTexto(0f, 1f, duracionFadeTextoInicio));
        StartWritingCurrentText();
    }

    private void StartWritingCurrentText()
    {
        if (introCancelled || introFinished)
        {
            return;
        }

        if (escribirCoroutine != null)
        {
            StopCoroutine(escribirCoroutine);
        }

        escribirCoroutine = StartCoroutine(EscribirTexto());
    }

    private IEnumerator EscribirTexto()
    {
        textoHistoria.text = "";

        if (audioEscritura != null && typewriter != null)
        {
            audioEscritura.clip = typewriter;
            audioEscritura.loop = true;
            audioEscritura.volume = volumenEscritura;
            audioEscritura.Play();
        }

        string textoActual = textos[indice];
        foreach (char letra in textoActual)
        {
            if (introCancelled || introFinished)
            {
                yield break;
            }

            textoHistoria.text += letra;
            yield return new WaitForSeconds(velocidadTexto);
        }

        if (audioEscritura != null)
        {
            audioEscritura.Stop();
        }

        yield return new WaitForSeconds(tiempoAntesDeCambiarAutomatico);

        if (!cambiando && !introCancelled && !introFinished)
        {
            StartCoroutine(CambiarPaginaOFinalizar());
        }
    }

    private IEnumerator CambiarPaginaOFinalizar()
    {
        if (introCancelled || introFinished)
        {
            yield break;
        }

        cambiando = true;

        if (audioEscritura != null)
        {
            audioEscritura.Stop();
        }

        if (escribirCoroutine != null)
        {
            StopCoroutine(escribirCoroutine);
            escribirCoroutine = null;
        }

        textoHistoria.text = "";

        if (audioPagina != null && enterSound != null)
        {
            audioPagina.PlayOneShot(enterSound, volumenPagina);
        }

        yield return StartCoroutine(FadeImagen(1f, 0f, duracionFade));

        indice++;
        if (indice >= imagenes.Length || indice >= textos.Length)
        {
            LoadNextSceneOnce();
            yield break;
        }

        imagenHistoria.sprite = imagenes[indice];

        yield return StartCoroutine(FadeImagen(0f, 1f, duracionFade));
        yield return StartCoroutine(FadeTexto(0f, 1f, duracionFade));

        cambiando = false;
        StartWritingCurrentText();
    }

    private IEnumerator FadeImagen(float inicio, float fin, float duracion)
    {
        Color color = imagenHistoria.color;
        float tiempo = 0f;
        float safeDuration = Mathf.Max(0.01f, duracion);

        while (tiempo < safeDuration)
        {
            if (introCancelled || introFinished)
            {
                yield break;
            }

            tiempo += Time.deltaTime;
            color.a = Mathf.Lerp(inicio, fin, tiempo / safeDuration);
            imagenHistoria.color = color;
            yield return null;
        }

        color.a = fin;
        imagenHistoria.color = color;
    }

    private IEnumerator FadeTexto(float inicio, float fin, float duracion)
    {
        Color color = textoHistoria.color;
        float tiempo = 0f;
        float safeDuration = Mathf.Max(0.01f, duracion);

        while (tiempo < safeDuration)
        {
            if (introCancelled || introFinished)
            {
                yield break;
            }

            tiempo += Time.deltaTime;
            color.a = Mathf.Lerp(inicio, fin, tiempo / safeDuration);
            textoHistoria.color = color;
            yield return null;
        }

        color.a = fin;
        textoHistoria.color = color;
    }

    private bool ValidateRequiredReferences()
    {
        if (imagenHistoria == null || textoHistoria == null)
        {
            Debug.LogError("IntroManager necesita Imagen Historia y Texto Historia.", this);
            return false;
        }

        if (imagenes == null || imagenes.Length == 0)
        {
            Debug.LogError("IntroManager necesita al menos una imagen.", this);
            return false;
        }

        if (textos == null || textos.Length == 0)
        {
            Debug.LogError("IntroManager necesita al menos un texto.", this);
            return false;
        }

        if (imagenes.Length != textos.Length)
        {
            Debug.LogWarning("IntroManager tiene diferente cantidad de imagenes y textos; se usara la cantidad menor.", this);
        }

        return true;
    }

    private void PlayInitialSound()
    {
        if (audioInicio != null && sonidoInicio != null)
        {
            audioInicio.volume = volumenInicio;
            audioInicio.PlayOneShot(sonidoInicio);
        }
    }

    private void StopAudio()
    {
        if (audioEscritura != null)
        {
            audioEscritura.Stop();
        }

        if (audioPagina != null)
        {
            audioPagina.Stop();
        }

        if (audioInicio != null)
        {
            audioInicio.Stop();
        }
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private bool EnterPresionado()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#else
        return false;
#endif
    }

    private void OnDisable()
    {
        StopAudio();
    }
}
