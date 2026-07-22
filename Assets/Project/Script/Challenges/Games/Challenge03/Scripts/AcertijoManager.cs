using System;
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

    // El bridge (Challenge03GameBridge) se suscribe a estos callbacks para
    // enterarse del resultado en vez de que este script destruya objetos del
    // sistema de retos.
    public Action OnWon;
    public Action OnExitRequested;

    private int preguntaActual = 0;
    private int respuestaSeleccionada = -1;

    private int respuestasCorrectas = 0;
    private int respuestasIncorrectas = 0;

    private bool listenersRegistrados;

    private void Awake()
    {
        RegistrarListenersUnaVez();
    }

    private void RegistrarListenersUnaVez()
    {
        if (listenersRegistrados)
        {
            return;
        }

        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            int indice = i;
            botonesRespuesta[i].onClick.AddListener(() => SeleccionarRespuesta(indice));
        }

        botonContinuar.onClick.AddListener(Continuar);
        botonSalir.onClick.AddListener(Salir);

        listenersRegistrados = true;
    }

    // Llamado por Challenge03GameBridge cada vez que se entra a House (2).
    // Deja el acertijo como recien abierto: pregunta, conteos, selección,
    // colores, botones e invokes/audios pendientes quedan reiniciados.
    public void Show()
    {
        CancelInvoke();
        DetenerAudio();

        preguntaActual = 0;
        respuestasCorrectas = 0;
        respuestasIncorrectas = 0;
        respuestaSeleccionada = -1;

        HabilitarBotonesRespuesta(true);

        // AcertijoManager es hijo directo del root del prefab (AcertijoPanel,
        // junto a FondoOscuro y PanelAcertijo). Ese root empieza inactivo, y
        // si no se reactiva, todo el subarbol queda invisible aunque
        // panelJuego.activeSelf sea true.
        ActivarRaizDelPrefab(true);

        if (panelJuego != null)
        {
            panelJuego.SetActive(true);
        }

        MostrarPregunta();
    }

    // Llamado por Challenge03GameBridge al terminar el reto (ganado o
    // cancelado). Solo oculta el panel del Acertijo; no destruye nada del
    // sistema de retos.
    public void Hide()
    {
        CancelInvoke();
        DetenerAudio();

        if (panelJuego != null)
        {
            panelJuego.SetActive(false);
        }

        ActivarRaizDelPrefab(false);
    }

    private void ActivarRaizDelPrefab(bool activo)
    {
        GameObject raiz = transform.parent != null ? transform.parent.gameObject : gameObject;
        raiz.SetActive(activo);
    }

    private void MostrarPregunta()
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

    private void SeleccionarRespuesta(int indice)
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
            CancelInvoke();
            Invoke(nameof(FinalizarAcertijo), 1.5f);
            return;
        }

        CancelInvoke();
        Invoke(nameof(MostrarPregunta), 1f);
    }

    private void FinalizarAcertijo()
    {
        Debug.Log("Acertijo terminado.");

        Debug.Log(
            "Correctas: " + respuestasCorrectas +
            " | Incorrectas: " + respuestasIncorrectas
        );

        if (respuestasCorrectas > respuestasIncorrectas)
        {
            Debug.Log("RESULTADO: ÉXITO. El jugador puede avanzar.");
            OnWon?.Invoke();
        }
        else
        {
            Debug.Log("RESULTADO: FALLÓ. Reiniciando acertijo.");
            ReiniciarAcertijo();
        }
    }

    private void ReiniciarAcertijo()
    {
        preguntaActual = 0;
        respuestasCorrectas = 0;
        respuestasIncorrectas = 0;

        HabilitarBotonesRespuesta(true);
        MostrarPregunta();
    }

    public void Salir()
    {
        Debug.Log("Jugador salió del acertijo.");
        OnExitRequested?.Invoke();
    }

    private void HabilitarBotonesRespuesta(bool habilitados)
    {
        if (botonesRespuesta == null)
        {
            return;
        }

        foreach (Button boton in botonesRespuesta)
        {
            if (boton != null)
            {
                boton.interactable = habilitados;
            }
        }
    }

    private void DetenerAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void Reproducir(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
