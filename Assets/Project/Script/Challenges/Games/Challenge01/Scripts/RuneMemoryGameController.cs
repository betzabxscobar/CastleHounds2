using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneMemoryGameController : ChallengeGameController
{
    private enum RuneMemoryState
    {
        Inactive,
        WaitingToStart,
        ShowingSequence,
        WaitingForInput,
        RoundCompleted,
        Failed,
        Completed,
        Cancelling
    }

    [SerializeField] private RuneMemoryPanel panel;
    [SerializeField] private RuneMemoryButton[] runeButtons;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip highlightClip;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private int initialSequenceLength = 3;
    [SerializeField] private float prepareDelay = 1f;
    [SerializeField] private float highlightDuration = 0.5f;
    [SerializeField] private float gapBetweenRunes = 0.2f;
    [SerializeField] private float roundTransitionDelay = 1f;
    [SerializeField] private float victoryDelay = 1.5f;
    [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.35f;

    private readonly List<int> currentSequence = new List<int>();

    private RuneMemoryState state = RuneMemoryState.Inactive;
    private Coroutine activeRoutine;
    private int currentRound;
    private int expectedInputIndex;
    private bool acceptingInput;
    private bool resultSubmitted;

    protected override string FallbackChallengeId => ChallengeProgressManager.HouseChallenge01;

    private void Awake()
    {
        WirePanelCallbacks();
        ConfigureButtons();
        HidePanelIfPresent();
    }

    private void OnDestroy()
    {
        StopActiveRoutine();
        StopAmbient();
        ClearButtonHandlers();
    }

    public void ConfigureRuntime(
        RuneMemoryPanel configuredPanel,
        RuneMemoryButton[] configuredRuneButtons,
        AudioSource configuredSfxSource,
        AudioSource configuredAmbientSource,
        AudioClip configuredAmbientClip,
        AudioClip configuredHighlightClip,
        AudioClip configuredCorrectClip,
        AudioClip configuredErrorClip,
        AudioClip configuredVictoryClip)
    {
        panel = configuredPanel;
        runeButtons = configuredRuneButtons;
        sfxSource = configuredSfxSource;
        ambientSource = configuredAmbientSource;
        ambientClip = configuredAmbientClip;
        highlightClip = configuredHighlightClip;
        correctClip = configuredCorrectClip;
        errorClip = configuredErrorClip;
        victoryClip = configuredVictoryClip;

        WirePanelCallbacks();
        ConfigureButtons();
        HidePanelIfPresent();
    }

    public override void StartChallenge()
    {
        if (IsActive || state != RuneMemoryState.Inactive)
        {
            return;
        }

        if (!ValidateReferences())
        {
            return;
        }

        base.StartChallenge();

        if (!IsActive)
        {
            return;
        }

        BeginSession();
    }

    public override void CancelChallenge()
    {
        if (state == RuneMemoryState.Cancelling || resultSubmitted)
        {
            return;
        }

        state = RuneMemoryState.Cancelling;
        StopActiveRoutine();
        acceptingInput = false;
        StopAmbient();
        ResetPanelForClose();
        SubmitFinalResult(ChallengeResult.Cancelled);
    }

    public void StartSequence()
    {
        if (!IsActive || state != RuneMemoryState.WaitingToStart)
        {
            return;
        }

        StartAttemptFromFirstRound();
    }

    public void Retry()
    {
        if (!IsActive || state != RuneMemoryState.Failed)
        {
            return;
        }

        StartAttemptFromFirstRound();
    }

    public void Exit()
    {
        CancelChallenge();
    }

    public void HandleRunePressed(int runeIndex)
    {
        if (!IsActive || !acceptingInput || state != RuneMemoryState.WaitingForInput)
        {
            return;
        }

        if (expectedInputIndex < 0 || expectedInputIndex >= currentSequence.Count)
        {
            Debug.LogError("RuneMemoryGameController: indice de entrada fuera de rango.", this);
            FailCurrentAttempt();
            return;
        }

        RuneMemoryButton runeButton = GetRuneButton(runeIndex);
        if (runeButton != null)
        {
            runeButton.PlayHighlight(highlightDuration * 0.65f, sfxSource, highlightClip);
        }

        if (currentSequence[expectedInputIndex] != runeIndex)
        {
            FailCurrentAttempt();
            return;
        }

        expectedInputIndex++;

        if (expectedInputIndex >= currentSequence.Count)
        {
            CompleteCurrentRound();
        }
    }

    private void BeginSession()
    {
        resultSubmitted = false;
        state = RuneMemoryState.WaitingToStart;
        currentRound = 1;
        expectedInputIndex = 0;
        acceptingInput = false;
        currentSequence.Clear();

        PlayAmbient();
        panel.Show();
        panel.SetTitle("MEMORIA DE RUNAS");
        panel.SetInstruction("Observa la secuencia y repetela.");
        panel.SetRound(currentRound, totalRounds);
        panel.SetStatus("Pulsa COMENZAR");
        panel.SetStartVisible(true);
        panel.SetRetryVisible(false);
        panel.SetExitVisible(true);
        panel.SetRunesInteractable(false);
        panel.ResetRunesVisual();
    }

    private void StartAttemptFromFirstRound()
    {
        StopActiveRoutine();
        currentRound = 1;
        expectedInputIndex = 0;
        acceptingInput = false;
        state = RuneMemoryState.ShowingSequence;
        panel.SetStartVisible(false);
        panel.SetRetryVisible(false);
        panel.SetExitVisible(true);
        panel.SetRunesInteractable(false);
        panel.ResetRunesVisual();
        activeRoutine = StartCoroutine(PlayRoundRoutine());
    }

    private IEnumerator PlayRoundRoutine()
    {
        state = RuneMemoryState.ShowingSequence;
        acceptingInput = false;
        panel.SetRound(currentRound, totalRounds);
        panel.SetStatus("Preparate...");
        panel.SetRunesInteractable(false);

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, prepareDelay));

        GenerateSequence(GetCurrentSequenceLength());

        foreach (int runeIndex in currentSequence)
        {
            RuneMemoryButton runeButton = GetRuneButton(runeIndex);
            if (runeButton != null)
            {
                yield return runeButton.PlayHighlightRoutine(highlightDuration, sfxSource, highlightClip);
            }

            yield return new WaitForSecondsRealtime(Mathf.Max(0f, gapBetweenRunes));
        }

        expectedInputIndex = 0;
        acceptingInput = true;
        state = RuneMemoryState.WaitingForInput;
        panel.SetStatus("Tu turno");
        panel.SetRunesInteractable(true);
        activeRoutine = null;
    }

    private void CompleteCurrentRound()
    {
        acceptingInput = false;
        panel.SetRunesInteractable(false);
        state = RuneMemoryState.RoundCompleted;

        if (sfxSource != null && correctClip != null)
        {
            sfxSource.PlayOneShot(correctClip);
        }

        panel.SetStatus("Secuencia correcta");

        if (currentRound >= totalRounds)
        {
            CompleteChallenge();
            return;
        }

        currentRound++;
        StopActiveRoutine();
        activeRoutine = StartCoroutine(NextRoundRoutine());
    }

    private IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, roundTransitionDelay));
        activeRoutine = StartCoroutine(PlayRoundRoutine());
    }

    private void FailCurrentAttempt()
    {
        acceptingInput = false;
        state = RuneMemoryState.Failed;
        StopActiveRoutine();
        panel.SetRunesInteractable(false);
        panel.SetStatus("Secuencia incorrecta");
        panel.SetStartVisible(false);
        panel.SetRetryVisible(true);
        panel.SetExitVisible(true);

        if (sfxSource != null && errorClip != null)
        {
            sfxSource.PlayOneShot(errorClip);
        }
    }

    private void CompleteChallenge()
    {
        if (resultSubmitted)
        {
            return;
        }

        state = RuneMemoryState.Completed;
        acceptingInput = false;
        panel.SetRunesInteractable(false);
        panel.SetStatus("Reto completado");
        panel.SetStartVisible(false);
        panel.SetRetryVisible(false);
        panel.SetExitVisible(false);
        StopActiveRoutine();
        activeRoutine = StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        if (sfxSource != null && victoryClip != null)
        {
            sfxSource.PlayOneShot(victoryClip);
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, victoryDelay));

        StopAmbient();
        ResetPanelForClose();
        SubmitFinalResult(ChallengeResult.Won);
    }

    private void SubmitFinalResult(ChallengeResult result)
    {
        if (resultSubmitted)
        {
            return;
        }

        resultSubmitted = true;
        acceptingInput = false;
        state = RuneMemoryState.Inactive;

        if (result == ChallengeResult.Cancelled)
        {
            base.CancelChallenge();
            return;
        }

        SubmitResult(result);
    }

    private void GenerateSequence(int length)
    {
        currentSequence.Clear();

        int buttonCount = runeButtons != null ? runeButtons.Length : 0;
        for (int i = 0; i < length; i++)
        {
            currentSequence.Add(Random.Range(0, buttonCount));
        }
    }

    private int GetCurrentSequenceLength()
    {
        return initialSequenceLength + Mathf.Max(0, currentRound - 1);
    }

    private RuneMemoryButton GetRuneButton(int runeIndex)
    {
        if (runeButtons == null || runeIndex < 0 || runeIndex >= runeButtons.Length)
        {
            return null;
        }

        return runeButtons[runeIndex];
    }

    private bool ValidateReferences()
    {
        if (panel == null)
        {
            Debug.LogError("RuneMemoryGameController: falta RuneMemoryPanel.", this);
            return false;
        }

        if (runeButtons == null || runeButtons.Length < 4)
        {
            Debug.LogError("RuneMemoryGameController: faltan los cuatro RuneMemoryButton.", this);
            return false;
        }

        for (int i = 0; i < runeButtons.Length; i++)
        {
            if (runeButtons[i] == null)
            {
                Debug.LogError($"RuneMemoryGameController: falta RuneMemoryButton en indice {i}.", this);
                return false;
            }
        }

        return true;
    }

    private void ConfigureButtons()
    {
        if (runeButtons == null)
        {
            return;
        }

        for (int i = 0; i < runeButtons.Length; i++)
        {
            if (runeButtons[i] != null)
            {
                runeButtons[i].SetClickHandler(HandleRunePressed);
                runeButtons[i].SetInteractable(false);
            }
        }
    }

    private void ClearButtonHandlers()
    {
        if (runeButtons == null)
        {
            return;
        }

        foreach (RuneMemoryButton runeButton in runeButtons)
        {
            if (runeButton != null)
            {
                runeButton.SetClickHandler(null);
            }
        }
    }

    private void WirePanelCallbacks()
    {
        if (panel != null)
        {
            panel.SetCallbacks(StartSequence, Retry, Exit);
        }
    }

    private void PlayAmbient()
    {
        if (ambientSource == null)
        {
            return;
        }

        ambientSource.Stop();
        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.spatialBlend = 0f;
        ambientSource.volume = ambientVolume;

        if (ambientClip != null)
        {
            ambientSource.Play();
        }
    }

    private void StopAmbient()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    private void ResetPanelForClose()
    {
        if (panel == null)
        {
            return;
        }

        panel.SetRunesInteractable(false);
        panel.ResetRunesVisual();
        panel.Hide();
    }

    private void HidePanelIfPresent()
    {
        if (panel != null)
        {
            panel.Hide();
        }
    }

    private void OnValidate()
    {
        totalRounds = Mathf.Max(1, totalRounds);
        initialSequenceLength = Mathf.Max(1, initialSequenceLength);
        prepareDelay = Mathf.Max(0f, prepareDelay);
        highlightDuration = Mathf.Max(0.05f, highlightDuration);
        gapBetweenRunes = Mathf.Max(0f, gapBetweenRunes);
        roundTransitionDelay = Mathf.Max(0f, roundTransitionDelay);
        victoryDelay = Mathf.Max(0f, victoryDelay);
    }
}
