using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ChallengeProgressManager : MonoBehaviour
{
    public const string HouseChallenge01 = "house_challenge_01";
    public const string HouseChallenge02 = "house_challenge_02";
    public const string HouseChallenge03 = "house_challenge_03";
    public const string HouseChallenge04 = "house_challenge_04";
    public const string HouseChallenge05 = "house_challenge_05";
    public const string HouseChallenge06 = "house_challenge_06";
    public const string HouseChallenge07 = "house_challenge_07";

    private const string PlayerPrefsPrefix = "CastleHounds2.SevenChallenges.";

    private static readonly string[] ValidChallengeIds =
    {
        HouseChallenge01,
        HouseChallenge02,
        HouseChallenge03,
        HouseChallenge04,
        HouseChallenge05,
        HouseChallenge06,
        HouseChallenge07
    };

    private readonly HashSet<string> completedChallenges = new HashSet<string>();
    private readonly HashSet<string> validChallengeLookup = new HashSet<string>(ValidChallengeIds);

    private bool allChallengesEventRaised;

    public static ChallengeProgressManager Instance { get; private set; }

    public event Action<string, int, int> OnChallengeCompleted;
    public event Action<int, int> OnProgressChanged;
    public event Action OnAllChallengesCompleted;

    public int CompletedCount => completedChallenges.Count;
    public int TotalChallenges => ValidChallengeIds.Length;
    public bool AreAllChallengesCompleted => CompletedCount >= TotalChallenges;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceBeforeSceneLoad()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(nameof(ChallengeProgressManager));
        managerObject.AddComponent<ChallengeProgressManager>();
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetEditorChallengeProgress()
    {
        ResetStoredProgress();
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
        allChallengesEventRaised = AreAllChallengesCompleted;
    }

    public bool CompleteChallenge(string challengeId)
    {
        if (!IsKnownChallengeId(challengeId))
        {
            Debug.LogError($"ChallengeProgressManager: ID de reto invalido '{challengeId}'.", this);
            return false;
        }

        if (!completedChallenges.Add(challengeId))
        {
            return false;
        }

        SaveChallenge(challengeId, true);
        OnChallengeCompleted?.Invoke(challengeId, CompletedCount, TotalChallenges);
        OnProgressChanged?.Invoke(CompletedCount, TotalChallenges);
        GameEvents.RaiseMessageRequested($"Reto completado. Progreso: {CompletedCount}/{TotalChallenges}");

        if (AreAllChallengesCompleted && !allChallengesEventRaised)
        {
            allChallengesEventRaised = true;
            OnAllChallengesCompleted?.Invoke();
            GameEvents.RaiseMessageRequested("Has completado los siete retos. El castillo central ha sido desbloqueado.");
        }

        return true;
    }

    public bool IsChallengeCompleted(string challengeId)
    {
        return IsKnownChallengeId(challengeId) && completedChallenges.Contains(challengeId);
    }

    public bool IsKnownChallengeId(string challengeId)
    {
        return !string.IsNullOrWhiteSpace(challengeId) && validChallengeLookup.Contains(challengeId);
    }

    public int GetRemainingCount()
    {
        return Mathf.Max(0, TotalChallenges - CompletedCount);
    }

    public void ResetProgress()
    {
        ClearRuntimeProgress();
        ClearPersistedProgress();
        PlayerPrefs.Save();
        OnProgressChanged?.Invoke(CompletedCount, TotalChallenges);
    }

    public static void ResetStoredProgress()
    {
        ClearPersistedProgress();
        PlayerPrefs.Save();

        if (Instance != null)
        {
            Instance.ClearRuntimeProgress();
            Instance.OnProgressChanged?.Invoke(Instance.CompletedCount, Instance.TotalChallenges);
        }
    }

    public static IReadOnlyList<string> GetValidChallengeIds()
    {
        return ValidChallengeIds;
    }

    private void LoadProgress()
    {
        completedChallenges.Clear();

        foreach (string challengeId in ValidChallengeIds)
        {
            if (PlayerPrefs.GetInt(GetPlayerPrefsKey(challengeId), 0) == 1)
            {
                completedChallenges.Add(challengeId);
            }
        }
    }

    private static void SaveChallenge(string challengeId, bool completed)
    {
        PlayerPrefs.SetInt(GetPlayerPrefsKey(challengeId), completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ClearRuntimeProgress()
    {
        completedChallenges.Clear();
        allChallengesEventRaised = false;
    }

    private static void ClearPersistedProgress()
    {
        foreach (string challengeId in ValidChallengeIds)
        {
            PlayerPrefs.DeleteKey(GetPlayerPrefsKey(challengeId));
        }
    }

    private static string GetPlayerPrefsKey(string challengeId)
    {
        return PlayerPrefsPrefix + challengeId;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
