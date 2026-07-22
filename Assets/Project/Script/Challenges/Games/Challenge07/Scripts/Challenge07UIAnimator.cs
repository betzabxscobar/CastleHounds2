using System.Collections;
using UnityEngine;

public sealed class Challenge07UIAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private CanvasGroup backgroundDarkener;
    [SerializeField] private RectTransform victoryPanel;
    [SerializeField, Min(0.05f)] private float openDuration = 0.25f;
    [SerializeField, Min(0.05f)] private float victoryDuration = 0.45f;

    private Coroutine animationRoutine;

    public void Configure(CanvasGroup root, CanvasGroup darkener, RectTransform victory)
    {
        rootCanvasGroup = root;
        backgroundDarkener = darkener;
        victoryPanel = victory;
    }

    public void PlayOpen()
    {
        StopAnimation();
        animationRoutine = StartCoroutine(OpenRoutine());
    }

    public void PlayVictory()
    {
        StopAnimation();
        animationRoutine = StartCoroutine(VictoryRoutine());
    }

    public void ResetVisuals()
    {
        StopAnimation();
        if (rootCanvasGroup != null) rootCanvasGroup.alpha = 1f;
        if (backgroundDarkener != null) backgroundDarkener.alpha = 0f;
        if (victoryPanel != null) victoryPanel.localScale = Vector3.one;
    }

    private IEnumerator OpenRoutine()
    {
        if (rootCanvasGroup == null) yield break;
        rootCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            rootCanvasGroup.alpha = Mathf.Clamp01(elapsed / openDuration);
            yield return null;
        }
        rootCanvasGroup.alpha = 1f;
        animationRoutine = null;
    }

    private IEnumerator VictoryRoutine()
    {
        if (victoryPanel != null) victoryPanel.localScale = Vector3.one * 0.82f;
        float elapsed = 0f;
        while (elapsed < victoryDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / victoryDuration), 3f);
            if (backgroundDarkener != null) backgroundDarkener.alpha = Mathf.Lerp(0f, 0.76f, t);
            if (victoryPanel != null) victoryPanel.localScale = Vector3.one * Mathf.Lerp(0.82f, 1f, t);
            yield return null;
        }
        animationRoutine = null;
    }

    private void StopAnimation()
    {
        if (animationRoutine == null) return;
        StopCoroutine(animationRoutine);
        animationRoutine = null;
    }
}
