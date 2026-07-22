using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SevenChallengesSceneBootstrap : MonoBehaviour
{
    private const string DemoSceneName = "Demo";
    private const string RuntimeRootName = "SevenChallengesRuntime";
    private const string FinalBattleSceneName = "_DemoScene";
    private const string ChallengesCanvasName = "ChallengesCanvas";
    private const string RuneMemoryPanelName = "RuneMemoryPanel";
    private const string RuneMemoryPanelResourcePath = "Challenges/Challenge01/RuneMemoryPanel";
    private const string ChestCombinationPanelName = "ChestCombinationPanel";
    private const string ChestCombinationPanelResourcePath = "Challenges/Challenge02/ChestCombinationPanel";

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
            else if (bridge is ChestCombinationGameController chestCombinationGame)
            {
                ConfigureChestCombinationGame(chestCombinationGame, uiRoot);
            }
            else if (bridge is Challenge07GameBridge challenge07Game)
            {
                ConfigureChallenge07Game(challenge07Game, uiRoot);
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

        EnemyHealth wolfHealth = wolf.GetComponent<EnemyHealth>();
        GameObject player = GameObject.Find("Player_Dog_Model");
        MouseBasicAttack mouseAttack = player != null ? player.GetComponent<MouseBasicAttack>() : null;
        if (mouseAttack != null)
        {
            mouseAttack.ClearEnemyTargetIfMatches(wolfHealth);
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
            Debug.LogError("SevenChallengesSceneBootstrap: no se pudo cargar RuneMemoryPanel desde Resources. El reto 1 no se iniciara para evitar bloquear al jugador.");
            return;
        }

        AudioSource sfxSource = panel.SfxSource != null ? panel.SfxSource : panel.gameObject.AddComponent<AudioSource>();
        ConfigureUiAudioSource(sfxSource, false);

        AudioSource ambientSource = panel.AmbientSource != null ? panel.AmbientSource : panel.gameObject.AddComponent<AudioSource>();
        ConfigureUiAudioSource(ambientSource, true);
        ValidateRuneMemoryPanelAssets(panel);

        gameController.ConfigureRuntime(
            panel,
            panel.RuneButtons,
            sfxSource,
            ambientSource,
            panel.AmbientClip,
            panel.HighlightClip,
            panel.CorrectClip,
            panel.ErrorClip,
            panel.VictoryClip);
    }

    private static RuneMemoryPanel FindOrCreateRuneMemoryPanel(Transform uiRoot)
    {
        Transform existingPanel = uiRoot.Find(RuneMemoryPanelName);
        if (existingPanel != null)
        {
            return existingPanel.GetComponent<RuneMemoryPanel>();
        }

        GameObject panelPrefab = Resources.Load<GameObject>(RuneMemoryPanelResourcePath);
        if (panelPrefab == null)
        {
            Debug.LogError($"SevenChallengesSceneBootstrap: Resources.Load fallo para '{RuneMemoryPanelResourcePath}'.");
            return null;
        }

        GameObject panelObject = Object.Instantiate(panelPrefab, uiRoot);
        panelObject.name = RuneMemoryPanelName;
        return panelObject.GetComponent<RuneMemoryPanel>();
    }

    private static void ConfigureUiAudioSource(AudioSource audioSource, bool loop)
    {
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.spatialBlend = 0f;
    }

    private static void ValidateRuneMemoryPanelAssets(RuneMemoryPanel panel)
    {
        if (panel.RuneButtons == null || panel.RuneButtons.Length < 4)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: RuneMemoryPanel no tiene las cuatro runas serializadas.");
        }

        if (panel.SfxSource == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: RuneMemoryPanel no tiene SfxSource serializado.");
        }

        if (panel.AmbientSource == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: RuneMemoryPanel no tiene AmbientSource serializado.");
        }

        if (panel.AmbientClip == null || panel.HighlightClip == null || panel.CorrectClip == null || panel.ErrorClip == null || panel.VictoryClip == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: RuneMemoryPanel no tiene todos los AudioClip serializados.");
        }
    }

    private static void ConfigureChestCombinationGame(ChestCombinationGameController gameController, Transform uiRoot)
    {
        if (uiRoot == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: falta uiRoot para Cofre con combinacion.");
            return;
        }

        ChestCombinationPanel panel = FindOrCreateChestCombinationPanel(uiRoot);
        if (panel == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: no se pudo cargar ChestCombinationPanel desde Resources. El reto 2 no se iniciara para evitar bloquear al jugador.");
            return;
        }

        AudioSource sfxSource = panel.SfxSource != null ? panel.SfxSource : panel.gameObject.AddComponent<AudioSource>();
        ConfigureUiAudioSource(sfxSource, false);

        AudioSource ambienceSource = panel.AmbienceSource != null ? panel.AmbienceSource : panel.gameObject.AddComponent<AudioSource>();
        ConfigureUiAudioSource(ambienceSource, true);
        ValidateChestCombinationPanelAssets(panel);

        gameController.ConfigureRuntime(
            panel,
            sfxSource,
            ambienceSource,
            panel.AmbienceClip,
            panel.NumberPressClip,
            panel.CorrectClip,
            panel.IncorrectClip,
            panel.ChestOpeningClip,
            panel.VictoryClip);
    }

    private static ChestCombinationPanel FindOrCreateChestCombinationPanel(Transform uiRoot)
    {
        Transform existingPanel = uiRoot.Find(ChestCombinationPanelName);
        if (existingPanel != null)
        {
            return existingPanel.GetComponent<ChestCombinationPanel>();
        }

        GameObject panelPrefab = Resources.Load<GameObject>(ChestCombinationPanelResourcePath);
        if (panelPrefab == null)
        {
            Debug.LogError($"SevenChallengesSceneBootstrap: Resources.Load fallo para '{ChestCombinationPanelResourcePath}'.");
            return null;
        }

        GameObject panelObject = Object.Instantiate(panelPrefab, uiRoot);
        panelObject.name = ChestCombinationPanelName;
        return panelObject.GetComponent<ChestCombinationPanel>();
    }

    private static void ValidateChestCombinationPanelAssets(ChestCombinationPanel panel)
    {
        if (panel.SfxSource == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: ChestCombinationPanel no tiene SfxSource serializado.");
        }

        if (panel.AmbienceSource == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: ChestCombinationPanel no tiene AmbienceSource serializado.");
        }

        if (panel.AmbienceClip == null || panel.NumberPressClip == null || panel.CorrectClip == null || panel.IncorrectClip == null || panel.ChestOpeningClip == null || panel.VictoryClip == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: ChestCombinationPanel no tiene todos los AudioClip serializados.");
        }
    }

    private static void ConfigureChallenge07Game(Challenge07GameBridge gameController, Transform uiRoot)
    {
        if (uiRoot == null)
        {
            Debug.LogError("SevenChallengesSceneBootstrap: falta uiRoot para Challenge 07.");
            return;
        }

        Transform existingPanel = uiRoot.Find("Challenge07UI");
        Challenge07PuzzleController panel = null;
        if (existingPanel != null)
        {
            panel = existingPanel.GetComponent<Challenge07PuzzleController>();
        }

        if (panel == null)
        {
            panel = Object.FindAnyObjectByType<Challenge07PuzzleController>(FindObjectsInactive.Include);
        }

        if (panel == null)
        {
            string prefabPath = "Assets/Project/Script/Challenges/Games/Challenge07/Prefabs/Challenge07UI.prefab";
#if UNITY_EDITOR
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject panelObject = Object.Instantiate(prefab, uiRoot);
                panelObject.name = "Challenge07UI";
                panel = panelObject.GetComponent<Challenge07PuzzleController>();
            }
#endif
        }

        if (panel != null)
        {
            panel.SetGameBridge(gameController);
            panel.ConfigureStandaloneDemo(false);
            panel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("SevenChallengesSceneBootstrap: no se pudo encontrar o instanciar Challenge07UI.");
        }
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
