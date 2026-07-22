using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SevenChallengesSceneBootstrap : MonoBehaviour
{
    private const string DemoSceneName = "Demo";
    private const string RuntimeRootName = "SevenChallengesRuntime";
    private const string FinalBattleSceneName = "_DemoScene";

    private static readonly (string HouseName, string ChallengeId, System.Type BridgeType)[] HouseBindings =
    {
        ("House", ChallengeProgressManager.HouseChallenge01, typeof(Challenge01GameBridge)),
        ("House (1)", ChallengeProgressManager.HouseChallenge02, typeof(Challenge02GameBridge)),
        ("House (2)", ChallengeProgressManager.HouseChallenge03, typeof(Challenge03GameBridge)),
        ("House (3)", ChallengeProgressManager.HouseChallenge04, typeof(Challenge04GameBridge)),
        ("House (4)", ChallengeProgressManager.HouseChallenge05, typeof(Challenge05GameBridge)),
        ("House (5)", ChallengeProgressManager.HouseChallenge06, typeof(Challenge06GameBridge)),
        ("House (6)", ChallengeProgressManager.HouseChallenge07, typeof(Challenge07GameBridge))
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHandler()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    private static void TryInstall(Scene scene)
    {
        if (scene.name != DemoSceneName || GameObject.Find(RuntimeRootName) != null)
        {
            return;
        }

        GameObject runtimeRoot = new GameObject(RuntimeRootName);
        runtimeRoot.AddComponent<SevenChallengesSceneBootstrap>();

        GameObject player = GameObject.Find("Player_Dog_Model");
        if (player == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: no se encontro Player_Dog_Model.");
            return;
        }

        PlayerControlLock controlLock = EnsurePlayerControlLock(player);
        List<MonoBehaviour> challengeBridges = ConfigureHouseChallenges(controlLock);
        ChallengeProgressHUD hud = ConfigureHud(runtimeRoot.transform);
        ConfigureTestPanel(challengeBridges);
        ConfigureCastle(controlLock);
        ConfigureInitialWolfTransition(runtimeRoot, player, controlLock, hud);
    }

    private static PlayerControlLock EnsurePlayerControlLock(GameObject player)
    {
        PlayerControlLock controlLock = player.GetComponent<PlayerControlLock>();
        if (controlLock == null)
        {
            controlLock = player.AddComponent<PlayerControlLock>();
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        Behaviour[] behavioursToDisable =
        {
            player.GetComponent<MouseBasicAttack>()
        };

        controlLock.Configure(playerController, behavioursToDisable, true);
        return controlLock;
    }

    private static List<MonoBehaviour> ConfigureHouseChallenges(PlayerControlLock controlLock)
    {
        List<MonoBehaviour> challengeBridges = new List<MonoBehaviour>();

        foreach ((string houseName, string challengeId, System.Type bridgeType) in HouseBindings)
        {
            GameObject house = GameObject.Find(houseName);
            if (house == null)
            {
                Debug.LogError($"SevenChallengesSceneBootstrap: no se encontro {houseName}.");
                continue;
            }

            MonoBehaviour bridge = house.GetComponent(bridgeType) as MonoBehaviour;
            if (bridge == null)
            {
                bridge = house.AddComponent(bridgeType) as MonoBehaviour;
            }

            GameObject triggerObject = EnsureChild(house.transform, "ChallengeTrigger");
            BoxCollider triggerCollider = triggerObject.GetComponent<BoxCollider>();
            if (triggerCollider == null)
            {
                triggerCollider = triggerObject.AddComponent<BoxCollider>();
            }

            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(2.5f, 2.5f, 2.5f);
            triggerCollider.center = new Vector3(0f, 1.25f, 0f);

            HouseChallengeTrigger trigger = triggerObject.GetComponent<HouseChallengeTrigger>();
            if (trigger == null)
            {
                trigger = triggerObject.AddComponent<HouseChallengeTrigger>();
            }

            trigger.Configure(challengeId, bridge, controlLock);
            challengeBridges.Add(bridge);
        }

        return challengeBridges;
    }

    private static void ConfigureCastle(PlayerControlLock controlLock)
    {
        GameObject castle = GameObject.Find("Castle");
        if (castle == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: no se encontro Castle.");
            return;
        }

        CastleUnlockController unlockController = castle.GetComponent<CastleUnlockController>();
        if (unlockController == null)
        {
            unlockController = castle.AddComponent<CastleUnlockController>();
        }

        GameObject triggerObject = EnsureChild(castle.transform, "FinalBattleTrigger");
        BoxCollider triggerCollider = triggerObject.GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = triggerObject.AddComponent<BoxCollider>();
        }

        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(4f, 4f, 3f);
        triggerCollider.center = new Vector3(0f, 2f, 0f);

        FinalCastleTrigger finalTrigger = triggerObject.GetComponent<FinalCastleTrigger>();
        if (finalTrigger == null)
        {
            finalTrigger = triggerObject.AddComponent<FinalCastleTrigger>();
        }

        finalTrigger.Configure(FinalBattleSceneName, controlLock, unlockController);
    }

    private static void ConfigureInitialWolfTransition(GameObject runtimeRoot, GameObject player, PlayerControlLock controlLock, ChallengeProgressHUD hud)
    {
        GameObject wolf = GameObject.Find("Enemy_Wolf_Model");
        if (wolf == null)
        {
            Debug.LogWarning("SevenChallengesSceneBootstrap: no se encontro Enemy_Wolf_Model.");
            return;
        }

        EnemyRoleMarker roleMarker = wolf.GetComponent<EnemyRoleMarker>();
        if (roleMarker == null)
        {
            roleMarker = wolf.AddComponent<EnemyRoleMarker>();
        }

        roleMarker.Configure(EnemyRole.InitialWolf);

        Transform cityEntryPoint = ResolveCityEntryPoint(runtimeRoot.transform, player.transform);
        if (cityEntryPoint == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: no se pudo resolver CityEntryPoint. La transicion del lobo inicial no podra teletransportar.");
            return;
        }

        InitialWolfVictoryTransition transition = runtimeRoot.GetComponent<InitialWolfVictoryTransition>();
        if (transition == null)
        {
            transition = runtimeRoot.AddComponent<InitialWolfVictoryTransition>();
        }

        transition.Configure(
            wolf.GetComponent<EnemyHealth>(),
            player.transform,
            player.GetComponent<CharacterController>(),
            player.GetComponent<Rigidbody>(),
            controlLock,
            cityEntryPoint,
            hud,
            hud != null ? hud.gameObject : null);
    }

    private static Transform ResolveCityEntryPoint(Transform runtimeRoot, Transform player)
    {
        GameObject existingPoint = GameObject.Find("CityEntryPoint");
        if (existingPoint != null)
        {
            return existingPoint.transform;
        }

        if (!TryCalculateCityEntryPose(player, out Vector3 position, out Quaternion rotation))
        {
            return null;
        }

        GameObject generatedPoint = new GameObject("CityEntryPoint");
        generatedPoint.transform.SetParent(runtimeRoot, false);
        generatedPoint.transform.SetPositionAndRotation(position, rotation);

        Debug.LogWarning("SevenChallengesSceneBootstrap: no habia CityEntryPoint en Demo; se creo uno temporal en runtime usando el centro de las casas.");
        return generatedPoint.transform;
    }

    private static bool TryCalculateCityEntryPose(Transform player, out Vector3 position, out Quaternion rotation)
    {
        Vector3 accumulatedPosition = Vector3.zero;
        int foundHouses = 0;

        foreach (var binding in HouseBindings)
        {
            GameObject house = GameObject.Find(binding.HouseName);
            if (house == null)
            {
                continue;
            }

            accumulatedPosition += house.transform.position;
            foundHouses++;
        }

        if (foundHouses == 0)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        position = accumulatedPosition / foundHouses;
        if (player != null)
        {
            position.y = player.position.y;
        }

        GameObject castle = GameObject.Find("Castle");
        Vector3 forward = castle != null ? castle.transform.position - position : Vector3.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = player != null ? player.forward : Vector3.forward;
        }

        rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        return true;
    }

    private static ChallengeProgressHUD ConfigureHud(Transform runtimeRoot)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("SevenChallengesSceneBootstrap: no hay Canvas para crear HUD de retos.");
            return null;
        }

        GameObject hudObject = EnsureChild(canvas.transform, "ChallengeProgressHUD");
        RectTransform rectTransform = EnsureRectTransform(hudObject);
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(24f, -24f);
        rectTransform.sizeDelta = new Vector2(360f, 48f);

        TMP_Text text = hudObject.GetComponent<TMP_Text>();
        if (text == null)
        {
            text = hudObject.AddComponent<TextMeshProUGUI>();
        }

        text.fontSize = 24f;
        text.alignment = TextAlignmentOptions.Left;

        ChallengeProgressHUD hud = hudObject.GetComponent<ChallengeProgressHUD>();
        if (hud == null)
        {
            hud = hudObject.AddComponent<ChallengeProgressHUD>();
        }

        hud.Configure(text, hudObject, true);
        hudObject.transform.SetParent(canvas.transform, false);
        return hud;
    }

    private static void ConfigureTestPanel(List<MonoBehaviour> challengeBridges)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        GameObject host = EnsureChild(canvas.transform, "ChallengeTestPanelController");
        ChallengeTestPanel panel = host.GetComponent<ChallengeTestPanel>();
        if (panel == null)
        {
            panel = host.AddComponent<ChallengeTestPanel>();
        }

        GameObject panelRoot = EnsureChild(host.transform, "ChallengeTestPanel");
        RectTransform panelRect = EnsureRectTransform(panelRoot);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(460f, 280f);

        Image background = panelRoot.GetComponent<Image>();
        if (background == null)
        {
            background = panelRoot.AddComponent<Image>();
        }

        background.color = new Color(0f, 0f, 0f, 0.82f);

        TMP_Text titleText = EnsureText(panelRoot.transform, "ChallengeId", new Vector2(0f, 85f), new Vector2(400f, 48f), 26f);
        Button winButton = EnsureButton(panelRoot.transform, "BtnSimularVictoria", "Simular victoria", new Vector2(0f, 20f));
        Button loseButton = EnsureButton(panelRoot.transform, "BtnSimularDerrota", "Simular derrota", new Vector2(0f, -45f));
        Button cancelButton = EnsureButton(panelRoot.transform, "BtnCancelar", "Cancelar", new Vector2(0f, -110f));

        winButton.onClick.RemoveAllListeners();
        loseButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        winButton.onClick.AddListener(panel.SimulateVictory);
        loseButton.onClick.AddListener(panel.SimulateDefeat);
        cancelButton.onClick.AddListener(panel.CancelChallenge);

        panel.Configure(panelRoot, titleText, challengeBridges.ToArray());
    }

    private static Button EnsureButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = EnsureChild(parent, name);
        RectTransform rectTransform = EnsureRectTransform(buttonObject);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(280f, 48f);

        Image image = buttonObject.GetComponent<Image>();
        if (image == null)
        {
            image = buttonObject.AddComponent<Image>();
        }

        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        TMP_Text text = EnsureText(buttonObject.transform, "Text", Vector2.zero, new Vector2(260f, 42f), 20f);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static TMP_Text EnsureText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float fontSize)
    {
        GameObject textObject = EnsureChild(parent, name);
        RectTransform rectTransform = EnsureRectTransform(textObject);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        if (text == null)
        {
            text = textObject.AddComponent<TextMeshProUGUI>();
        }

        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.text = string.Empty;
        return text;
    }

    private static GameObject EnsureChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        return child;
    }

    private static RectTransform EnsureRectTransform(GameObject gameObject)
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            return rectTransform;
        }

        return gameObject.AddComponent<RectTransform>();
    }
}
