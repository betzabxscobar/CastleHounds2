using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class SevenChallengesSceneBootstrap : MonoBehaviour
{
    private const string DemoSceneName = "Demo";
    private const string RuntimeRootName = "SevenChallengesRuntime";
    private const string FinalBattleSceneName = "_DemoScene";
    private const string ChallengesCanvasName = "ChallengesCanvas";
    private const string RuneMemoryPanelName = "RuneMemoryPanel";

    private const string RuneMemoryPanelPrefabPath = "Assets/Project/Script/Challenges/Games/Challenge01/Prefabs/RuneMemoryPanel.prefab";
    private const string RuneSwordSpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/Runes/espada.png";
    private const string RuneShieldSpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/Runes/escudo.png";
    private const string RuneWolfSpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/Runes/lobo.png";
    private const string RuneCrownSpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/Runes/corona.png";
    private const string RuneFrameSpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/UI/marco.png";
    private const string RuneStartSpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/UI/comenzar.png";
    private const string RuneRetrySpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/UI/reintentar.png";
    private const string RuneExitSpritePath = "Assets/Project/Script/Challenges/Games/Challenge01/Art/UI/salir.png";
    private const string RuneAmbientAudioPath = "Assets/Project/Script/Challenges/Games/Challenge01/Audio/ambiente.mp3";
    private const string RuneHighlightAudioPath = "Assets/Project/Script/Challenges/Games/Challenge01/Audio/iluminar.mp3";
    private const string RuneCorrectAudioPath = "Assets/Project/Script/Challenges/Games/Challenge01/Audio/correcta.mp3";
    private const string RuneErrorAudioPath = "Assets/Project/Script/Challenges/Games/Challenge01/Audio/error.mp3";
    private const string RuneVictoryAudioPath = "Assets/Project/Script/Challenges/Games/Challenge01/Audio/victoria.mp3";

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
        Canvas challengesCanvas = EnsureChallengesCanvas();
        EnsureEventSystem();
        List<MonoBehaviour> challengeBridges = ConfigureHouseChallenges(controlLock, challengesCanvas != null ? challengesCanvas.transform : runtimeRoot.transform);
        ConfigureHud(runtimeRoot.transform);
        ConfigureTestPanel(challengeBridges);
        ConfigureCastle(controlLock);
        DisableInitialSceneWolf();
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

    private static List<MonoBehaviour> ConfigureHouseChallenges(PlayerControlLock controlLock, Transform uiRoot)
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

            if (bridge is RuneMemoryGameController runeMemoryGame)
            {
                ConfigureRuneMemoryGame(runeMemoryGame, uiRoot);
            }
            else
            {
                challengeBridges.Add(bridge);
            }
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

    private static void DisableInitialSceneWolf()
    {
        GameObject wolf = GameObject.Find("Enemy_Wolf_Model");
        if (wolf == null)
        {
            return;
        }

        wolf.SetActive(false);
        Debug.Log("SevenChallengesSceneBootstrap: Enemy_Wolf_Model desactivado en Demo. El lobo aparecera al cargar la escena de batalla desde la puerta.");
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

    private static Canvas EnsureChallengesCanvas()
    {
        GameObject existingCanvasObject = GameObject.Find(ChallengesCanvasName);
        Canvas canvas = existingCanvasObject != null ? existingCanvasObject.GetComponent<Canvas>() : null;

        if (canvas == null)
        {
            canvas = Object.FindAnyObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(ChallengesCanvasName);
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.name = ChallengesCanvasName;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        StandaloneInputModule standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneInputModule != null)
        {
            standaloneInputModule.enabled = false;
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static void ConfigureRuneMemoryGame(RuneMemoryGameController gameController, Transform uiRoot)
    {
        if (uiRoot == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: falta uiRoot para Memoria de Runas.");
            return;
        }

        RuneMemoryPanel panel = FindOrCreateRuneMemoryPanel(uiRoot);
        if (panel == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: no se pudo crear RuneMemoryPanel.");
            return;
        }

        AudioSource[] audioSources = panel.GetComponents<AudioSource>();
        AudioSource sfxSource = audioSources.Length > 0 ? audioSources[0] : panel.gameObject.AddComponent<AudioSource>();
        ConfigureUiAudioSource(sfxSource, false);

        AudioSource ambientSource = audioSources.Length > 1 ? audioSources[1] : panel.gameObject.AddComponent<AudioSource>();
        ConfigureUiAudioSource(ambientSource, true);

        gameController.ConfigureRuntime(
            panel,
            panel.RuneButtons,
            sfxSource,
            ambientSource,
            LoadAsset<AudioClip>(RuneAmbientAudioPath),
            LoadAsset<AudioClip>(RuneHighlightAudioPath),
            LoadAsset<AudioClip>(RuneCorrectAudioPath),
            LoadAsset<AudioClip>(RuneErrorAudioPath),
            LoadAsset<AudioClip>(RuneVictoryAudioPath));
    }

    private static RuneMemoryPanel FindOrCreateRuneMemoryPanel(Transform uiRoot)
    {
        Transform existingPanel = uiRoot.Find(RuneMemoryPanelName);
        if (existingPanel != null)
        {
            return existingPanel.GetComponent<RuneMemoryPanel>();
        }

        GameObject panelPrefab = LoadAsset<GameObject>(RuneMemoryPanelPrefabPath, false);
        if (panelPrefab != null)
        {
            GameObject panelObject = Object.Instantiate(panelPrefab, uiRoot);
            panelObject.name = RuneMemoryPanelName;
            return panelObject.GetComponent<RuneMemoryPanel>();
        }

        return RuneMemoryPanel.CreateDefault(
            uiRoot,
            LoadAsset<Sprite>(RuneSwordSpritePath),
            LoadAsset<Sprite>(RuneShieldSpritePath),
            LoadAsset<Sprite>(RuneWolfSpritePath),
            LoadAsset<Sprite>(RuneCrownSpritePath),
            LoadAsset<Sprite>(RuneFrameSpritePath),
            LoadAsset<Sprite>(RuneStartSpritePath),
            LoadAsset<Sprite>(RuneRetrySpritePath),
            LoadAsset<Sprite>(RuneExitSpritePath));
    }

    private static void ConfigureUiAudioSource(AudioSource audioSource, bool loop)
    {
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.spatialBlend = 0f;
    }

    private static T LoadAsset<T>(string assetPath, bool logMissing = true) where T : Object
    {
#if UNITY_EDITOR
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset == null && logMissing)
        {
            Debug.LogError($"SevenChallengesSceneBootstrap: no se pudo cargar asset '{assetPath}'.");
        }

        return asset;
#else
        Debug.LogError($"SevenChallengesSceneBootstrap: carga runtime no configurada para '{assetPath}'.");
        return null;
#endif
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
