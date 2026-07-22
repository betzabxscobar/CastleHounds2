using UnityEngine;

public sealed class CastleUnlockController : MonoBehaviour
{
    [SerializeField] private Collider blockingCollider;
    [SerializeField] private GameObject blockingObject;
    [SerializeField] private bool showUnlockMessage = true;

    private ChallengeProgressManager progressManager;
    private bool unlockProcessed;

    private void OnEnable()
    {
        progressManager = ChallengeProgressManager.Instance;
        if (progressManager == null)
        {
            Debug.LogError("CastleUnlockController: falta ChallengeProgressManager.", this);
            enabled = false;
            return;
        }

        progressManager.OnAllChallengesCompleted += HandleAllChallengesCompleted;
        ApplyCurrentState(false);
    }

    private void OnDisable()
    {
        if (progressManager != null)
        {
            progressManager.OnAllChallengesCompleted -= HandleAllChallengesCompleted;
        }
    }

    public bool IsUnlocked()
    {
        return progressManager != null && progressManager.AreAllChallengesCompleted;
    }

    public void ShowLockedMessage()
    {
        if (progressManager == null)
        {
            return;
        }

        GameEvents.RaiseMessageRequested($"Debes completar los siete retos. Faltan {progressManager.GetRemainingCount()}.");
    }

    private void HandleAllChallengesCompleted()
    {
        ApplyCurrentState(showUnlockMessage);
    }

    private void ApplyCurrentState(bool notify)
    {
        bool unlocked = IsUnlocked();

        if (blockingCollider != null)
        {
            blockingCollider.enabled = !unlocked;
        }

        if (blockingObject != null)
        {
            blockingObject.SetActive(!unlocked);
        }

        if (unlocked && !unlockProcessed)
        {
            unlockProcessed = true;

            if (notify)
            {
                GameEvents.RaiseMessageRequested("El castillo central ha sido desbloqueado.");
            }
        }
    }
}
