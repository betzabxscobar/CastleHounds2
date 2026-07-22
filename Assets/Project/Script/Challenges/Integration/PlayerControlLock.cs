using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerControlLock : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Behaviour[] behavioursToDisable;
    [SerializeField] private bool manageCursor = true;

    private readonly HashSet<string> activeLocks = new HashSet<string>();
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockMode;

    public bool IsLocked => activeLocks.Count > 0;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }

    public void LockControl(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            Debug.LogError("PlayerControlLock: falta reason para bloquear control.", this);
            return;
        }

        bool wasLocked = IsLocked;
        activeLocks.Add(reason);

        if (!wasLocked)
        {
            ApplyLockedState(true);
        }
    }

    public void UnlockControl(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            Debug.LogError("PlayerControlLock: falta reason para desbloquear control.", this);
            return;
        }

        activeLocks.Remove(reason);

        if (!IsLocked)
        {
            ApplyLockedState(false);
        }
    }

    public void ForceUnlockAll()
    {
        activeLocks.Clear();
        ApplyLockedState(false);
    }

    private void ApplyLockedState(bool locked)
    {
        if (playerController != null)
        {
            playerController.SetInputEnabled(!locked);
        }

        if (behavioursToDisable != null)
        {
            foreach (Behaviour behaviour in behavioursToDisable)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = !locked;
                }
            }
        }

        if (!manageCursor)
        {
            return;
        }

        if (locked)
        {
            previousCursorVisible = Cursor.visible;
            previousCursorLockMode = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = previousCursorVisible;
            Cursor.lockState = previousCursorLockMode;
        }
    }
}
