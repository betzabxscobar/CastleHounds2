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

    [Header("Botones de respuestas")]
    public Button[] botonesRespuesta;
    public TMP_Text[] textosRespuesta;

    [Header("Botones")]
    public Button botonContinuar;
    public Button botonSalir;

    [Header("Preguntas")]
    public List<Pregunta> preguntas = new List<Pregunta>();

    private List<Pregunta> preguntasSeleccionadas = new List<Pregunta>();

    private int preguntaActual = 0;
    private int respuestasCorrectas = 0;

    private int respuestaSeleccionada = -1;
}