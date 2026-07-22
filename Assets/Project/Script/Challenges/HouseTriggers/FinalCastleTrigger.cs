using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public sealed class FinalCastleTrigger : MonoBehaviour
{
    [SerializeField] private string finalBattleSceneName = "_DemoScene";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private PlayerControlLock playerControlLock;
    [SerializeField] private CastleUnlockController castleUnlockController;

    private const string LockReason = "FinalCastleTrigger";

    private bool isLoadingScene;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning("FinalCastleTrigger requiere un collider marcado como Is Trigger.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoadingScene || !IsPlayer(other))
        {
            return;
        }

        TryEnterFinalBattle();
    }

    public void TryEnterFinalBattle()
    {
        ChallengeProgressManager progressManager = ChallengeProgressManager.Instance;
        if (progressManager == null)
        {
            Debug.LogError("FinalCastleTrigger: falta ChallengeProgressManager.", this);
            return;
        }

        if (!progressManager.AreAllChallengesCompleted)
        {
            if (castleUnlockController != null)
            {
                castleUnlockController.ShowLockedMessage();
            }
            else
            {
                GameEvents.RaiseMessageRequested($"Debes completar los siete retos. Faltan {progressManager.GetRemainingCount()}.");
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(finalBattleSceneName))
        {
            Debug.LogError("FinalCastleTrigger: falta nombre de escena final.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(finalBattleSceneName))
        {
            Debug.LogError($"FinalCastleTrigger: la escena '{finalBattleSceneName}' no esta en Build Settings.", this);
            return;
        }

        isLoadingScene = true;
        Time.timeScale = 1f;

        if (playerControlLock != null)
        {
            playerControlLock.LockControl(LockReason);
        }

        SceneManager.LoadScene(finalBattleSceneName);
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
}
