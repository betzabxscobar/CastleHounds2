using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class Challenge07PuzzleController : MonoBehaviour
{
    public enum PuzzleMode
    {
        SixPieces,
        NinePieces
    }

    [Header("Modo")]
    [SerializeField] private PuzzleMode puzzleMode = PuzzleMode.SixPieces;
    [SerializeField] private GameObject pieces6Root;
    [SerializeField] private GameObject pieces9Root;
    [SerializeField] private GameObject targets6Root;
    [SerializeField] private GameObject targets9Root;
    [SerializeField] private MapPuzzlePiece[] sixPieces;
    [SerializeField] private MapPuzzlePiece[] ninePieces;
    [SerializeField] private Button sixPiecesButton;
    [SerializeField] private Button ninePiecesButton;
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject remainingPiecesPanel;
    [SerializeField] private Button startButton;

    [Header("Piezas del mapa")]
    [SerializeField] private MapPuzzlePiece[] puzzlePieces;

    [Header("Interfaz")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text hintCounterText;
    [SerializeField] private Image completedMapImage;
    [SerializeField] private GameObject castleReveal;
    [SerializeField] private CanvasGroup backgroundDarkener;
    [SerializeField] private TMP_Text piecesCountText;

    [Header("Presentación")]
    [SerializeField] private Animator puzzleAnimator;
    [SerializeField] private ParticleSystem victoryParticles;
    [SerializeField] private Challenge07UIAnimator uiAnimator;
    [SerializeField, Min(0.05f)] private float victoryAnimationDuration = 0.65f;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip hintClip;
    [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.3f;

    [Header("Integración")]
    [SerializeField] private Challenge07GameBridge gameBridge;
    [SerializeField] private UnityEvent onPuzzleCompleted;
    [SerializeField] private UnityEvent onContinuePressed;
    [SerializeField] private bool standaloneDemo;

    private int placedPieces;
    private bool puzzleCompleted;
    private bool victoryReported;
    private bool initialized;
    private Coroutine victoryRoutine;
    private int remainingHints = 3;
    private bool sessionStarted;

    public PuzzleMode Mode => puzzleMode;
    public Challenge07GameBridge PuzzleBridge => gameBridge;

    public void SetGameBridge(Challenge07GameBridge bridge)
    {
        gameBridge = bridge;
        if (bridge != null) bridge.ConfigureRuntime(this);
    }

    public void ConfigureStandaloneDemo(bool enabled)
    {
        standaloneDemo = enabled;
        if (enabled) gameBridge = null;
    }

    public void ConfigureRuntime(
        Challenge07GameBridge bridge,
        GameObject configuredPanelRoot,
        MapPuzzlePiece[] configuredSixPieces,
        MapPuzzlePiece[] configuredNinePieces,
        GameObject configuredPieces6Root,
        GameObject configuredPieces9Root,
        GameObject configuredTargets6Root,
        GameObject configuredTargets9Root,
        GameObject configuredModeSelectionPanel,
        GameObject configuredGamePanel,
        GameObject configuredRemainingPiecesPanel,
        TMP_Text configuredProgressText,
        GameObject configuredVictoryPanel,
        Button configuredRestartButton,
        Button configuredHintButton,
        Button configuredContinueButton,
        Button configuredSixPiecesButton,
        Button configuredNinePiecesButton,
        Button configuredStartButton,
        TMP_Text configuredHintCounterText,
        Image configuredCompletedMapImage,
        GameObject configuredCastleReveal,
        CanvasGroup configuredDarkener,
        ParticleSystem configuredVictoryParticles,
        AudioSource configuredMusicSource,
        AudioSource configuredEffectsSource,
        AudioClip configuredAmbientMusic,
        AudioClip configuredVictoryClip,
        AudioClip configuredHintClip,
        Challenge07UIAnimator configuredUiAnimator)
    {
        gameBridge = bridge;
        panelRoot = configuredPanelRoot;
        sixPieces = configuredSixPieces;
        ninePieces = configuredNinePieces;
        pieces6Root = configuredPieces6Root;
        pieces9Root = configuredPieces9Root;
        targets6Root = configuredTargets6Root;
        targets9Root = configuredTargets9Root;
        modeSelectionPanel = configuredModeSelectionPanel;
        gamePanel = configuredGamePanel;
        remainingPiecesPanel = configuredRemainingPiecesPanel;
        progressText = configuredProgressText;
        victoryPanel = configuredVictoryPanel;
        restartButton = configuredRestartButton;
        hintButton = configuredHintButton;
        continueButton = configuredContinueButton;
        sixPiecesButton = configuredSixPiecesButton;
        ninePiecesButton = configuredNinePiecesButton;
        startButton = configuredStartButton;
        hintCounterText = configuredHintCounterText;
        completedMapImage = configuredCompletedMapImage;
        castleReveal = configuredCastleReveal;
        backgroundDarkener = configuredDarkener;
        victoryParticles = configuredVictoryParticles;
        musicSource = configuredMusicSource;
        effectsSource = configuredEffectsSource;
        ambientMusic = configuredAmbientMusic;
        victoryClip = configuredVictoryClip;
        hintClip = configuredHintClip;
        uiAnimator = configuredUiAnimator;
        ApplySelectedMode();
        InitializePuzzle();
        HidePanel();
    }

    private void Awake()
    {
        if (gameBridge == null) gameBridge = GetComponentInParent<Challenge07GameBridge>();
        
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MenuPrincipal")
        {
            standaloneDemo = true;
        }
        
        ApplySelectedMode();
    }

    private void Start()
    {
        if (!initialized) InitializePuzzle();
        if (standaloneDemo)
            BeginSession();
        else if (gameBridge == null || !gameBridge.IsActive)
            HidePanel();
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
        StopVictoryRoutine();
        StopMusic();
    }

    public bool BeginSession()
    {
        if (!InitializePuzzle()) return false;
        victoryReported = false;
        sessionStarted = false;
        ResetPuzzleState(false);
        if (panelRoot != null) panelRoot.SetActive(true);
        if (modeSelectionPanel != null) modeSelectionPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (uiAnimator != null) uiAnimator.PlayOpen();
        PlayAmbientMusic();
        return true;
    }

    public void StartPuzzle()
    {
        sessionStarted = true;
        if (modeSelectionPanel != null) modeSelectionPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        RestartPuzzle();
    }

    public void SetSixPieceMode()
    {
        puzzleMode = PuzzleMode.SixPieces;
        ApplySelectedMode();
        InitializePuzzle();
        RestartPuzzle();
    }

    public void SetNinePieceMode()
    {
        puzzleMode = PuzzleMode.NinePieces;
        ApplySelectedMode();
        InitializePuzzle();
        RestartPuzzle();
    }

    public void NotifyPiecePlaced()
    {
        if (puzzleCompleted) return;
        placedPieces = CountPlacedPieces();
        UpdateProgressText();
        if (placedPieces >= CountValidPieces()) CompletePuzzle();
    }

    public void RestartPuzzle()
    {
        ResetPuzzleState(true);
    }

    private void ResetPuzzleState(bool restartMusic)
    {
        StopVictoryRoutine();
        puzzleCompleted = false;
        placedPieces = 0;
        remainingHints = 3;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (remainingPiecesPanel != null) remainingPiecesPanel.SetActive(true);
        if (castleReveal != null) castleReveal.SetActive(false);
        if (completedMapImage != null)
        {
            completedMapImage.gameObject.SetActive(false);
            completedMapImage.rectTransform.localScale = Vector3.one;
        }
        if (backgroundDarkener != null) backgroundDarkener.alpha = 0f;
        if (uiAnimator != null) uiAnimator.ResetVisuals();
        if (victoryParticles != null) victoryParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (puzzlePieces != null)
        {
            foreach (MapPuzzlePiece piece in puzzlePieces)
            {
                if (piece != null) piece.ResetPiece();
            }
        }

        UpdateProgressText();
        UpdateHintCounter();
        if (restartMusic) PlayAmbientMusic();
    }

    public void UseHint()
    {
        if (!sessionStarted || puzzleCompleted || remainingHints <= 0 || puzzlePieces == null) return;
        foreach (MapPuzzlePiece piece in puzzlePieces)
        {
            if (piece == null || piece.IsPlaced) continue;
            piece.PlayHint();
            remainingHints--;
            UpdateHintCounter();
            if (effectsSource != null && hintClip != null) effectsSource.PlayOneShot(hintClip);
            return;
        }
    }

    public void ContinueGame()
    {
        if (!puzzleCompleted) return;
        HidePanel();
        StopMusic();
        onContinuePressed?.Invoke();

        if (!victoryReported && gameBridge != null)
        {
            victoryReported = true;
            gameBridge.ReportPuzzleVictory();
        }
    }

    public void CancelSession()
    {
        HidePanel();
        StopMusic();
    }

    private bool InitializePuzzle()
    {
        if (initialized) return CountValidPieces() > 0;
        if (panelRoot == null) panelRoot = gameObject;
        if (puzzlePieces == null || puzzlePieces.Length == 0)
            puzzlePieces = panelRoot.GetComponentsInChildren<MapPuzzlePiece>(true);

        if (puzzlePieces == null || puzzlePieces.Length == 0)
        {
            Debug.LogError("Challenge07PuzzleController: no hay piezas configuradas.", this);
            return false;
        }

        foreach (MapPuzzlePiece piece in puzzlePieces)
        {
            if (piece != null) piece.Configure(this);
        }

        RemoveButtonListeners();
        if (restartButton != null) restartButton.onClick.AddListener(RestartPuzzle);
        if (hintButton != null) hintButton.onClick.AddListener(UseHint);
        if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
        if (sixPiecesButton != null) sixPiecesButton.onClick.AddListener(SetSixPieceMode);
        if (ninePiecesButton != null) ninePiecesButton.onClick.AddListener(SetNinePieceMode);
        if (startButton != null) startButton.onClick.AddListener(StartPuzzle);
        initialized = true;
        UpdateProgressText();
        return true;
    }

    private void CompletePuzzle()
    {
        if (puzzleCompleted) return;
        puzzleCompleted = true;
        placedPieces = CountValidPieces();
        UpdateProgressText();
        UpdateHintCounter();
        StopVictoryRoutine();
        victoryRoutine = StartCoroutine(VictoryPresentationRoutine());
        onPuzzleCompleted?.Invoke();
        if (standaloneDemo || gameBridge == null)
            Debug.Log("Challenge07 demo: mapa reconstruido.", this);
    }

    private IEnumerator VictoryPresentationRoutine()
    {
        if (musicSource != null) musicSource.volume = ambientVolume * 0.3f;
        if (effectsSource != null && victoryClip != null) effectsSource.PlayOneShot(victoryClip);
        if (completedMapImage != null) completedMapImage.gameObject.SetActive(true);
        if (remainingPiecesPanel != null) remainingPiecesPanel.SetActive(false);
        if (castleReveal != null) castleReveal.SetActive(true);
        if (victoryParticles != null) victoryParticles.Play(true);
        if (puzzleAnimator != null) puzzleAnimator.SetTrigger("Complete");

        RectTransform mapTransform = completedMapImage != null ? completedMapImage.rectTransform : null;
        float elapsed = 0f;
        while (elapsed < victoryAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / victoryAnimationDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            if (backgroundDarkener != null) backgroundDarkener.alpha = Mathf.Lerp(0f, 0.72f, eased);
            if (mapTransform != null) mapTransform.localScale = Vector3.one * Mathf.Lerp(0.88f, 1.04f, eased);
            yield return null;
        }

        if (mapTransform != null) mapTransform.localScale = Vector3.one;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (uiAnimator != null) uiAnimator.PlayVictory();
        victoryRoutine = null;
    }

    private void PlayAmbientMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = ambientMusic;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = ambientVolume;
        if (ambientMusic != null) musicSource.Play();
    }

    private void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    private void HidePanel()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private int CountPlacedPieces()
    {
        int total = 0;
        if (puzzlePieces == null) return total;
        foreach (MapPuzzlePiece piece in puzzlePieces)
            if (piece != null && piece.IsPlaced) total++;
        return total;
    }

    private void ApplySelectedMode()
    {
        bool useSix = puzzleMode == PuzzleMode.SixPieces;
        if (pieces6Root != null) pieces6Root.SetActive(useSix);
        if (targets6Root != null) targets6Root.SetActive(useSix);
        if (pieces9Root != null) pieces9Root.SetActive(!useSix);
        if (targets9Root != null) targets9Root.SetActive(!useSix);
        puzzlePieces = useSix ? sixPieces : ninePieces;
        initialized = false;
        UpdateModeButtonVisuals();
    }

    private void UpdateModeButtonVisuals()
    {
        bool useSix = puzzleMode == PuzzleMode.SixPieces;
        if (piecesCountText != null)
        {
            piecesCountText.text = useSix ? "6" : "9";
        }

        Color selectedBg = new Color(0.48f, 0.25f, 0.05f, 1f);
        Color selectedOutline = new Color(1f, 0.85f, 0.2f, 1f);
        Color unselectedBg = new Color(0.12f, 0.05f, 0.02f, 1f);
        Color unselectedOutline = new Color(0.38f, 0.22f, 0.08f, 1f);

        if (sixPiecesButton != null)
        {
            Image img = sixPiecesButton.GetComponent<Image>();
            if (img != null) img.color = useSix ? selectedBg : unselectedBg;
            Outline outline = sixPiecesButton.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = useSix ? selectedOutline : unselectedOutline;
                outline.effectDistance = useSix ? new Vector2(4f, -4f) : new Vector2(1.5f, -1.5f);
            }
        }

        if (ninePiecesButton != null)
        {
            Image img = ninePiecesButton.GetComponent<Image>();
            if (img != null) img.color = !useSix ? selectedBg : unselectedBg;
            Outline outline = ninePiecesButton.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = !useSix ? selectedOutline : unselectedOutline;
                outline.effectDistance = !useSix ? new Vector2(4f, -4f) : new Vector2(1.5f, -1.5f);
            }
        }
    }

    private int CountValidPieces()
    {
        int total = 0;
        if (puzzlePieces == null) return total;
        foreach (MapPuzzlePiece piece in puzzlePieces)
            if (piece != null) total++;
        return total;
    }

    private void UpdateProgressText()
    {
        if (progressText != null) progressText.text = $"FRAGMENTOS {placedPieces}/{CountValidPieces()}";
    }

    private void UpdateHintCounter()
    {
        if (hintCounterText != null) hintCounterText.text = remainingHints.ToString();
        if (hintButton != null) hintButton.interactable = remainingHints > 0 && !puzzleCompleted;
    }

    private void RemoveButtonListeners()
    {
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartPuzzle);
        if (hintButton != null) hintButton.onClick.RemoveListener(UseHint);
        if (continueButton != null) continueButton.onClick.RemoveListener(ContinueGame);
        if (sixPiecesButton != null) sixPiecesButton.onClick.RemoveListener(SetSixPieceMode);
        if (ninePiecesButton != null) ninePiecesButton.onClick.RemoveListener(SetNinePieceMode);
        if (startButton != null) startButton.onClick.RemoveListener(StartPuzzle);
    }

    private void StopVictoryRoutine()
    {
        if (victoryRoutine != null)
        {
            StopCoroutine(victoryRoutine);
            victoryRoutine = null;
        }
    }

    private void OnValidate()
    {
        victoryAnimationDuration = Mathf.Max(0.05f, victoryAnimationDuration);
    }
}
