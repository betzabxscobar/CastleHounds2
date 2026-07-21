using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    // Fade entre páginas
    public float duracionFade = 1f;

    // Fade inicial separado
    public float duracionFadeImagenInicio = 2f;
    public float duracionFadeTextoInicio = 1.5f;


    // Tiempo de espera antes del salto automático
    public float tiempoAntesDeCambiarAutomatico = 5f;



    // ===================== AUDIO =====================

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



    // ===================== ESCENA FINAL =====================

    public string siguienteNivel = "Demo";


    private int indice = 0;
    private bool cambiando = false;
    private Coroutine escribirCoroutine;



    void Start()
    {
        // Sonido inicial
        if (audioInicio != null && sonidoInicio != null)
        {
            audioInicio.volume = volumenInicio;
            audioInicio.PlayOneShot(sonidoInicio);
        }



        // Imagen transparente
        Color imagenColor = imagenHistoria.color;
        imagenColor.a = 0;
        imagenHistoria.color = imagenColor;


        // Texto transparente
        Color textoColor = textoHistoria.color;
        textoColor.a = 0;
        textoHistoria.color = textoColor;


        imagenHistoria.sprite = imagenes[indice];


        StartCoroutine(IniciarHistoria());
    }



    IEnumerator IniciarHistoria()
    {
        StartCoroutine(FadeImagen(0f, 1f, duracionFadeImagenInicio));


        yield return new WaitForSeconds(0.3f);


        yield return StartCoroutine(FadeTexto(0f, 1f, duracionFadeTextoInicio));


        escribirCoroutine = StartCoroutine(EscribirTexto());
    }



    void Update()
    {
        if (EnterPresionado())
        {
            if (!cambiando)
            {
                StartCoroutine(CambiarEscena());
            }
        }
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



    IEnumerator EscribirTexto()
    {
        textoHistoria.text = "";


        if (audioEscritura != null && typewriter != null)
        {
            audioEscritura.clip = typewriter;
            audioEscritura.loop = true;
            audioEscritura.volume = volumenEscritura;
            audioEscritura.Play();
        }



        foreach (char letra in textos[indice])
        {
            textoHistoria.text += letra;
            yield return new WaitForSeconds(velocidadTexto);
        }



        if (audioEscritura != null)
        {
            audioEscritura.Stop();
        }


        // Espera antes de cambiar automáticamente
        yield return new WaitForSeconds(tiempoAntesDeCambiarAutomatico);


        if (!cambiando)
        {
            StartCoroutine(CambiarEscena());
        }
    }



    IEnumerator CambiarEscena()
    {
        cambiando = true;


        if (audioEscritura != null)
            audioEscritura.Stop();


        if (escribirCoroutine != null)
            StopCoroutine(escribirCoroutine);



        textoHistoria.text = "";



        // Sonido de pasar página
        if (audioPagina != null && enterSound != null)
        {
            audioPagina.PlayOneShot(enterSound, volumenPagina);
        }



        // Fade actual
        yield return StartCoroutine(FadeImagen(1f, 0f, duracionFade));



        indice++;



        // ================= FIN DE INTRO =================

        if (indice >= imagenes.Length)
        {
            Debug.Log("Fin de introducción");


            yield return StartCoroutine(FadeImagen(1f, 0f, duracionFade));


            SceneManager.LoadScene(siguienteNivel);

            yield break;
        }



        imagenHistoria.sprite = imagenes[indice];



        yield return StartCoroutine(FadeImagen(0f, 1f, duracionFade));



        yield return StartCoroutine(FadeTexto(0f, 1f, duracionFade));



        escribirCoroutine = StartCoroutine(EscribirTexto());


        cambiando = false;
    }





    IEnumerator FadeImagen(float inicio, float fin, float duracion)
    {
        Color color = imagenHistoria.color;

        float tiempo = 0;


        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            color.a = Mathf.Lerp(inicio, fin, tiempo / duracion);

            imagenHistoria.color = color;

            yield return null;
        }


        color.a = fin;
        imagenHistoria.color = color;
    }




    IEnumerator FadeTexto(float inicio, float fin, float duracion)
    {
        Color color = textoHistoria.color;

        float tiempo = 0;


        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            color.a = Mathf.Lerp(inicio, fin, tiempo / duracion);

            textoHistoria.color = color;

            yield return null;
        }


        color.a = fin;
        textoHistoria.color = color;
    }
}
