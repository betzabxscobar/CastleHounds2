using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ChestCombinationPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;

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

    private Action<int> numberHandler;
    private Action confirmHandler;
    private Action clearHandler;
    private Action retryHandler;
    private Action exitHandler;

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
        Hide();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
        ClearNumberHandlers();
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
        EnsureBuilt();

        if (root != null)
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

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void ResetForNewAttempt(string configuredClueText)
    {
        SetTitle("COFRE CON COMBINACION");
        SetClue(configuredClueText);
        SetStatus("Introduce tres numeros.");
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
            if (combinationSlots[i] == null)
            {
                continue;
            }

            if (digits != null && i < count)
            {
                combinationSlots[i].SetValue(digits[i]);
            }
            else
            {
                combinationSlots[i].Clear();
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
        if (combinationSlots == null)
        {
            return;
        }

        foreach (CombinationSlot slot in combinationSlots)
        {
            if (slot != null)
            {
                slot.ResetVisual();
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
            lockImage.color = new Color(1f, 0.32f, 0.32f, 1f);
            lockImage.transform.localScale = Vector3.one * 1.08f;
        }
    }

    public void SetCorrectFeedback()
    {
        if (combinationSlots != null)
        {
            foreach (CombinationSlot slot in combinationSlots)
            {
                if (slot != null)
                {
                    slot.SetCorrectFeedback();
                }
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

        RectTransform rectTransform = root.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = root.AddComponent<RectTransform>();
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        if (canvasGroup == null)
        {
            canvasGroup = root.GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = root.AddComponent<CanvasGroup>();
        }

        if (sfxSource == null)
        {
            sfxSource = root.GetComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = root.AddComponent<AudioSource>();
        }

        if (ambienceSource == null)
        {
            ambienceSource = root.AddComponent<AudioSource>();
        }

        ConfigureAudioSource(sfxSource, false);
        ConfigureAudioSource(ambienceSource, true);

        if (titleText != null && combinationSlots != null && combinationSlots.Length == 3 && numberButtons != null && numberButtons.Length == 10)
        {
            return;
        }

        BuildDefaultLayout();
        RegisterButtonListeners();
        SetCallbacks(numberHandler, confirmHandler, clearHandler, retryHandler, exitHandler);
    }

    private void BuildDefaultLayout()
    {
        Transform rootTransform = root.transform;
        ClearRuntimeChildren(rootTransform);

        GameObject overlay = CreateUIObject("BackgroundOverlay", rootTransform);
        RectTransform overlayRect = Stretch(overlay);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject frame = CreateUIObject("MainFrame", rootTransform);
        RectTransform frameRect = frame.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.pivot = new Vector2(0.5f, 0.5f);
        frameRect.sizeDelta = new Vector2(1040f, 760f);
        frameRect.anchoredPosition = Vector2.zero;
        Image frameImage = frame.AddComponent<Image>();
        frameImage.sprite = mainFrameSprite;
        frameImage.color = mainFrameSprite != null ? Color.white : new Color(0.08f, 0.07f, 0.05f, 0.96f);

        titleText = CreateText(frame.transform, "TitleText", "COFRE CON COMBINACION", new Vector2(0f, 310f), new Vector2(860f, 58f), 42f, Color.white);

        GameObject clueScroll = CreateUIObject("ClueScroll", frame.transform);
        RectTransform clueRect = clueScroll.GetComponent<RectTransform>();
        clueRect.anchorMin = new Vector2(0.5f, 0.5f);
        clueRect.anchorMax = new Vector2(0.5f, 0.5f);
        clueRect.pivot = new Vector2(0.5f, 0.5f);
        clueRect.sizeDelta = new Vector2(420f, 190f);
        clueRect.anchoredPosition = new Vector2(-260f, 165f);
        Image clueImage = clueScroll.AddComponent<Image>();
        clueImage.sprite = clueScrollSprite;
        clueImage.color = clueScrollSprite != null ? Color.white : new Color(0.72f, 0.58f, 0.36f, 1f);

        clueText = CreateText(clueScroll.transform, "ClueText", string.Empty, Vector2.zero, new Vector2(330f, 126f), 24f, new Color(0.18f, 0.11f, 0.04f, 1f));

        GameObject chest = CreateUIObject("ChestImage", frame.transform);
        RectTransform chestRect = chest.GetComponent<RectTransform>();
        chestRect.anchorMin = new Vector2(0.5f, 0.5f);
        chestRect.anchorMax = new Vector2(0.5f, 0.5f);
        chestRect.pivot = new Vector2(0.5f, 0.5f);
        chestRect.sizeDelta = new Vector2(280f, 280f);
        chestRect.anchoredPosition = new Vector2(255f, 120f);
        chestImage = chest.AddComponent<Image>();
        chestImage.sprite = closedChestSprite;
        chestImage.preserveAspect = true;

        GameObject lockObject = CreateUIObject("LockImage", chest.transform);
        RectTransform lockRect = lockObject.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.pivot = new Vector2(0.5f, 0.5f);
        lockRect.sizeDelta = new Vector2(92f, 92f);
        lockRect.anchoredPosition = new Vector2(0f, -18f);
        lockImage = lockObject.AddComponent<Image>();
        lockImage.sprite = lockSprite;
        lockImage.preserveAspect = true;

        GameObject display = CreateUIObject("CombinationDisplay", frame.transform);
        RectTransform displayRect = display.GetComponent<RectTransform>();
        displayRect.anchorMin = new Vector2(0.5f, 0.5f);
        displayRect.anchorMax = new Vector2(0.5f, 0.5f);
        displayRect.pivot = new Vector2(0.5f, 0.5f);
        displayRect.sizeDelta = new Vector2(360f, 110f);
        displayRect.anchoredPosition = new Vector2(0f, -70f);

        combinationSlots = new CombinationSlot[3];
        for (int i = 0; i < combinationSlots.Length; i++)
        {
            combinationSlots[i] = CreateSlot(display.transform, "Slot" + (i + 1), new Vector2(-120f + i * 120f, 0f));
        }

        GameObject numberPad = CreateUIObject("NumberPad", frame.transform);
        RectTransform padRect = numberPad.GetComponent<RectTransform>();
        padRect.anchorMin = new Vector2(0.5f, 0.5f);
        padRect.anchorMax = new Vector2(0.5f, 0.5f);
        padRect.pivot = new Vector2(0.5f, 0.5f);
        padRect.sizeDelta = new Vector2(360f, 290f);
        padRect.anchoredPosition = new Vector2(-250f, -200f);
        GridLayoutGroup grid = numberPad.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(78f, 60f);
        grid.spacing = new Vector2(14f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.MiddleCenter;

        int[] orderedNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        numberButtons = new ChestNumberButton[10];
        foreach (int number in orderedNumbers)
        {
            numberButtons[number] = CreateNumberButton(numberPad.transform, number);
        }

        confirmButton = CreateCommandButton(frame.transform, "ConfirmButton", "CONFIRMAR", confirmButtonSprite, new Vector2(250f, -150f));
        clearButton = CreateCommandButton(frame.transform, "ClearButton", "BORRAR", clearButtonSprite, new Vector2(250f, -220f));
        retryButton = CreateCommandButton(frame.transform, "RetryButton", "REINTENTAR", retryButtonSprite, new Vector2(250f, -290f));
        exitButton = CreateCommandButton(frame.transform, "ExitButton", "SALIR", exitButtonSprite, new Vector2(430f, -290f));

        statusText = CreateText(frame.transform, "StatusText", "Introduce tres numeros.", new Vector2(0f, -315f), new Vector2(560f, 44f), 26f, Color.white);

        keyImage = CreateReward(frame.transform, "RewardKey", keySprite, new Vector2(350f, 20f));
        coinImage = CreateReward(frame.transform, "RewardCoin", coinSprite, new Vector2(430f, 40f));

        _ = overlayRect;
        SetRewardsVisible(false);
        SetRetryVisible(false);
    }

    private static void ClearRuntimeChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private CombinationSlot CreateSlot(Transform parent, string objectName, Vector2 anchoredPosition)
    {
        GameObject slotObject = CreateUIObject(objectName, parent);
        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.sizeDelta = new Vector2(92f, 92f);
        slotRect.anchoredPosition = anchoredPosition;

        Image image = slotObject.AddComponent<Image>();
        image.sprite = numberSlotSprite;
        image.color = Color.white;

        TMP_Text text = CreateText(slotObject.transform, "ValueText", string.Empty, Vector2.zero, new Vector2(78f, 60f), 42f, new Color(0.1f, 0.06f, 0.02f, 1f));
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

        Button button = buttonObject.AddComponent<Button>();
        TMP_Text text = CreateText(buttonObject.transform, "Text", value.ToString(), Vector2.zero, new Vector2(64f, 46f), 30f, new Color(0.1f, 0.06f, 0.02f, 1f));
        ChestNumberButton numberButton = buttonObject.AddComponent<ChestNumberButton>();
        numberButton.Configure(value, button, text, numberHandler);
        return numberButton;
    }

    private Button CreateCommandButton(Transform parent, string objectName, string label, Sprite sprite, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(168f, 56f);
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = sprite != null ? Color.white : new Color(0.22f, 0.16f, 0.1f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        TMP_Text text = CreateText(buttonObject.transform, "Text", label, Vector2.zero, new Vector2(142f, 36f), 18f, new Color(0.12f, 0.08f, 0.03f, 1f));
        text.fontStyle = FontStyles.Bold;
        return button;
    }

    private Image CreateReward(Transform parent, string objectName, Sprite sprite, Vector2 anchoredPosition)
    {
        GameObject rewardObject = CreateUIObject(objectName, parent);
        RectTransform rewardRect = rewardObject.GetComponent<RectTransform>();
        rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
        rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
        rewardRect.pivot = new Vector2(0.5f, 0.5f);
        rewardRect.sizeDelta = new Vector2(96f, 96f);
        rewardRect.anchoredPosition = anchoredPosition;
        Image image = rewardObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        return image;
    }

    private void RegisterButtonListeners()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            confirmButton.onClick.AddListener(HandleConfirmClicked);
        }

        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(HandleClearClicked);
            clearButton.onClick.AddListener(HandleClearClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            retryButton.onClick.AddListener(HandleRetryClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(HandleExitClicked);
            exitButton.onClick.AddListener(HandleExitClicked);
        }
    }

    private void UnregisterButtonListeners()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(HandleConfirmClicked);
        }

        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(HandleClearClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(HandleExitClicked);
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

    private static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
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

    private static TMP_Text CreateText(Transform parent, string objectName, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, Color color)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        TMP_Text tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = color;
        tmpText.raycastTarget = false;
        return tmpText;
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
