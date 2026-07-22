using System.Collections;
using UnityEngine;

public sealed class InitialWolfVictoryTransition : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EnemyHealth initialWolf;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private PlayerControlLock playerControlLock;
    [SerializeField] private Transform cityEntryPoint;
    [SerializeField] private ChallengeProgressHUD challengeHUD;
    [SerializeField] private GameObject challengeHudRoot;
    [SerializeField] private Behaviour[] behavioursToDisableDuringTransition;

    [Header("Camara opcional")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2.2f, -4f);
    [SerializeField] private Vector3 cameraLookOffset = new Vector3(0f, 0.8f, 0f);

    [Header("Transicion")]
    [SerializeField, Min(0f)] private float wolfDeathDelay = 1.5f;
    [SerializeField] private bool resetChallengeProgressOnTransition = true;

    private const string LockReason = "InitialWolfVictoryTransition";

    private bool transitionStarted;

    private void OnEnable()
    {
        GameEvents.OnEnemyDefeatedWithRole += HandleEnemyDefeated;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDefeatedWithRole -= HandleEnemyDefeated;
    }

    private void HandleEnemyDefeated(EnemyHealth enemyHealth, EnemyRole role)
    {
        if (transitionStarted || role != EnemyRole.InitialWolf)
        {
            return;
        }

        if (initialWolf != null && enemyHealth != initialWolf)
        {
            return;
        }

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        transitionStarted = true;
        Time.timeScale = 1f;

        if (!ValidateRequiredReferences())
        {
            transitionStarted = false;
            yield break;
        }

        if (playerControlLock != null)
        {
            playerControlLock.LockControl(LockReason);
        }

        SetTemporaryBehavioursEnabled(false);

        if (wolfDeathDelay > 0f)
        {
            yield return new WaitForSeconds(wolfDeathDelay);
        }

        TeleportPlayer();
        AlignCamera();

        ChallengeProgressManager progressManager = ChallengeProgressManager.Instance;
        if (resetChallengeProgressOnTransition && progressManager != null)
        {
            progressManager.ResetProgress();
        }

        if (challengeHUD != null)
        {
            challengeHUD.SetVisible(true);
            challengeHUD.Refresh();
        }
        else if (challengeHudRoot != null)
        {
            challengeHudRoot.SetActive(true);
        }

        GameEvents.RaiseMessageRequested("Explora las siete casas abandonadas.");
        SetTemporaryBehavioursEnabled(true);

        if (playerControlLock != null)
        {
            playerControlLock.UnlockControl(LockReason);
        }
    }

    private bool ValidateRequiredReferences()
    {
        if (playerRoot == null)
        {
            Debug.LogError("InitialWolfVictoryTransition: falta playerRoot.", this);
            return false;
        }

        if (cityEntryPoint == null)
        {
            Debug.LogError("InitialWolfVictoryTransition: falta CityEntryPoint.", this);
            return false;
        }

        return true;
    }

    private void TeleportPlayer()
    {
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        playerRoot.SetPositionAndRotation(cityEntryPoint.position, cityEntryPoint.rotation);

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    private void AlignCamera()
    {
        if (cameraTransform == null || playerRoot == null)
        {
            return;
        }

        Vector3 desiredPosition = playerRoot.position + playerRoot.TransformDirection(cameraOffset);
        Vector3 lookPosition = playerRoot.position + cameraLookOffset;
        Vector3 direction = lookPosition - desiredPosition;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = playerRoot.forward;
        }

        cameraTransform.SetPositionAndRotation(desiredPosition, Quaternion.LookRotation(direction.normalized, Vector3.up));
    }

    private void SetTemporaryBehavioursEnabled(bool enabledState)
    {
        if (behavioursToDisableDuringTransition == null)
        {
            return;
        }

        foreach (Behaviour behaviour in behavioursToDisableDuringTransition)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabledState;
            }
        }
    }

    private void OnValidate()
    {
        wolfDeathDelay = Mathf.Max(0f, wolfDeathDelay);
    }
}
