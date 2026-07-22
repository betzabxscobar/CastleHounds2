using UnityEngine;

public sealed class AvatarSelectionApplier : MonoBehaviour
{
    [SerializeField] private GameObject[] avatarRoots;
    [SerializeField] private int defaultAvatarIndex;

    private void Awake()
    {
        ApplySavedAvatar();
    }

    public void ApplySavedAvatar()
    {
        if (avatarRoots == null || avatarRoots.Length == 0)
        {
            Debug.LogWarning("AvatarSelectionApplier no tiene avatares configurados.", this);
            return;
        }

        int selectedIndex = PlayerPrefs.GetInt(AvatarSelector.AvatarSeleccionadoKey, defaultAvatarIndex);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, avatarRoots.Length - 1);

        bool activatedAny = false;
        for (int i = 0; i < avatarRoots.Length; i++)
        {
            GameObject avatarRoot = avatarRoots[i];
            if (avatarRoot == null)
            {
                continue;
            }

            bool shouldBeActive = i == selectedIndex;
            avatarRoot.SetActive(shouldBeActive);
            activatedAny |= shouldBeActive;
        }

        if (!activatedAny)
        {
            Debug.LogWarning("AvatarSelectionApplier no pudo activar el avatar seleccionado porque la referencia es nula.", this);
        }
    }

    private void OnValidate()
    {
        if (avatarRoots == null || avatarRoots.Length == 0)
        {
            defaultAvatarIndex = 0;
            return;
        }

        defaultAvatarIndex = Mathf.Clamp(defaultAvatarIndex, 0, avatarRoots.Length - 1);
    }
}
