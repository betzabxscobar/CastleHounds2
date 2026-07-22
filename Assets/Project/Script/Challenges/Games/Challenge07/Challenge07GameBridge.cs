using UnityEngine;

public sealed class Challenge07GameBridge : ChallengeGameController
{
    protected override string FallbackChallengeId => "house_challenge_07";

    [SerializeField] private Challenge07PuzzleController puzzleController;

    public Challenge07PuzzleController PuzzleController => puzzleController;

    public void ConfigureRuntime(Challenge07PuzzleController controller)
    {
        puzzleController = controller;
    }

    public override void StartChallenge()
    {
        if (IsActive)
        {
            return;
        }

        base.StartChallenge();

        if (IsActive && (puzzleController == null || !puzzleController.BeginSession()))
        {
            Debug.LogError("Challenge07GameBridge: no se pudo iniciar la interfaz del rompecabezas.", this);
            base.CancelChallenge();
        }
    }

    public override void CancelChallenge()
    {
        if (puzzleController != null)
        {
            puzzleController.CancelSession();
        }

        base.CancelChallenge();
    }

    public void ReportPuzzleVictory()
    {
        SubmitResult(ChallengeResult.Won);
    }
}
