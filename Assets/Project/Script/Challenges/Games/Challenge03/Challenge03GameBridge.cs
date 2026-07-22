using UnityEngine;

// Puente entre el sistema de retos (ChallengeGameController) y el Acertijo de
// Adrian. A diferencia de Challenge01/Challenge02 (que heredan directamente
// de su controlador de juego), aqui el bridge compone un AcertijoManager y
// escucha sus callbacks: AcertijoManager ya tenia su propio ciclo de vida de
// UI (mostrar/ocultar panel, preguntas, audio) y separar ambas
// responsabilidades evita reescribir esa logica dentro de un
// ChallengeGameController.
public sealed class Challenge03GameBridge : ChallengeGameController
{
    [SerializeField] private AcertijoManager acertijoManager;

    protected override string FallbackChallengeId => "house_challenge_03";

    public void ConfigureRuntime(AcertijoManager configuredManager)
    {
        acertijoManager = configuredManager;
        WireCallbacks();
    }

    private void Awake()
    {
        WireCallbacks();
    }

    private void WireCallbacks()
    {
        if (acertijoManager == null)
        {
            return;
        }

        acertijoManager.OnWon = HandleWon;
        acertijoManager.OnExitRequested = HandleExitRequested;
    }

    protected override void OnChallengeStarted()
    {
        if (acertijoManager == null)
        {
            Debug.LogError("Challenge03GameBridge: falta AcertijoManager.", this);
            SubmitResult(ChallengeResult.Cancelled);
            return;
        }

        acertijoManager.Show();
    }

    public override void CancelChallenge()
    {
        if (!IsActive)
        {
            return;
        }

        acertijoManager?.Hide();
        base.CancelChallenge();
    }

    private void HandleWon()
    {
        if (!IsActive)
        {
            return;
        }

        acertijoManager?.Hide();
        SubmitResult(ChallengeResult.Won);
    }

    private void HandleExitRequested()
    {
        CancelChallenge();
    }
}
