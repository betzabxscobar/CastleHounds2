using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ChestCombinationPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Typography")]
    [Tooltip("Fuente TMP medieval. Si queda vacía, se intentará cargar desde Resources/Challenges/Challenge02/Fonts/MedievalFont.")]
    [SerializeField] private TMP_FontAsset medievalFont;
    [SerializeField] private string medievalFontResourcesPath = "Challenges/Challenge02/Fonts/MedievalFont";

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text clueText;
    [SerializeField] private TMP_Text statusText;

    [Header("Images")]
    [SerializeField] private Image chestImage;
    [SerializeField] private Image lockImage;
    [SerializeField] private Image keyImage;
    [SerializeField] private Image coinImage;

    [Header("Sprites")]
    [SerializeField] private Sprite closedChestSprite;
    [SerializeField] private Sprite openChestSprite;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Sprite mainFrameSprite;
    [SerializeField] private Sprite clueScrollSprite;
    [SerializeField] private Sprite numberSlotSprite;
    [SerializeField] private Sprite confirmButtonSprite;
    [SerializeField] private Sprite clearButtonSprite;
    [SerializeField] private Sprite retryButtonSprite;
    [SerializeField] private Sprite exitButtonSprite;
    [SerializeField] private Sprite keySprite;
    [SerializeField] private Sprite coinSprite;

    [Header("Controls")]
    [SerializeField] private CombinationSlot[] combinationSlots;
    [SerializeField] private ChestNumberButton[] numberButtons;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private AudioClip numberPressClip;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip incorrectClip;
    [SerializeField] private AudioClip chestOpeningClip;
    [SerializeField] private AudioClip victoryClip;

    private const float FrameWidth = 1070f;
    private const float FrameHeight = 756f;

    private static readonly Color TitleColor = new Color(0.95f, 0.78f, 0.35f, 1f);
    private static readonly Color ParchmentTextColor = new Color(0.17f, 0.09f, 0.025f, 1f);
    private static readonly Color StatusColor = new Color(1f, 0.88f, 0.58f, 1f);
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.78f);

    private Action<int> numberHandler;
    private Action confirmHandler;
    private Action clearHandler;
    private Action retryHandler;
    private Action exitHandler;

    private RectTransform mainFrameRect;
    private bool showRequested;

    public AudioSource SfxSource => sfxSource;
    public AudioSource AmbienceSource => ambienceSource;
    public AudioClip AmbienceClip => ambienceClip;
    public AudioClip NumberPressClip => numberPressClip;
    public AudioClip CorrectClip => correctClip;
    public AudioClip IncorrectClip => incorrectClip;
    public AudioClip ChestOpeningClip => chestOpeningClip;
    public AudioClip VictoryClip => victoryClip;

    private void Awake()
    {
        EnsureBuilt();
        RegisterButtonListeners();

        if (showRequested)
        {
            ApplyVisibleState();
        }
        else
        {
            Hide();
        }
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
        ClearNumberHandlers();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateResponsiveScale();
    }

    public void ConfigureSprites(
        Sprite configuredClosedChestSprite,
        Sprite configuredOpenChestSprite,
        Sprite configuredLockSprite,
        Sprite configuredMainFrameSprite,
        Sprite configuredClueScrollSprite,
        Sprite configuredNumberSlotSprite,
        Sprite configuredConfirmButtonSprite,
        Sprite configuredClearButtonSprite,
        Sprite configuredRetryButtonSprite,
        Sprite configuredExitButtonSprite,
        Sprite configuredKeySprite,
        Sprite configuredCoinSprite)
    {
        closedChestSprite = configuredClosedChestSprite;
        openChestSprite = configuredOpenChestSprite;
        lockSprite = configuredLockSprite;
        mainFrameSprite = configuredMainFrameSprite;
        clueScrollSprite = configuredClueScrollSprite;
        numberSlotSprite = configuredNumberSlotSprite;
        confirmButtonSprite = configuredConfirmButtonSprite;
        clearButtonSprite = configuredClearButtonSprite;
        retryButtonSprite = configuredRetryButtonSprite;
        exitButtonSprite = configuredExitButtonSprite;
        keySprite = configuredKeySprite;
        coinSprite = configuredCoinSprite;
    }

    public void ConfigureAudioReferences(
        AudioSource configuredSfxSource,
        AudioSource configuredAmbienceSource,
        AudioClip configuredAmbienceClip,
        AudioClip configuredNumberPressClip,
        AudioClip configuredCorrectClip,
        AudioClip configuredIncorrectClip,
        AudioClip configuredChestOpeningClip,
        AudioClip configuredVictoryClip)
    {
        sfxSource = configuredSfxSource;
        ambienceSource = configuredAmbienceSource;
        ambienceClip = configuredAmbienceClip;
        numberPressClip = configuredNumberPressClip;
        correctClip = configuredCorrectClip;
        incorrectClip = configuredIncorrectClip;
        chestOpeningClip = configuredChestOpeningClip;
        victoryClip = configuredVictoryClip;

        ConfigureAudioSource(sfxSource, false);
        ConfigureAudioSource(ambienceSource, true);
    }

    public void SetCallbacks(Action<int> onNumber, Action onConfirm, Action onClear, Action onRetry, Action onExit)
    {
        numberHandler = onNumber;
        confirmHandler = onConfirm;
        clearHandler = onClear;
        retryHandler = onRetry;
        exitHandler = onExit;

        if (numberButtons == null)
        {
            return;
        }

        foreach (ChestNumberButton numberButton in numberButtons)
        {
            if (numberButton != null)
            {
                numberButton.SetClickHandler(numberHandler);
            }
        }
    }

    public void Show()
    {
        showRequested = true;
        ActivateSelfAndParents(root != null ? root.transform : transform);
        EnsureBuilt();
        RegisterButtonListeners();
        ApplyVisibleState();
        UpdateResponsiveScale();
    }

    public void Hide()
    {
        showRequested = false;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
        }

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void EnsureAudioSourcesReady()
    {
        EnsureBuilt();

        if (root == null)
        {
            return;
        }

        FindOrCreateAudioSources();

        if (sfxSource != null)
        {
            sfxSource.enabled = true;
            ConfigureAudioSource(sfxSource, false);
        }

        if (ambienceSource != null)
        {
            ambienceSource.enabled = true;
            ConfigureAudioSource(ambienceSource, true);
        }
    }

    public void ResetForNewAttempt(string configuredClueText)
    {
        SetTitle("COFRE CON COMBINACIÓN");
        SetClue(configuredClueText);
        SetStatus("Introduce tres números.");
        SetChestOpen(false);
        SetRewardsVisible(false);
        SetRetryVisible(false);
        SetNumberButtonsInteractable(true);
        SetConfirmClearInteractable(true);
        SetExitInteractable(true);
        ClearSlots();
        ResetSlotsVisual();
    }

    public void SetTitle(string text)
    {
        if (titleText != null)
        {
            titleText.text = text;
        }
    }

    public void SetClue(string text)
    {
        if (clueText != null)
        {
            clueText.text = text;
        }
    }

    public void SetStatus(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }

    public void SetDigits(int[] digits, int count)
    {
        if (combinationSlots == null)
        {
            return;
        }

        for (int i = 0; i < combinationSlots.Length; i++)
        {
            CombinationSlot slot = combinationSlots[i];
            if (slot == null)
            {
                continue;
            }

            if (digits != null && i < count)
            {
                slot.SetValue(digits[i]);
            }
            else
            {
                slot.Clear();
            }
        }
    }

    public void ClearSlots()
    {
        if (combinationSlots == null)
        {
            return;
        }

        foreach (CombinationSlot slot in combinationSlots)
        {
            if (slot != null)
            {
                slot.Clear();
            }
        }
    }

    public void ResetSlotsVisual()
    {
        if (combinationSlots != null)
        {
            foreach (CombinationSlot slot in combinationSlots)
            {
                if (slot != null)
                {
                    slot.ResetVisual();
                }
            }
        }

        if (lockImage != null)
        {
            lockImage.color = Color.white;
            lockImage.transform.localScale = Vector3.one;
        }
    }

    public void SetIncorrectFeedback()
    {
        if (combinationSlots != null)
        {
            foreach (CombinationSlot slot in combinationSlots)
            {
                if (slot != null)
                {
                    slot.SetIncorrectFeedback();
                }
            }
        }

        if (lockImage != null)
        {
            lockImage.color = new Color(1f, 0.38f, 0.32f, 1f);
            lockImage.transform.localScale = Vector3.one * 1.08f;
        }
    }

    public void SetCorrectFeedback()
    {
        if (combinationSlots == null)
        {
            return;
        }

        foreach (CombinationSlot slot in combinationSlots)
        {
            if (slot != null)
            {
                slot.SetCorrectFeedback();
            }
        }
    }

    public void SetChestOpen(bool open)
    {
        if (chestImage != null)
        {
            chestImage.sprite = open ? openChestSprite : closedChestSprite;
            chestImage.preserveAspect = true;
        }

        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!open);
        }
    }

    public void SetRewardsVisible(bool visible)
    {
        if (keyImage != null)
        {
            keyImage.gameObject.SetActive(visible);
        }

        if (coinImage != null)
        {
            coinImage.gameObject.SetActive(visible);
        }
    }

    public void SetNumberButtonsInteractable(bool interactable)
    {
        if (numberButtons == null)
        {
            return;
        }

        foreach (ChestNumberButton numberButton in numberButtons)
        {
            if (numberButton != null)
            {
                numberButton.SetInteractable(interactable);
            }
        }
    }

    public void SetConfirmClearInteractable(bool interactable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = interactable;
        }

        if (clearButton != null)
        {
            clearButton.interactable = interactable;
        }
    }

    public void SetRetryVisible(bool visible)
    {
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(visible);
        }
    }

    public void SetExitInteractable(bool interactable)
    {
        if (exitButton != null)
        {
            exitButton.interactable = interactable;
        }
    }

    private void EnsureBuilt()
    {
        if (root == null)
        {
            root = gameObject;
        }

        RectTransform rootRect = root.GetComponent<RectTransform>();
        if (rootRect == null)
        {
            rootRect = root.AddComponent<RectTransform>();
        }

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        if (canvasGroup == null)
        {
            canvasGroup = root.GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = root.AddComponent<CanvasGroup>();
        }

        ResolveMedievalFont();
        FindOrCreateAudioSources();
        ConfigureAudioSource(sfxSource, false);
        ConfigureAudioSource(ambienceSource, true);

        bool layoutAlreadyBuilt =
            titleText != null &&
            clueText != null &&
            statusText != null &&
            combinationSlots != null &&
            combinationSlots.Length == 3 &&
            numberButtons != null &&
            numberButtons.Length == 10;

        if (layoutAlreadyBuilt)
        {
            return;
        }

        BuildDefaultLayout();
        RegisterButtonListeners();
        SetCallbacks(numberHandler, confirmHandler, clearHandler, retryHandler, exitHandler);
    }

    private void ApplyVisibleState()
    {
        if (root != null && !root.activeSelf)
        {
            root.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void BuildDefaultLayout()
    {
        Transform rootTransform = root.transform;
        ClearRuntimeChildren(rootTransform);

        GameObject overlay = CreateUIObject("BackgroundOverlay", rootTransform);
        Stretch(overlay);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = OverlayColor;
        overlayImage.raycastTarget = true;

        GameObject frame = CreateUIObject("MainFrame", rootTransform);
        mainFrameRect = frame.GetComponent<RectTransform>();
        SetCenteredRect(mainFrameRect, Vector2.zero, new Vector2(FrameWidth, FrameHeight));

        Image frameImage = frame.AddComponent<Image>();
        frameImage.sprite = mainFrameSprite;
        frameImage.color = mainFrameSprite != null
            ? Color.white
            : new Color(0.12f, 0.085f, 0.045f, 0.98f);
        frameImage.preserveAspect = true;
        frameImage.raycastTarget = true;

        titleText = CreateText(
            frame.transform,
            "TitleText",
            "COFRE CON COMBINACIÓN",
            new Vector2(0f, 314f),
            new Vector2(820f, 62f),
            46f,
            30f,
            TitleColor,
            FontStyles.Bold | FontStyles.SmallCaps,
            true,
            new Color32(45, 20, 5, 255),
            0.22f);
        titleText.characterSpacing = 2.5f;

        GameObject clueScroll = CreateUIObject("ClueScroll", frame.transform);
        RectTransform clueRect = clueScroll.GetComponent<RectTransform>();
        SetCenteredRect(clueRect, new Vector2(-270f, 142f), new Vector2(318f, 318f));

        Image clueImage = clueScroll.AddComponent<Image>();
        clueImage.sprite = clueScrollSprite;
        clueImage.color = clueScrollSprite != null
            ? Color.white
            : new Color(0.78f, 0.63f, 0.39f, 1f);
        clueImage.preserveAspect = true;
        clueImage.raycastTarget = false;

        clueText = CreateText(
            clueScroll.transform,
            "ClueText",
            string.Empty,
            new Vector2(0f, -2f),
            new Vector2(220f, 190f),
            27f,
            18f,
            ParchmentTextColor,
            FontStyles.Bold,
            true,
            new Color32(255, 240, 190, 80),
            0.08f);
        clueText.margin = new Vector4(8f, 8f, 8f, 8f);
        clueText.lineSpacing = 5f;

        GameObject chest = CreateUIObject("ChestImage", frame.transform);
        RectTransform chestRect = chest.GetComponent<RectTransform>();
        SetCenteredRect(chestRect, new Vector2(250f, 155f), new Vector2(292f, 292f));

        chestImage = chest.AddComponent<Image>();
        chestImage.sprite = closedChestSprite;
        chestImage.preserveAspect = true;
        chestImage.raycastTarget = false;

        GameObject lockObject = CreateUIObject("LockImage", chest.transform);
        RectTransform lockRect = lockObject.GetComponent<RectTransform>();
        SetCenteredRect(lockRect, new Vector2(0f, -17f), new Vector2(88f, 88f));

        lockImage = lockObject.AddComponent<Image>();
        lockImage.sprite = lockSprite;
        lockImage.preserveAspect = true;
        lockImage.raycastTarget = false;

        GameObject display = CreateUIObject("CombinationDisplay", frame.transform);
        RectTransform displayRect = display.GetComponent<RectTransform>();
        SetCenteredRect(displayRect, new Vector2(250f, -28f), new Vector2(310f, 92f));

        combinationSlots = new CombinationSlot[3];
        for (int i = 0; i < combinationSlots.Length; i++)
        {
            combinationSlots[i] = CreateSlot(
                display.transform,
                "Slot" + (i + 1),
                new Vector2(-102f + i * 102f, 0f));
        }

        statusText = CreateText(
            frame.transform,
            "StatusText",
            "Introduce tres números.",
            new Vector2(250f, -91f),
            new Vector2(460f, 42f),
            25f,
            17f,
            StatusColor,
            FontStyles.Bold,
            true,
            new Color32(35, 15, 3, 255),
            0.18f);

        GameObject numberPad = CreateUIObject("NumberPad", frame.transform);
        RectTransform padRect = numberPad.GetComponent<RectTransform>();
        SetCenteredRect(padRect, new Vector2(-270f, -205f), new Vector2(240f, 300f));

        GridLayoutGroup grid = numberPad.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(64f, 64f);
        grid.spacing = new Vector2(12f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        numberButtons = new ChestNumberButton[10];
        for (int number = 1; number <= 9; number++)
        {
            numberButtons[number] = CreateNumberButton(numberPad.transform, number);
        }

        CreateGridSpacer(numberPad.transform, "SpacerLeft");
        numberButtons[0] = CreateNumberButton(numberPad.transform, 0);
        CreateGridSpacer(numberPad.transform, "SpacerRight");

        confirmButton = CreateCommandButton(
            frame.transform,
            "ConfirmButton",
            "CONFIRMAR",
            confirmButtonSprite,
            new Vector2(152f, -176f));

        clearButton = CreateCommandButton(
            frame.transform,
            "ClearButton",
            "BORRAR",
            clearButtonSprite,
            new Vector2(378f, -176f));

        retryButton = CreateCommandButton(
            frame.transform,
            "RetryButton",
            "REINTENTAR",
            retryButtonSprite,
            new Vector2(152f, -258f));

        exitButton = CreateCommandButton(
            frame.transform,
            "ExitButton",
            "SALIR",
            exitButtonSprite,
            new Vector2(378f, -258f));

        keyImage = CreateReward(frame.transform, "RewardKey", keySprite, new Vector2(390f, 86f), new Vector2(92f, 128f));
        coinImage = CreateReward(frame.transform, "RewardCoin", coinSprite, new Vector2(420f, 178f), new Vector2(92f, 92f));

        SetRewardsVisible(false);
        SetRetryVisible(false);
        UpdateResponsiveScale();
    }

    private CombinationSlot CreateSlot(Transform parent, string objectName, Vector2 anchoredPosition)
    {
        GameObject slotObject = CreateUIObject(objectName, parent);
        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        SetCenteredRect(slotRect, anchoredPosition, new Vector2(82f, 82f));

        Image image = slotObject.AddComponent<Image>();
        image.sprite = numberSlotSprite;
        image.color = Color.white;
        image.preserveAspect = true;

        TMP_Text text = CreateText(
            slotObject.transform,
            "ValueText",
            string.Empty,
            Vector2.zero,
            new Vector2(56f, 56f),
            43f,
            28f,
            ParchmentTextColor,
            FontStyles.Bold,
            false,
            new Color32(255, 235, 175, 70),
            0.08f);

        CombinationSlot slot = slotObject.AddComponent<CombinationSlot>();
        slot.Configure(image, text);
        return slot;
    }

    private ChestNumberButton CreateNumberButton(Transform parent, int value)
    {
        GameObject buttonObject = CreateUIObject("Button" + value, parent);

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = numberSlotSprite;
        image.color = Color.white;
        image.preserveAspect = true;

        Button button = buttonObject.AddComponent<Button>();
        ConfigureButtonColors(button);

        TMP_Text text = CreateText(
            buttonObject.transform,
            "Text",
            value.ToString(),
            Vector2.zero,
            new Vector2(46f, 46f),
            35f,
            24f,
            ParchmentTextColor,
            FontStyles.Bold,
            false,
            new Color32(255, 235, 175, 60),
            0.07f);

        ChestNumberButton numberButton = buttonObject.AddComponent<ChestNumberButton>();
        numberButton.Configure(value, button, text, numberHandler);
        return numberButton;
    }

    private Button CreateCommandButton(
        Transform parent,
        string objectName,
        string fallbackLabel,
        Sprite sprite,
        Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        SetCenteredRect(rectTransform, anchoredPosition, new Vector2(210f, 70f));

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = sprite != null
            ? Color.white
            : new Color(0.56f, 0.39f, 0.18f, 1f);
        image.preserveAspect = true;

        Button button = buttonObject.AddComponent<Button>();
        ConfigureButtonColors(button);

        // Los PNG de CONFIRMAR, BORRAR, REINTENTAR y SALIR ya incluyen sus letras.
        // Solo se crea texto cuando el sprite no existe, evitando letras duplicadas.
        if (sprite == null)
        {
            TMP_Text fallbackText = CreateText(
                buttonObject.transform,
                "FallbackText",
                fallbackLabel,
                Vector2.zero,
                new Vector2(174f, 42f),
                24f,
                16f,
                ParchmentTextColor,
                FontStyles.Bold | FontStyles.SmallCaps,
                false,
                new Color32(255, 235, 175, 70),
                0.08f);
            fallbackText.characterSpacing = 1.5f;
        }

        return button;
    }

    private Image CreateReward(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject rewardObject = CreateUIObject(objectName, parent);
        RectTransform rewardRect = rewardObject.GetComponent<RectTransform>();
        SetCenteredRect(rewardRect, anchoredPosition, size);

        Image image = rewardObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(
        Transform parent,
        string objectName,
        string text,
        Vector2 anchoredPosition,
        Vector2 size,
        float maximumFontSize,
        float minimumFontSize,
        Color color,
        FontStyles style,
        bool wordWrap,
        Color32 outlineColor,
        float outlineWidth)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        SetCenteredRect(rectTransform, anchoredPosition, size);

        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.font = ResolveMedievalFont();
        tmpText.fontSize = maximumFontSize;
        tmpText.enableAutoSizing = true;
        tmpText.fontSizeMax = maximumFontSize;
        tmpText.fontSizeMin = minimumFontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = color;
        tmpText.fontStyle = style;
        tmpText.enableWordWrapping = wordWrap;
        tmpText.overflowMode = TextOverflowModes.Ellipsis;
        tmpText.raycastTarget = false;
        tmpText.outlineColor = outlineColor;
        tmpText.outlineWidth = outlineWidth;
        return tmpText;
    }

    private TMP_FontAsset ResolveMedievalFont()
    {
        if (medievalFont != null)
        {
            return medievalFont;
        }

        if (!string.IsNullOrWhiteSpace(medievalFontResourcesPath))
        {
            medievalFont = Resources.Load<TMP_FontAsset>(medievalFontResourcesPath);
        }

        if (medievalFont == null)
        {
            medievalFont = Resources.Load<TMP_FontAsset>("Fonts/MedievalFont");
        }

        if (medievalFont == null)
        {
            medievalFont = TMP_Settings.defaultFontAsset;
        }

        return medievalFont;
    }

    private void FindOrCreateAudioSources()
    {
        if (root == null)
        {
            return;
        }

        AudioSource[] sources = root.GetComponents<AudioSource>();

        if (sfxSource == null)
        {
            sfxSource = sources.Length > 0 ? sources[0] : root.AddComponent<AudioSource>();
        }

        if (ambienceSource == null || ambienceSource == sfxSource)
        {
            if (sources.Length > 1 && sources[1] != sfxSource)
            {
                ambienceSource = sources[1];
            }
            else
            {
                ambienceSource = root.AddComponent<AudioSource>();
            }
        }
    }

    private void UpdateResponsiveScale()
    {
        if (mainFrameRect == null || root == null)
        {
            return;
        }

        RectTransform rootRect = root.GetComponent<RectTransform>();
        if (rootRect == null)
        {
            return;
        }

        float availableWidth = Mathf.Max(1f, rootRect.rect.width - 32f);
        float availableHeight = Mathf.Max(1f, rootRect.rect.height - 32f);
        float scale = Mathf.Min(availableWidth / FrameWidth, availableHeight / FrameHeight);
        scale = Mathf.Clamp(scale, 0.55f, 1f);
        mainFrameRect.localScale = Vector3.one * scale;
    }

    private void RegisterButtonListeners()
    {
        RegisterButton(confirmButton, HandleConfirmClicked);
        RegisterButton(clearButton, HandleClearClicked);
        RegisterButton(retryButton, HandleRetryClicked);
        RegisterButton(exitButton, HandleExitClicked);
    }

    private void UnregisterButtonListeners()
    {
        UnregisterButton(confirmButton, HandleConfirmClicked);
        UnregisterButton(clearButton, HandleClearClicked);
        UnregisterButton(retryButton, HandleRetryClicked);
        UnregisterButton(exitButton, HandleExitClicked);
    }

    private static void RegisterButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(callback);
        button.onClick.AddListener(callback);
    }

    private static void UnregisterButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(callback);
        }
    }

    private void ClearNumberHandlers()
    {
        if (numberButtons == null)
        {
            return;
        }

        foreach (ChestNumberButton numberButton in numberButtons)
        {
            if (numberButton != null)
            {
                numberButton.SetClickHandler(null);
            }
        }
    }

    private void HandleConfirmClicked()
    {
        confirmHandler?.Invoke();
    }

    private void HandleClearClicked()
    {
        clearHandler?.Invoke();
    }

    private void HandleRetryClicked()
    {
        retryHandler?.Invoke();
    }

    private void HandleExitClicked()
    {
        exitHandler?.Invoke();
    }

    private static void ActivateSelfAndParents(Transform target)
    {
        if (target == null)
        {
            return;
        }

        if (target.parent != null)
        {
            ActivateSelfAndParents(target.parent);
        }

        if (!target.gameObject.activeSelf)
        {
            target.gameObject.SetActive(true);
        }
    }

    private static void ClearRuntimeChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        uiObject.layer = parent.gameObject.layer;
        return uiObject;
    }

    private static void CreateGridSpacer(Transform parent, string objectName)
    {
        GameObject spacer = CreateUIObject(objectName, parent);
        Image image = spacer.AddComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = false;
    }

    private static RectTransform Stretch(GameObject gameObject)
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        return rectTransform;
    }

    private static void SetCenteredRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }

    private static void ConfigureButtonColors(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.92f, 0.70f, 1f);
        colors.pressedColor = new Color(0.78f, 0.63f, 0.38f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.72f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;
    }

    private static void ConfigureAudioSource(AudioSource audioSource, bool loop)
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.spatialBlend = 0f;
    }
}
