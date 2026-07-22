using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text texto;

    [TextArea(3, 10)]
    public string mensaje;

    public float velocidad = 0.03f;

    // Tiempo que espera antes de empezar a escribir
    public float esperarAntesDeEscribir = 2.8f;

    private bool escribiendo = false;
    private Coroutine escribirCoroutine;

    void Start()
    {
        if (texto == null)
        {
            Debug.LogError("TypewriterEffect necesita una referencia de texto.", this);
            enabled = false;
            return;
        }

        texto.text = "";
        escribirCoroutine = StartCoroutine(Escribir());
    }

    IEnumerator Escribir()
    {
        // Espera mientras aparece el panel negro
        yield return new WaitForSeconds(esperarAntesDeEscribir);

        escribiendo = true;

        foreach (char letra in mensaje)
        {
            texto.text += letra;
            yield return new WaitForSeconds(velocidad);
        }

        escribiendo = false;
        escribirCoroutine = null;
    }

    public bool Terminado()
    {
        return !escribiendo;
    }

    public void StopTyping()
    {
        if (escribirCoroutine != null)
        {
            StopCoroutine(escribirCoroutine);
            escribirCoroutine = null;
        }

        escribiendo = false;
    }

    public void CompleteImmediately()
    {
        StopTyping();

        if (texto != null)
        {
            texto.text = mensaje;
        }
    }

    private void OnDisable()
    {
        StopTyping();
    }
}
