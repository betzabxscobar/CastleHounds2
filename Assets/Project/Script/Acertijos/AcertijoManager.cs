using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcertijoManager : MonoBehaviour
{
    [Header("Juego Completo")]
    public GameObject panelJuego;

    [Header("Textos")]
    public TMP_Text textoPregunta;
    public TMP_Text textoProgreso;
    public TMP_Text textoIndicaciones;

    [Header("Respuestas")]
    public Button[] botonesRespuesta;
    public TMP_Text[] textosRespuesta;

    [Header("Botones")]
    public Button botonContinuar;
    public Button botonSalir;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoSeleccion;
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;

    [Header("Preguntas")]
    public List<Pregunta> preguntas = new List<Pregunta>();

    private int preguntaActual = 0;
    private int respuestaSeleccionada = -1;

    private int respuestasCorrectas = 0;
    private int respuestasIncorrectas = 0;

    void Start()
    {
        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            int indice = i;

            botonesRespuesta[i].onClick.AddListener(() =>
            {
                SeleccionarRespuesta(indice);
            });
        }

        botonContinuar.onClick.AddListener(Continuar);
        botonSalir.onClick.AddListener(Salir);

        MostrarPregunta();
    }

    void MostrarPregunta()
    {
        if (preguntas.Count == 0)
        {
            textoPregunta.text = "No existen preguntas.";
            return;
        }

        Pregunta pregunta = preguntas[preguntaActual];

        textoPregunta.text = pregunta.acertijo;

        for (int i = 0; i < 4; i++)
        {
            textosRespuesta[i].text = pregunta.respuestas[i];

            ColorBlock cb = botonesRespuesta[i].colors;
            cb.normalColor = Color.white;
            botonesRespuesta[i].colors = cb;
        }

        respuestaSeleccionada = -1;

        textoIndicaciones.text = "";
        textoProgreso.text = $"Pregunta {preguntaActual + 1}/{preguntas.Count}";
    }

    void SeleccionarRespuesta(int indice)
    {
        respuestaSeleccionada = indice;

        Reproducir(sonidoSeleccion);

        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            ColorBlock cb = botonesRespuesta[i].colors;

            if (i == indice)
                cb.normalColor = Color.yellow;
            else
                cb.normalColor = Color.white;

            botonesRespuesta[i].colors = cb;
        }
    }

    public void Continuar()
    {
        if (respuestaSeleccionada == -1)
        {
            textoIndicaciones.text = "Selecciona una respuesta.";
            return;
        }

        Pregunta pregunta = preguntas[preguntaActual];

        if (respuestaSeleccionada == pregunta.respuestaCorrecta)
        {
            respuestasCorrectas++;

            textoIndicaciones.text = "¡Respuesta correcta!";

            Reproducir(sonidoCorrecto);
        }
        else
        {
            respuestasIncorrectas++;

            textoIndicaciones.text = "Respuesta incorrecta.";

            Reproducir(sonidoIncorrecto);
        }

        preguntaActual++;

        if (preguntaActual >= preguntas.Count)
        {
            Invoke(nameof(FinalizarAcertijo), 1.5f);
            return;
        }

        CancelInvoke();
        Invoke(nameof(MostrarPregunta), 1f);
    }

    void FinalizarAcertijo()
    {
        Debug.Log("Acertijo terminado.");

        Debug.Log(
            "Correctas: " + respuestasCorrectas +
            " | Incorrectas: " + respuestasIncorrectas
        );

        if (respuestasCorrectas > respuestasIncorrectas)
        {
            Debug.Log("RESULTADO: ÉXITO. El jugador puede avanzar.");
            CerrarJuego();
        }
        else
        {
            Debug.Log("RESULTADO: FALLÓ. Reiniciando acertijo.");
            ReiniciarAcertijo();
        }
    }

    void ReiniciarAcertijo()
    {
        preguntaActual = 0;
        respuestasCorrectas = 0;
        respuestasIncorrectas = 0;

        MostrarPregunta();
    }

    public void Salir()
    {
        Debug.Log("Jugador salió del acertijo.");
        CerrarJuego();
    }

    void CerrarJuego()
    {
        Destroy(transform.root.gameObject);
    }

    void Reproducir(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}