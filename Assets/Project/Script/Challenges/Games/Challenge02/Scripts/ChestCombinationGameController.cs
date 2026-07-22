using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestCombinationGameController : ChallengeGameController
{
    private enum ChestCombinationState
    {
        Inactive,
        Ready,
        EnteringCombination,
        Checking,
        Incorrect,
        OpeningChest,
        Completed,
        Cancelling
    }

    [SerializeField] private ChestCombinationPanel panel;
    [SerializeField] private int[] correctCombination = { 2, 3, 1 };
    [SerializeField, TextArea] private string clueText = "Dos caballeros custodian tres antorchas y una corona.";
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private AudioClip numberPressClip;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip incorrectClip;
    [SerializeField] private AudioClip chestOpeningClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private float feedbackDelay = 0.75f;
    [SerializeField] private float completionDelay = 1.5f;
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.35f;

    private const int CombinationLength = 3;

    private readonly List<int> enteredDigits = new List<int>(CombinationLength);
    private readonly int[] displayDigits = new int[CombinationLength];

    private ChestCombinationState state = ChestCombinationState.Inactive;
    private Coroutine activeRoutine;
    private bool resultSubmitted;

    protected override string FallbackChallengeId => ChallengeProgressManager.HouseChallenge02;

    private void Awake()
    {
        WirePanelCallbacks();
        HidePanelIfPresent();
    }

    private void OnDestroy()
    {
        StopActiveRoutine();
        StopAmbience();
    }

    public void ConfigureRuntime(
        ChestCombinationPanel configuredPanel,
        AudioSource configuredSfxSource,
        AudioSource configuredAmbienceSource,
        AudioClip configuredAmbienceClip,
        AudioClip configuredNumberPressClip,
        AudioClip configuredCorrectClip,
        AudioClip configuredIncorrectClip,
        AudioClip configuredChestOpeningClip,
        AudioClip configuredVictoryClip)
    {
        panel = configuredPanel;
        sfxSource = configuredSfxSource;
        ambienceSource = configuredAmbienceSource;
        ambienceClip = configuredAmbienceClip;
        numberPressClip = configuredNumberPressClip;
        correctClip = configuredCorrectClip;
        incorrectClip = configuredIncorrectClip;
        chestOpeningClip = configuredChestOpeningClip;
        victoryClip = configuredVictoryClip;

        WirePanelCallbacks();
        HidePanelIfPresent();
    }

    public override void StartChallenge()
    {
        if (IsActive || state != ChestCombinationState.Inactive)
        {
            return;
        }

        if (!ValidateReferences() || !ValidateCombination())
        {
            return;
        }

        base.StartChallenge();

        if (!IsActive)
        {
            return;
        }

        try
        {
            BeginSession();
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"ChestCombinationGameController: error al abrir el reto. Se cancelara para restaurar el control. {exception}", this);
            StopActiveRoutine();
            StopAmbience();

            if (panel != null)
            {
                panel.Hide();
            }

            SubmitFinalResult(ChallengeResult.Cancelled);
        }
    }

    public override void CancelChallenge()
    {
        if (resultSubmitted || state == ChestCombinationState.Cancelling || state == ChestCombinationState.Completed)
        {
            return;
        }

        state = ChestCombinationState.Cancelling;
        StopActiveRoutine();
        StopAmbience();
        enteredDigits.Clear();

        if (panel != null)
        {
            panel.SetNumberButtonsInteractable(false);
            panel.SetConfirmClearInteractable(false);
            panel.SetExitInteractable(false);
            panel.ClearSlots();
            panel.Hide();
        }

        SubmitFinalResult(ChallengeResult.Cancelled);
    }

    public void HandleNumberPressed(int number)
    {
        if (state != ChestCombinationState.EnteringCombination || !IsActive || enteredDigits.Count >= CombinationLength)
        {
            return;
        }

        enteredDigits.Add(Mathf.Clamp(number, 0, 9));
        RefreshDigits();
        PlayOneShot(numberPressClip);
    }

    public void ClearLastDigit()
    {
        if (state != ChestCombinationState.EnteringCombination || !IsActive)
        {
            return;
        }

        if (enteredDigits.Count == 0)
        {
            return;
        }

        enteredDigits.RemoveAt(enteredDigits.Count - 1);
        RefreshDigits();
        panel.SetStatus("Introduce tres numeros.");
    }

    public void ConfirmCombination()
    {
        if (state != ChestCombinationState.EnteringCombination || !IsActive)
        {
            return;
        }

        if (enteredDigits.Count != CombinationLength)
        {
            panel.SetStatus("Introduce los tres numeros.");
            return;
        }

        StopActiveRoutine();

        if (IsCombinationCorrect())
        {
            activeRoutine = StartCoroutine(CorrectRoutine());
        }
        else
        {
            activeRoutine = StartCoroutine(IncorrectRoutine());
        }
    }

    public void Retry()
    {
        if (state != ChestCombinationState.Incorrect || !IsActive)
        {
            return;
        }

        enteredDigits.Clear();
        state = ChestCombinationState.EnteringCombination;
        panel.ResetForNewAttempt(clueText);
        RefreshDigits();
    }

    public void Exit()
    {
        CancelChallenge();
    }

    private void BeginSession()
    {
        resultSubmitted = false;
        state = ChestCombinationState.Ready;
        enteredDigits.Clear();

        panel.Show();
        panel.ResetForNewAttempt(clueText);
        RefreshDigits();
        PlayAmbience();
        state = ChestCombinationState.EnteringCombination;
    }

    private IEnumerator IncorrectRoutine()
    {
        state = ChestCombinationState.Checking;
        panel.SetNumberButtonsInteractable(false);
        panel.SetConfirmClearInteractable(false);
        panel.SetStatus("Combinacion incorrecta.");
        panel.SetIncorrectFeedback();
        PlayOneShot(incorrectClip);

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, feedbackDelay));

        if (resultSubmitted || state == ChestCombinationState.Cancelling)
        {
            yield break;
        }

        state = ChestCombinationState.Incorrect;
        panel.SetRetryVisible(true);
        panel.SetExitInteractable(true);
        activeRoutine = null;
    }

    private IEnumerator CorrectRoutine()
    {
        state = ChestCombinationState.Checking;
        panel.SetNumberButtonsInteractable(false);
        panel.SetConfirmClearInteractable(false);
        panel.SetExitInteractable(false);
        panel.SetRetryVisible(false);
        panel.SetStatus("Combinacion correcta.");
        panel.SetCorrectFeedback();
        PlayOneShot(correctClip);

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, feedbackDelay));

        if (resultSubmitted || state == ChestCombinationState.Cancelling)
        {
            yield break;
        }

        state = ChestCombinationState.OpeningChest;
        panel.SetChestOpen(true);
        panel.SetRewardsVisible(true);
        panel.SetStatus("Cofre abierto.");
        PlayOneShot(chestOpeningClip);

        yield return new WaitForSecondsRealtime(0.35f);

        PlayOneShot(victoryClip);

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, completionDelay));

        StopAmbience();
        panel.Hide();
        state = ChestCombinationState.Completed;
        activeRoutine = null;
        SubmitFinalResult(ChallengeResult.Won);
    }

    private void RefreshDigits()
    {
        for (int i = 0; i < displayDigits.Length; i++)
        {
            displayDigits[i] = i < enteredDigits.Count ? enteredDigits[i] : 0;
        }

        if (panel != null)
        {
            panel.SetDigits(displayDigits, enteredDigits.Count);
            panel.ResetSlotsVisual();
        }
    }

    private bool IsCombinationCorrect()
    {
        for (int i = 0; i < CombinationLength; i++)
        {
            if (enteredDigits[i] != correctCombination[i])
            {
                return false;
            }
        }

        return true;
    }

    private bool ValidateReferences()
    {
        if (panel == null)
        {
            Debug.LogError("ChestCombinationGameController: falta ChestCombinationPanel.", this);
            return false;
        }

        return true;
    }

    private bool ValidateCombination()
    {
        if (correctCombination == null || correctCombination.Length != CombinationLength)
        {
            Debug.LogError("ChestCombinationGameController: la combinacion correcta debe tener exactamente tres numeros.", this);
            return false;
        }

        for (int i = 0; i < correctCombination.Length; i++)
        {
            if (correctCombination[i] < 0 || correctCombination[i] > 9)
            {
                Debug.LogError($"ChestCombinationGameController: numero invalido en la combinacion: {correctCombination[i]}.", this);
                return false;
            }
        }

        return true;
    }

    private void WirePanelCallbacks()
    {
        if (panel != null)
        {
            panel.SetCallbacks(HandleNumberPressed, ConfirmCombination, ClearLastDigit, Retry, Exit);
        }
    }

    private void PlayAmbience()
    {
        if (ambienceSource == null)
        {
            Debug.LogWarning("ChestCombinationGameController: no se reproduce ambiente porque falta AmbienceSource.", this);
            return;
        }

        if (!ambienceSource.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("ChestCombinationGameController: no se reproduce ambiente porque el AudioSource esta en un GameObject inactivo.", ambienceSource);
            return;
        }

        if (!ambienceSource.enabled)
        {
            ambienceSource.enabled = true;
        }

        if (ambienceClip == null)
        {
            Debug.LogWarning("ChestCombinationGameController: no se reproduce ambiente porque falta AmbienceClip.", this);
            return;
        }

        ambienceSource.clip = ambienceClip;
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.volume = ambienceVolume;

        if (!ambienceSource.isPlaying)
        {
            ambienceSource.Play();
        }
    }

    private void StopAmbience()
    {
        if (ambienceSource != null && ambienceSource.enabled)
        {
            ambienceSource.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
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

    private void SubmitFinalResult(ChallengeResult result)
    {
        if (resultSubmitted)
        {
            return;
        }

        resultSubmitted = true;
        state = ChestCombinationState.Inactive;

        if (result == ChallengeResult.Cancelled)
        {
            base.CancelChallenge();
            return;
        }

        SubmitResult(result);
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
        feedbackDelay = Mathf.Max(0f, feedbackDelay);
        completionDelay = Mathf.Max(0f, completionDelay);

        if (correctCombination == null || correctCombination.Length != CombinationLength)
        {
            correctCombination = new[] { 2, 3, 1 };
        }

        for (int i = 0; i < correctCombination.Length; i++)
        {
            correctCombination[i] = Mathf.Clamp(correctCombination[i], 0, 9);
        }
    }
}
