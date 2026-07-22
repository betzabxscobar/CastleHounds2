using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcertijoManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelJuego;
    public GameObject panelFinal;

    [Header("Texto")]
    public TMP_Text textoPregunta;
    public TMP_Text textoProgreso;

    [Header("Botones")]
    public Button[] botonesRespuesta;
    public TMP_Text[] textosRespuesta;

    public Button botonContinuar;
    public Button botonSalir;

    [Header("Preguntas")]
    public List<Pregunta> preguntas = new();

    private int preguntaActual = 0;
    private int respuestaSeleccionada = -1;

    void Start()
    {
        MostrarPregunta();

        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            int indice = i;
            botonesRespuesta[i].onClick.AddListener(() => SeleccionarRespuesta(indice));
        }
    }

    void MostrarPregunta()
    {
        Pregunta p = preguntas[preguntaActual];

        textoPregunta.text = p.acertijo;

        for (int i = 0; i < 4; i++)
        {
            textosRespuesta[i].text = p.respuestas[i];
        }

        textoProgreso.text = $"Pregunta {preguntaActual + 1}/{preguntas.Count}";
    }

    void SeleccionarRespuesta(int indice)
    {
        respuestaSeleccionada = indice;

        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            ColorBlock colores = botonesRespuesta[i].colors;

            if (i == indice)
                colores.normalColor = Color.yellow;
            else
                colores.normalColor = Color.white;

            botonesRespuesta[i].colors = colores;
        }
    }
}