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

    [Header("Preguntas")]
    public List<Pregunta> preguntas = new List<Pregunta>();

    private int preguntaActual = 0;
    private int respuestaSeleccionada = -1;
    private int respuestasCorrectas = 0;

    void Start()
    {
        // Asignar botones de respuesta
        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            int indice = i;
            botonesRespuesta[i].onClick.AddListener(() => SeleccionarRespuesta(indice));
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

        if (preguntaActual >= preguntas.Count)
        {
            textoPregunta.text = "Fin del desafío";
            textoIndicaciones.text = $"Correctas: {respuestasCorrectas}";

            botonContinuar.interactable = false;

            foreach (Button b in botonesRespuesta)
                b.interactable = false;

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
        }
        else
        {
            textoIndicaciones.text = "Respuesta incorrecta.";
        }

        preguntaActual++;

        CancelInvoke();
        Invoke(nameof(MostrarPregunta), 1f);
    }

    public void Salir()
    {
        panelJuego.SetActive(false);
    }
}