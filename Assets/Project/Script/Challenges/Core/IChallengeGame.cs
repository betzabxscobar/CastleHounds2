using System;

public interface IChallengeGame
{
    event Action<IChallengeGame, ChallengeResult> ChallengeFinished;

    string ChallengeId { get; }
    bool IsActive { get; }

    void StartChallenge();
    void CancelChallenge();
}
