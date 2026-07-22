using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInPanel : MonoBehaviour
{
    public Image panel;

    public float esperar = 2f;
    public float duracion = 0.8f;

    [Range(0, 1)]
    public float opacidadFinal = 0.65f;

    void Start()
    {
        if (panel == null)
        {
            panel = GetComponent<Image>();
        }

        if (panel == null)
        {
            Debug.LogWarning("FadeInPanel necesita una referencia Image en el mismo objeto o en el campo panel.", this);
            enabled = false;
            return;
        }

        Color c = panel.color;
        c.a = 0;
        panel.color = c;

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(esperar);

        float tiempo = 0;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            Color c = panel.color;
            c.a = Mathf.Lerp(0, opacidadFinal, tiempo / duracion);

            panel.color = c;

            yield return null;
        }

        Color final = panel.color;
        final.a = opacidadFinal;
        panel.color = final;
    }
}
