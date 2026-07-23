using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SelectionAtmosphere : MonoBehaviour
{
    [SerializeField] private Transform platformRunes;
    [SerializeField] private Light[] torchLights;
    [SerializeField] private float runeRotationSpeed = 7f;
    [SerializeField] private float torchFlicker = 0.18f;

    private void Update()
    {
        if (platformRunes != null)
        {
            platformRunes.Rotate(0f, runeRotationSpeed * Time.deltaTime, 0f, Space.Self);
        }

        for (int i = 0; i < torchLights.Length; i++)
        {
            Light lightSource = torchLights[i];
            if (lightSource == null) continue;
            float noise = Mathf.PerlinNoise(Time.time * 5f, i * 2.37f);
            lightSource.intensity = 4.2f + (noise - 0.5f) * torchFlicker * 10f;
        }
    }
}

public sealed class MedievalButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Graphic glow;
    [SerializeField] private Color idle = new Color(0.63f, 0.39f, 0.15f, 0.45f);
    [SerializeField] private Color hover = new Color(1f, 0.72f, 0.25f, 0.95f);

    public void Configure(Graphic target)
    {
        glow = target;
        if (glow != null) glow.color = idle;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (glow != null) glow.color = hover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glow != null) glow.color = idle;
    }
}

public sealed class CircleGraphic : MaskableGraphic
{
    [SerializeField, Range(12, 128)] private int segments = 64;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = rectTransform.rect;
        Vector2 center = rect.center;
        float radius = Mathf.Min(rect.width, rect.height) * .5f;
        vh.AddVert(center, color, new Vector2(.5f, .5f));
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI * 2f * i / segments;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            vh.AddVert(center + direction * radius, color, (direction + Vector2.one) * .5f);
        }
        for (int i = 0; i < segments; i++)
        {
            vh.AddTriangle(0, i + 1, i + 2);
        }
    }
}
