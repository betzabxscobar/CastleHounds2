using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RuneMemoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private RuneMemoryButton[] runeButtons;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip highlightClip;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip victoryClip;

    private Action startHandler;
    private Action retryHandler;
    private Action exitHandler;

    public RuneMemoryButton[] RuneButtons => runeButtons;
    public AudioSource SfxSource => sfxSource;
    public AudioSource AmbientSource => ambientSource;
    public AudioClip AmbientClip => ambientClip;
    public AudioClip HighlightClip => highlightClip;
    public AudioClip CorrectClip => correctClip;
    public AudioClip ErrorClip => errorClip;
    public AudioClip VictoryClip => victoryClip;

    public static RuneMemoryPanel CreateDefault(
        Transform parent,
        Sprite swordSprite,
        Sprite shieldSprite,
        Sprite wolfSprite,
        Sprite crownSprite,
        Sprite frameSprite,
        Sprite startSprite,
        Sprite retrySprite,
        Sprite exitSprite)
    {
        GameObject panelObject = CreateUIObject("RuneMemoryPanel", parent);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CanvasGroup canvasGroup = panelObject.AddComponent<CanvasGroup>();
        AudioSource sfxSource = panelObject.AddComponent<AudioSource>();
        AudioSource ambientSource = panelObject.AddComponent<AudioSource>();
        ConfigureAudioSource(sfxSource, false);
        ConfigureAudioSource(ambientSource, true);

        GameObject overlayObject = CreateUIObject("BackgroundOverlay", panelObject.transform);
        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image overlayImage = overlayObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject frameObject = CreateUIObject("Frame", panelObject.transform);
        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.pivot = new Vector2(0.5f, 0.5f);
        frameRect.sizeDelta = new Vector2(980f, 720f);
        frameRect.anchoredPosition = Vector2.zero;
        Image frameImage = frameObject.AddComponent<Image>();
        frameImage.sprite = frameSprite;
        frameImage.type = Image.Type.Simple;
        frameImage.color = frameSprite != null ? Color.white : new Color(0.08f, 0.07f, 0.06f, 0.96f);

        TMP_Text title = CreateText(frameObject.transform, "TitleText", "MEMORIA DE RUNAS", new Vector2(0f, 260f), new Vector2(820f, 70f), 48f);
        TMP_Text instruction = CreateText(frameObject.transform, "InstructionText", "Observa la secuencia y repetela.", new Vector2(0f, 205f), new Vector2(820f, 42f), 26f);
        TMP_Text round = CreateText(frameObject.transform, "RoundText", "Ronda 1 de 3", new Vector2(0f, 155f), new Vector2(420f, 42f), 28f);
        TMP_Text status = CreateText(frameObject.transform, "StatusText", "Pulsa COMENZAR", new Vector2(0f, 105f), new Vector2(620f, 46f), 30f);

        GameObject runesContainer = CreateUIObject("RunesContainer", frameObject.transform);
        RectTransform runesRect = runesContainer.GetComponent<RectTransform>();
        runesRect.anchorMin = new Vector2(0.5f, 0.5f);
        runesRect.anchorMax = new Vector2(0.5f, 0.5f);
        runesRect.pivot = new Vector2(0.5f, 0.5f);
        runesRect.sizeDelta = new Vector2(760f, 170f);
        runesRect.anchoredPosition = new Vector2(0f, -35f);

        RuneMemoryButton[] createdRuneButtons =
        {
            CreateRuneButton(runesContainer.transform, "RuneButtonSword", "RuneImageSword", 0, swordSprite, new Vector2(-285f, 0f)),
            CreateRuneButton(runesContainer.transform, "RuneButtonShield", "RuneImageShield", 1, shieldSprite, new Vector2(-95f, 0f)),
            CreateRuneButton(runesContainer.transform, "RuneButtonWolf", "RuneImageWolf", 2, wolfSprite, new Vector2(95f, 0f)),
            CreateRuneButton(runesContainer.transform, "RuneButtonCrown", "RuneImageCrown", 3, crownSprite, new Vector2(285f, 0f))
        };

        Button start = CreateCommandButton(frameObject.transform, "StartButton", "COMENZAR", startSprite, new Vector2(-250f, -260f));
        Button retry = CreateCommandButton(frameObject.transform, "RetryButton", "REINTENTAR", retrySprite, new Vector2(0f, -260f));
        Button exit = CreateCommandButton(frameObject.transform, "ExitButton", "SALIR", exitSprite, new Vector2(250f, -260f));

        RuneMemoryPanel panel = panelObject.AddComponent<RuneMemoryPanel>();
        panel.Configure(panelObject, canvasGroup, title, instruction, round, status, start, retry, exit, createdRuneButtons);
        panel.ConfigureAudioReferences(sfxSource, ambientSource, null, null, null, null, null);
        panel.Hide();
        return panel;
    }

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        RegisterButtonListeners();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
    }

    public void Configure(
        GameObject configuredRoot,
        CanvasGroup configuredCanvasGroup,
        TMP_Text configuredTitleText,
        TMP_Text configuredInstructionText,
        TMP_Text configuredRoundText,
        TMP_Text configuredStatusText,
        Button configuredStartButton,
        Button configuredRetryButton,
        Button configuredExitButton,
        RuneMemoryButton[] configuredRuneButtons)
    {
        UnregisterButtonListeners();

        root = configuredRoot != null ? configuredRoot : gameObject;
        canvasGroup = configuredCanvasGroup;
        titleText = configuredTitleText;
        instructionText = configuredInstructionText;
        roundText = configuredRoundText;
        statusText = configuredStatusText;
        startButton = configuredStartButton;
        retryButton = configuredRetryButton;
        exitButton = configuredExitButton;
        runeButtons = configuredRuneButtons;

        RegisterButtonListeners();
        SetTitle("MEMORIA DE RUNAS");
        SetInstruction("Observa la secuencia y repetela.");
    }

    public void ConfigureAudioReferences(
        AudioSource configuredSfxSource,
        AudioSource configuredAmbientSource,
        AudioClip configuredAmbientClip,
        AudioClip configuredHighlightClip,
        AudioClip configuredCorrectClip,
        AudioClip configuredErrorClip,
        AudioClip configuredVictoryClip)
    {
        sfxSource = configuredSfxSource;
        ambientSource = configuredAmbientSource;
        ambientClip = configuredAmbientClip;
        highlightClip = configuredHighlightClip;
        correctClip = configuredCorrectClip;
        errorClip = configuredErrorClip;
        victoryClip = configuredVictoryClip;

        ConfigureAudioSource(sfxSource, false);
        ConfigureAudioSource(ambientSource, true);
    }

    public void SetCallbacks(Action onStart, Action onRetry, Action onExit)
    {
        startHandler = onStart;
        retryHandler = onRetry;
        exitHandler = onExit;
    }

    public void Show()
    {
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

    public void SetTitle(string text)
    {
        if (titleText != null)
        {
            titleText.text = text;
        }
    }

    public void SetInstruction(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }

    public void SetRound(int current, int total)
    {
        if (roundText != null)
        {
            roundText.text = $"Ronda {current} de {total}";
        }
    }

    public void SetStatus(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }

    public void SetStartVisible(bool visible)
    {
        SetButtonVisible(startButton, visible);
    }

    public void SetRetryVisible(bool visible)
    {
        SetButtonVisible(retryButton, visible);
    }

    public void SetExitVisible(bool visible)
    {
        SetButtonVisible(exitButton, visible);
    }

    public void SetRunesInteractable(bool interactable)
    {
        if (runeButtons == null)
        {
            return;
        }

        foreach (RuneMemoryButton runeButton in runeButtons)
        {
            if (runeButton != null)
            {
                runeButton.SetInteractable(interactable);
            }
        }
    }

    public void ResetRunesVisual()
    {
        if (runeButtons == null)
        {
            return;
        }

        foreach (RuneMemoryButton runeButton in runeButtons)
        {
            if (runeButton != null)
            {
                runeButton.ResetVisual();
            }
        }
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
        }
    }

    private void RegisterButtonListeners()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartClicked);
            startButton.onClick.AddListener(HandleStartClicked);
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
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartClicked);
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

    private void HandleStartClicked()
    {
        startHandler?.Invoke();
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

    private static TMP_Text CreateText(Transform parent, string objectName, string text, Vector2 anchoredPosition, Vector2 size, float fontSize)
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
        tmpText.color = Color.white;
        tmpText.raycastTarget = false;
        return tmpText;
    }

    private static RuneMemoryButton CreateRuneButton(Transform parent, string buttonName, string imageName, int runeIndex, Sprite sprite, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateUIObject(buttonName, parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(140f, 140f);

        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.08f, 0.86f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.78f, 1f);
        colors.pressedColor = new Color(0.85f, 0.72f, 0.35f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.65f);
        button.colors = colors;

        GameObject imageObject = CreateUIObject(imageName, buttonObject.transform);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = new Vector2(16f, 16f);
        imageRect.offsetMax = new Vector2(-16f, -16f);

        Image runeImage = imageObject.AddComponent<Image>();
        runeImage.sprite = sprite;
        runeImage.preserveAspect = true;
        runeImage.color = Color.white;
        runeImage.raycastTarget = false;

        RuneMemoryButton runeButton = buttonObject.AddComponent<RuneMemoryButton>();
        runeButton.Configure(runeIndex, button, runeImage, null);
        return runeButton;
    }

    private static Button CreateCommandButton(Transform parent, string objectName, string label, Sprite sprite, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(210f, 76f);

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = sprite != null ? Color.white : new Color(0.18f, 0.16f, 0.12f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.95f, 0.78f, 1f);
        colors.pressedColor = new Color(0.85f, 0.72f, 0.35f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.65f);
        button.colors = colors;

        TMP_Text text = CreateText(buttonObject.transform, "Text", label, Vector2.zero, new Vector2(190f, 46f), 22f);
        text.color = new Color(0.12f, 0.08f, 0.03f, 1f);
        return button;
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
