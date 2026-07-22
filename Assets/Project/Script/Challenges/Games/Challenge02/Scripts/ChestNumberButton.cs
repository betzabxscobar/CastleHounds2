using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ChestNumberButton : MonoBehaviour
{
    [SerializeField, Range(0, 9)] private int numberValue;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text numberText;

    private Action<int> clickHandler;
    private Vector3 normalScale = Vector3.one;

    public int NumberValue => numberValue;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (numberText == null)
        {
            numberText = GetComponentInChildren<TMP_Text>();
        }

        normalScale = transform.localScale;
        RegisterClickListener();
        RefreshText();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Configure(int configuredNumberValue, Button configuredButton, TMP_Text configuredNumberText, Action<int> configuredClickHandler)
    {
        numberValue = Mathf.Clamp(configuredNumberValue, 0, 9);
        button = configuredButton != null ? configuredButton : GetComponent<Button>();
        numberText = configuredNumberText != null ? configuredNumberText : GetComponentInChildren<TMP_Text>();
        clickHandler = configuredClickHandler;
        normalScale = transform.localScale;
        RegisterClickListener();
        RefreshText();
    }

    public void SetClickHandler(Action<int> configuredClickHandler)
    {
        clickHandler = configuredClickHandler;
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
        {
            button.interactable = value;
        }
    }

    public void PlayPressedFeedback()
    {
        transform.localScale = normalScale * 1.06f;
        CancelInvoke(nameof(ResetScale));
        Invoke(nameof(ResetScale), 0.08f);
    }

    private void RegisterClickListener()
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(HandleClicked);
        button.onClick.AddListener(HandleClicked);
    }

    private void HandleClicked()
    {
        PlayPressedFeedback();
        clickHandler?.Invoke(numberValue);
    }

    private void ResetScale()
    {
        transform.localScale = normalScale;
    }

    private void RefreshText()
    {
        if (numberText != null)
        {
            numberText.text = numberValue.ToString();
        }
    }

    private void OnValidate()
    {
        numberValue = Mathf.Clamp(numberValue, 0, 9);
    }
}
