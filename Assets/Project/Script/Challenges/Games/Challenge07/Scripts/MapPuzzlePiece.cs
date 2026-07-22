using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public sealed class MapPuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Destino")]
    [SerializeField] private RectTransform targetPosition;
    [SerializeField] private Image pieceImage;
    [SerializeField] private Sprite pieceSprite;
    [SerializeField, Min(1f)] private float snapDistance = 80f;

    [Header("Animación")]
    [SerializeField, Min(1f)] private float dragScale = 1.08f;
    [SerializeField, Min(0.01f)] private float snapDuration = 0.22f;
    [SerializeField, Min(0.01f)] private float returnDuration = 0.3f;

    [Header("Efectos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip wrongClip;
    [SerializeField] private ParticleSystem correctParticles;
    [SerializeField] private Shadow pieceShadow;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Challenge07PuzzleController puzzleController;
    private Vector2 initialPosition;
    private Vector2 initialSize;
    private Vector3 initialScale;
    private Transform initialParent;
    private int initialSiblingIndex;
    private bool initialStateSaved;
    private bool isPlaced;
    private bool isAnimating;
    private Coroutine movementRoutine;

    public bool IsPlaced => isPlaced;
    public RectTransform TargetPosition => targetPosition;

    private void Awake()
    {
        CacheReferences();
        ApplyPieceSprite();
    }

    private void OnDisable()
    {
        StopMovement();
    }

    public void Configure(Challenge07PuzzleController controller)
    {
        puzzleController = controller;
        CacheReferences();
        ApplyPieceSprite();
        SaveInitialPosition();
    }

    public void ConfigureRuntime(
        Challenge07PuzzleController controller,
        RectTransform target,
        AudioSource configuredAudioSource,
        AudioClip configuredPickupClip,
        AudioClip configuredCorrectClip,
        AudioClip configuredWrongClip,
        ParticleSystem configuredParticles,
        Sprite configuredSprite = null)
    {
        puzzleController = controller;
        targetPosition = target;
        audioSource = configuredAudioSource;
        pickupClip = configuredPickupClip;
        correctClip = configuredCorrectClip;
        wrongClip = configuredWrongClip;
        correctParticles = configuredParticles;
        pieceSprite = configuredSprite;
        CacheReferences();
        ApplyPieceSprite();
        SaveInitialPosition();
    }

    public void SaveInitialPosition()
    {
        CacheReferences();
        initialPosition = rectTransform.anchoredPosition;
        initialSize = rectTransform.sizeDelta;
        initialScale = rectTransform.localScale;
        initialParent = rectTransform.parent;
        initialSiblingIndex = rectTransform.GetSiblingIndex();
        initialStateSaved = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }

        StopMovement();
        rectTransform.SetAsLastSibling();
        rectTransform.localScale = initialScale * dragScale;
        canvasGroup.alpha = 0.94f;
        canvasGroup.blocksRaycasts = false;
        SetShadowStrength(true);
        PlayOneShot(pickupClip);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / Mathf.Max(0.01f, rootCanvas.scaleFactor);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced || isAnimating)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        SetShadowStrength(false);

        if (targetPosition == null)
        {
            Debug.LogWarning($"La pieza {name} no tiene Target Position asignado.", this);
            StartReturnAnimation();
            return;
        }

        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        Vector2 pieceScreenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, rectTransform.position);
        Vector2 targetScreenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, targetPosition.position);

        if (Vector2.Distance(pieceScreenPosition, targetScreenPosition) <= snapDistance)
        {
            movementRoutine = StartCoroutine(SnapRoutine());
        }
        else
        {
            PlayOneShot(wrongClip);
            movementRoutine = StartCoroutine(ReturnRoutine());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPlaced && !isAnimating && rectTransform != null)
            rectTransform.localScale = initialScale * 1.025f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPlaced && !isAnimating && rectTransform != null)
            rectTransform.localScale = initialScale;
    }

    public void PlayHint(float duration = 0.8f)
    {
        if (!isPlaced && !isAnimating && targetPosition != null)
            movementRoutine = StartCoroutine(HintRoutine(Mathf.Max(0.2f, duration)));
    }

    public void ResetPiece()
    {
        CacheReferences();
        if (!initialStateSaved)
        {
            SaveInitialPosition();
        }

        StopMovement();
        isPlaced = false;
        isAnimating = false;
        if (initialParent != null && rectTransform.parent != initialParent)
            rectTransform.SetParent(initialParent, false);
        rectTransform.anchoredPosition = initialPosition;
        rectTransform.sizeDelta = initialSize;
        rectTransform.localScale = initialScale;
        rectTransform.SetSiblingIndex(Mathf.Min(initialSiblingIndex, rectTransform.parent.childCount - 1));
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        SetShadowStrength(false);

        if (correctParticles != null)
        {
            correctParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private IEnumerator SnapRoutine()
    {
        isAnimating = true;
        canvasGroup.blocksRaycasts = false;
        Vector3 startPosition = rectTransform.position;
        Vector3 startScale = rectTransform.localScale;
        Vector2 startSize = rectTransform.sizeDelta;
        float elapsed = 0f;

        while (elapsed < snapDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = SmoothStep(elapsed / snapDuration);
            rectTransform.position = Vector3.LerpUnclamped(startPosition, targetPosition.position, t);
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, initialScale * 1.03f, t);
            rectTransform.sizeDelta = Vector2.LerpUnclamped(startSize, targetPosition.rect.size, t);
            yield return null;
        }

        rectTransform.position = targetPosition.position;
        rectTransform.localScale = initialScale;
        rectTransform.sizeDelta = targetPosition.rect.size;
        isPlaced = true;
        isAnimating = false;
        movementRoutine = null;
        PlayOneShot(correctClip);

        if (correctParticles != null)
        {
            correctParticles.transform.position = targetPosition.position;
            correctParticles.Play(true);
        }

        if (puzzleController != null)
        {
            puzzleController.NotifyPiecePlaced();
        }
    }

    private IEnumerator ReturnRoutine()
    {
        isAnimating = true;
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;
        Vector2 startSize = rectTransform.sizeDelta;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / returnDuration);
            float t = SmoothStep(normalized);
            float rejection = Mathf.Sin(normalized * Mathf.PI * 4f) * (1f - normalized) * 10f;
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, initialPosition, t) + Vector2.right * rejection;
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, initialScale, t);
            rectTransform.sizeDelta = Vector2.LerpUnclamped(startSize, initialSize, t);
            yield return null;
        }

        rectTransform.anchoredPosition = initialPosition;
        rectTransform.localScale = initialScale;
        rectTransform.sizeDelta = initialSize;
        rectTransform.SetSiblingIndex(Mathf.Min(initialSiblingIndex, rectTransform.parent.childCount - 1));
        canvasGroup.blocksRaycasts = true;
        isAnimating = false;
        movementRoutine = null;
    }

    private IEnumerator HintRoutine(float duration)
    {
        isAnimating = true;
        Vector3 originalScale = targetPosition.localScale;
        Image targetImage = targetPosition.GetComponent<Image>();
        Color originalColor = targetImage != null ? targetImage.color : Color.white;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float pulse = (Mathf.Sin(elapsed * 12f) + 1f) * 0.5f;
            targetPosition.localScale = originalScale * Mathf.Lerp(1f, 1.09f, pulse);
            if (targetImage != null)
                targetImage.color = Color.Lerp(originalColor, new Color(1f, 0.78f, 0.2f, 0.85f), pulse);
            yield return null;
        }

        targetPosition.localScale = originalScale;
        if (targetImage != null) targetImage.color = originalColor;
        isAnimating = false;
        movementRoutine = null;
    }

    private void StartReturnAnimation()
    {
        PlayOneShot(wrongClip);
        StopMovement();
        movementRoutine = StartCoroutine(ReturnRoutine());
    }

    private bool CanDrag()
    {
        return !isPlaced && !isAnimating && rootCanvas != null && rectTransform != null;
    }

    private void CacheReferences()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (rootCanvas == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            rootCanvas = canvas != null ? canvas.rootCanvas : null;
        }
        if (pieceShadow == null) pieceShadow = GetComponent<Shadow>();
        if (pieceImage == null) pieceImage = GetComponent<Image>();
    }

    private void ApplyPieceSprite()
    {
        if (pieceImage == null) return;
        if (pieceSprite != null) pieceImage.sprite = pieceSprite;
        pieceImage.preserveAspect = true;
    }

    private void StopMovement()
    {
        if (movementRoutine != null)
        {
            StopCoroutine(movementRoutine);
            movementRoutine = null;
        }
        isAnimating = false;
    }

    private void SetShadowStrength(bool dragging)
    {
        if (pieceShadow == null) return;
        pieceShadow.effectDistance = dragging ? new Vector2(10f, -10f) : new Vector2(5f, -5f);
        pieceShadow.effectColor = dragging ? new Color(0f, 0f, 0f, 0.8f) : new Color(0f, 0f, 0f, 0.5f);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private static float SmoothStep(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

    private void OnValidate()
    {
        snapDistance = Mathf.Max(1f, snapDistance);
        dragScale = Mathf.Max(1f, dragScale);
        snapDuration = Mathf.Max(0.01f, snapDuration);
        returnDuration = Mathf.Max(0.01f, returnDuration);
    }
}
