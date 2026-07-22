using TMPro;
using UnityEngine;

public sealed class ChallengeProgressHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject root;
    [SerializeField] private bool visibleOnEnable = true;

    private ChallengeProgressManager progressManager;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }
    }

    private void OnEnable()
    {
        progressManager = ChallengeProgressManager.Instance;
        if (progressManager == null)
        {
            Debug.LogError("ChallengeProgressHUD: falta ChallengeProgressManager.", this);
            enabled = false;
            return;
        }

        progressManager.OnProgressChanged += HandleProgressChanged;
        progressManager.OnChallengeCompleted += HandleChallengeCompleted;
        SetVisible(visibleOnEnable);
        Refresh();
    }

    private void OnDisable()
    {
        if (progressManager != null)
        {
            progressManager.OnProgressChanged -= HandleProgressChanged;
            progressManager.OnChallengeCompleted -= HandleChallengeCompleted;
        }
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.SetActive(visible);
        }
    }

    public void Refresh()
    {
        if (progressManager == null)
        {
            return;
        }

        HandleProgressChanged(progressManager.CompletedCount, progressManager.TotalChallenges);
    }

    private void HandleChallengeCompleted(string challengeId, int completed, int total)
    {
        HandleProgressChanged(completed, total);
    }

    private void HandleProgressChanged(int completed, int total)
    {
        if (progressText != null)
        {
            progressText.text = $"Retos completados: {completed}/{total}";
        }
    }
}
