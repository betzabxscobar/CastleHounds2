using TMPro;
using UnityEngine;

public sealed class ChallengeTestPanel : MonoBehaviour
{
    [Header("Temporal de integracion")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text challengeIdText;
    [SerializeField] private MonoBehaviour[] observedChallengeBehaviours;

    private IChallengeGame activeChallenge;
    private bool resultSubmitted;

    private void Awake()
    {
        HidePanel();
    }

    private void OnEnable()
    {
        SubscribeToObservedChallenges();
    }

    private void OnDisable()
    {
        UnsubscribeFromObservedChallenges();
        activeChallenge = null;
        resultSubmitted = false;
    }

    public void SimulateVictory()
    {
        SubmitResult(ChallengeResult.Won);
    }

    public void SimulateDefeat()
    {
        SubmitResult(ChallengeResult.Lost);
    }

    public void CancelChallenge()
    {
        SubmitResult(ChallengeResult.Cancelled);
    }

    public void Configure(GameObject configuredPanelRoot, TMP_Text configuredChallengeIdText, MonoBehaviour[] configuredObservedChallengeBehaviours)
    {
        UnsubscribeFromObservedChallenges();
        panelRoot = configuredPanelRoot;
        challengeIdText = configuredChallengeIdText;
        observedChallengeBehaviours = configuredObservedChallengeBehaviours;
        SubscribeToObservedChallenges();
        HidePanel();
    }

    private void SubscribeToObservedChallenges()
    {
        if (observedChallengeBehaviours == null)
        {
            return;
        }

        foreach (MonoBehaviour behaviour in observedChallengeBehaviours)
        {
            if (behaviour is IChallengeGame challengeGame)
            {
                challengeGame.ChallengeStarted += HandleChallengeStarted;
                challengeGame.ChallengeFinished += HandleChallengeFinished;
            }
        }
    }

    private void UnsubscribeFromObservedChallenges()
    {
        if (observedChallengeBehaviours == null)
        {
            return;
        }

        foreach (MonoBehaviour behaviour in observedChallengeBehaviours)
        {
            if (behaviour is IChallengeGame challengeGame)
            {
                challengeGame.ChallengeStarted -= HandleChallengeStarted;
                challengeGame.ChallengeFinished -= HandleChallengeFinished;
            }
        }
    }

    private void HandleChallengeStarted(IChallengeGame challengeGame)
    {
        activeChallenge = challengeGame;
        resultSubmitted = false;

        if (challengeIdText != null)
        {
            challengeIdText.text = challengeGame.ChallengeId;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    private void HandleChallengeFinished(IChallengeGame challengeGame, ChallengeResult result)
    {
        if (challengeGame != activeChallenge)
        {
            return;
        }

        HidePanel();
        activeChallenge = null;
        resultSubmitted = false;
    }

    private void SubmitResult(ChallengeResult result)
    {
        if (resultSubmitted || activeChallenge == null)
        {
            return;
        }

        resultSubmitted = true;

        if (activeChallenge is ChallengeGameController controller)
        {
            if (result == ChallengeResult.Won)
            {
                controller.SimulateVictory();
            }
            else if (result == ChallengeResult.Lost)
            {
                controller.SimulateDefeat();
            }
            else
            {
                controller.CancelChallenge();
            }
        }
        else
        {
            Debug.LogError("ChallengeTestPanel requiere un ChallengeGameController para simular resultados.", this);
            resultSubmitted = false;
        }
    }

    private void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}
