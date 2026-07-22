using System;
using UnityEngine;

public class ChallengeGameController : MonoBehaviour, IChallengeGame
{
    [SerializeField] private string challengeId;

    public event Action<IChallengeGame> ChallengeStarted;
    public event Action<IChallengeGame, ChallengeResult> ChallengeFinished;

    public string ChallengeId => string.IsNullOrWhiteSpace(challengeId) ? FallbackChallengeId : challengeId;
    public bool IsActive { get; private set; }

    protected virtual string FallbackChallengeId => string.Empty;

    public virtual void StartChallenge()
    {
        if (IsActive)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ChallengeId))
        {
            Debug.LogError("ChallengeGameController: falta ChallengeId.", this);
            return;
        }

        IsActive = true;
        OnChallengeStarted();
        ChallengeStarted?.Invoke(this);
    }

    public virtual void CancelChallenge()
    {
        SubmitResult(ChallengeResult.Cancelled);
    }

    public void SimulateVictory()
    {
        SubmitResult(ChallengeResult.Won);
    }

    public void SimulateDefeat()
    {
        SubmitResult(ChallengeResult.Lost);
    }

    protected void SubmitResult(ChallengeResult result)
    {
        if (!IsActive || result == ChallengeResult.None)
        {
            return;
        }

        IsActive = false;
        OnChallengeFinished(result);
        ChallengeFinished?.Invoke(this, result);
    }

    protected virtual void OnChallengeStarted()
    {
    }

    protected virtual void OnChallengeFinished(ChallengeResult result)
    {
    }
}
