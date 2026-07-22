using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class RuneMemoryButton : MonoBehaviour
{
    [SerializeField] private int runeIndex;
    [SerializeField] private Button button;
    [SerializeField] private Image runeImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = new Color(1f, 0.84f, 0.35f, 1f);
    [SerializeField] private float highlightedScale = 1.15f;

    private Action<int> clickHandler;
    private Vector3 normalScale = Vector3.one;
    private Coroutine highlightCoroutine;

    public int RuneIndex => runeIndex;
    public Image RuneImage => runeImage;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (runeImage == null)
        {
            runeImage = GetComponentInChildren<Image>();
        }

        normalScale = transform.localScale;
        RegisterClickListener();
        ResetVisual();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Configure(int configuredRuneIndex, Button configuredButton, Image configuredRuneImage, Action<int> configuredClickHandler)
    {
        runeIndex = configuredRuneIndex;
        button = configuredButton != null ? configuredButton : GetComponent<Button>();
        runeImage = configuredRuneImage != null ? configuredRuneImage : GetComponentInChildren<Image>();
        clickHandler = configuredClickHandler;
        normalScale = transform.localScale;
        RegisterClickListener();
        ResetVisual();
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

    public Coroutine PlayHighlight(float duration, AudioSource audioSource, AudioClip audioClip)
    {
        if (!isActiveAndEnabled)
        {
            return null;
        }

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        highlightCoroutine = StartCoroutine(HighlightRoutine(duration, audioSource, audioClip));
        return highlightCoroutine;
    }

    public IEnumerator PlayHighlightRoutine(float duration, AudioSource audioSource, AudioClip audioClip)
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }

        yield return HighlightRoutine(duration, audioSource, audioClip);
    }

    public void ResetVisual()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }

        transform.localScale = normalScale;

        if (runeImage != null)
        {
            runeImage.color = normalColor;
        }
    }

    private IEnumerator HighlightRoutine(float duration, AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }

        transform.localScale = normalScale * highlightedScale;

        if (runeImage != null)
        {
            runeImage.color = highlightedColor;
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, duration));

        transform.localScale = normalScale;

        if (runeImage != null)
        {
            runeImage.color = normalColor;
        }

        highlightCoroutine = null;
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
        clickHandler?.Invoke(runeIndex);
    }
}
