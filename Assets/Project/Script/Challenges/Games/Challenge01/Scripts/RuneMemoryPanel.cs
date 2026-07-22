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

    private Action startHandler;
    private Action retryHandler;
    private Action exitHandler;

    public RuneMemoryButton[] RuneButtons => runeButtons;

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
}
