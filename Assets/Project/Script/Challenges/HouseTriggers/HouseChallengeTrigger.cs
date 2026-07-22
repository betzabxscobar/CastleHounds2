using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class HouseChallengeTrigger : MonoBehaviour
{
    private enum TriggerState
    {
        Idle,
        Starting,
        Active,
        Completed,
        Cooldown
    }

    [SerializeField] private string challengeId;
    [SerializeField] private MonoBehaviour challengeGameBehaviour;
    [SerializeField] private PlayerControlLock playerControlLock;
    [SerializeField] private string playerTag = "Player";
    [SerializeField, Min(0f)] private float retryCooldown = 0.75f;

    private const string LockReasonPrefix = "HouseChallenge:";

    private Collider triggerCollider;
    private IChallengeGame challengeGame;
    private TriggerState state = TriggerState.Idle;
    private Coroutine cooldownCoroutine;

    private string LockReason => LockReasonPrefix + challengeId;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning("HouseChallengeTrigger requiere un collider marcado como Is Trigger.", this);
        }

        ResolveChallengeGame();
    }

    private void OnDisable()
    {
        UnsubscribeFromChallenge();

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        if (playerControlLock != null)
        {
            playerControlLock.UnlockControl(LockReason);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        TryStartChallenge();
    }

    public void TryStartChallenge()
    {
        ChallengeProgressManager progressManager = ChallengeProgressManager.Instance;
        if (!ValidateStart(progressManager))
        {
            return;
        }

        if (progressManager.IsChallengeCompleted(challengeId))
        {
            state = TriggerState.Completed;
            GameEvents.RaiseMessageRequested("Este reto ya fue completado.");
            return;
        }

        state = TriggerState.Starting;
        SubscribeToChallenge();

        if (playerControlLock != null)
        {
            playerControlLock.LockControl(LockReason);
        }

        challengeGame.StartChallenge();

        if (!challengeGame.IsActive)
        {
            UnsubscribeFromChallenge();
            RestoreControl();
            state = TriggerState.Idle;
            return;
        }

        state = TriggerState.Active;
    }

    private bool ValidateStart(ChallengeProgressManager progressManager)
    {
        if (state == TriggerState.Starting || state == TriggerState.Active || state == TriggerState.Cooldown)
        {
            return false;
        }

        if (progressManager == null)
        {
            Debug.LogError("HouseChallengeTrigger: falta ChallengeProgressManager.", this);
            enabled = false;
            return false;
        }

        if (!progressManager.IsKnownChallengeId(challengeId))
        {
            Debug.LogError($"HouseChallengeTrigger: ID de reto invalido '{challengeId}'.", this);
            enabled = false;
            return false;
        }

        if (challengeGame == null)
        {
            Debug.LogError("HouseChallengeTrigger: falta controlador del reto.", this);
            enabled = false;
            return false;
        }

        if (challengeGame.ChallengeId != challengeId)
        {
            Debug.LogError($"HouseChallengeTrigger: el controlador usa '{challengeGame.ChallengeId}' pero el trigger usa '{challengeId}'.", this);
            enabled = false;
            return false;
        }

        return true;
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(playerTag) && other.CompareTag(playerTag))
        {
            return true;
        }

        return other.GetComponentInParent<PlayerController>() != null;
    }

    private void ResolveChallengeGame()
    {
        challengeGame = challengeGameBehaviour as IChallengeGame;
        if (challengeGame == null && challengeGameBehaviour != null)
        {
            Debug.LogError("HouseChallengeTrigger: challengeGameBehaviour debe implementar IChallengeGame.", this);
        }
    }

    private void SubscribeToChallenge()
    {
        UnsubscribeFromChallenge();
        challengeGame.ChallengeFinished += HandleChallengeFinished;
    }

    private void UnsubscribeFromChallenge()
    {
        if (challengeGame != null)
        {
            challengeGame.ChallengeFinished -= HandleChallengeFinished;
        }
    }

    private void HandleChallengeFinished(IChallengeGame sender, ChallengeResult result)
    {
        if (sender != challengeGame)
        {
            return;
        }

        UnsubscribeFromChallenge();

        ChallengeProgressManager progressManager = ChallengeProgressManager.Instance;
        if (result == ChallengeResult.Won && progressManager != null)
        {
            progressManager.CompleteChallenge(challengeId);
            state = TriggerState.Completed;
        }
        else if (result == ChallengeResult.Lost)
        {
            GameEvents.RaiseMessageRequested("No completaste el reto. Puedes intentarlo nuevamente.");
            BeginCooldown();
        }
        else
        {
            BeginCooldown();
        }

        RestoreControl();
    }

    private void RestoreControl()
    {
        if (playerControlLock != null)
        {
            playerControlLock.UnlockControl(LockReason);
        }
    }

    private void BeginCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        cooldownCoroutine = StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        state = TriggerState.Cooldown;

        if (retryCooldown > 0f)
        {
            yield return new WaitForSecondsRealtime(retryCooldown);
        }

        state = TriggerState.Idle;
        cooldownCoroutine = null;
    }

    private void OnValidate()
    {
        retryCooldown = Mathf.Max(0f, retryCooldown);
    }
}
