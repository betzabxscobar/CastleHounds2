using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CombinationSlot : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color correctColor = new Color(0.55f, 1f, 0.55f, 1f);
    [SerializeField] private Color incorrectColor = new Color(1f, 0.35f, 0.35f, 1f);

    private Vector3 normalScale = Vector3.one;

    private void Awake()
    {
        if (background == null)
        {
            background = GetComponent<Image>();
        }

        if (valueText == null)
        {
            valueText = GetComponentInChildren<TMP_Text>();
        }

        normalScale = transform.localScale;
        ResetVisual();
    }

    public void Configure(Image configuredBackground, TMP_Text configuredValueText)
    {
        background = configuredBackground != null ? configuredBackground : GetComponent<Image>();
        valueText = configuredValueText != null ? configuredValueText : GetComponentInChildren<TMP_Text>();
        normalScale = transform.localScale;
        ResetVisual();
    }

    public void SetValue(int value)
    {
        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
    }

    public void Clear()
    {
        if (valueText != null)
        {
            valueText.text = string.Empty;
        }

        ResetVisual();
    }

    public void SetCorrectFeedback()
    {
        SetColor(correctColor);
    }

    public void SetIncorrectFeedback()
    {
        SetColor(incorrectColor);
        transform.localScale = normalScale * 1.08f;
    }

    public void ResetVisual()
    {
        transform.localScale = normalScale;
        SetColor(normalColor);
    }

    private void SetColor(Color color)
    {
        if (background != null)
        {
            background.color = color;
        }
    }
}
