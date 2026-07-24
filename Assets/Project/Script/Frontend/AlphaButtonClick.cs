using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaButtonClick : MonoBehaviour
{
    [Range(0f, 1f)]
    public float minimoAlpha = 0.1f;

    void Start()
    {
        Image imagen = GetComponent<Image>();
        try
        {
            imagen.alphaHitTestMinimumThreshold = minimoAlpha;
        }
        catch (System.InvalidOperationException)
        {
            Debug.LogWarning($"AlphaButtonClick: textura de {gameObject.name} no es readable. Se omite alphaHitTest.", this);
        }
    }
}