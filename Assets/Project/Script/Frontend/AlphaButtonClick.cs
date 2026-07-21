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
        imagen.alphaHitTestMinimumThreshold = minimoAlpha;
    }
}