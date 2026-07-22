#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class Challenge07QuickSetup
{
    private const string RootMenu = "Tools/CastleHounds/Challenge 07/";
    private const string ArtFolder = "Assets/Project/Script/Challenges/Games/Challenge07/Art/Map";
    private const string PrefabPath = "Assets/Project/Script/Challenges/Games/Challenge07/Prefabs/Challenge07UI.prefab";

    [InitializeOnLoadMethod]
    private static void SchedulePrefabBuild()
    {
        EditorApplication.delayCall += EnsurePrefabExists;
    }

    [DidReloadScripts]
    private static void BuildPrefabAfterReload()
    {
        EditorApplication.delayCall += EnsurePrefabExists;
    }

    private static void EnsurePrefabExists()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return;
        Sprite map = FindCompletedMap();
        Sprite[] six = LoadSixSprites();
        Sprite[] nine = LoadNineSprites();
        if (six.Any(sprite => sprite == null)) return;
        if (map == null) map = LoadSpriteByPrefix("MapPieces6") ?? six[0];
        AudioClip ambient = FindAudio("ambiente");
        AudioClip pickup = FindAudio("paper");
        AudioClip correct = FindAudio("correcta");
        AudioClip wrong = FindAudio("error");
        AudioClip hint = FindAudio("iluminar");
        AudioClip button = FindAudio("SonidoBoton");
        AudioClip victory = FindAudio("victoria");
        BuildPrefab(map, six, nine, ambient, pickup, correct, wrong, hint, button, victory);
        Debug.Log("Challenge07QuickSetup: Challenge07UI.prefab generado automáticamente y listo para instalar.");
    }

    public static void BuildPrefabAssetOnly()
    {
        EnsurePrefabExists();
    }

    [MenuItem(RootMenu + "INSTALL IN SELECTED HOUSE 07")]
    public static void BuildCompleteGame()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Challenge 07", "Selecciona House (6) en la Hierarchy.", "Aceptar");
            return;
        }

        Challenge07GameBridge bridge = selected.GetComponent<Challenge07GameBridge>() ?? selected.GetComponentInChildren<Challenge07GameBridge>(true);
        if (bridge == null) bridge = Undo.AddComponent<Challenge07GameBridge>(selected);

        if (bridge.PuzzleController != null)
        {
            EditorUtility.DisplayDialog("Challenge 07", "El Juego 7 ya está instalado en este objeto. Usa VALIDATE GAME.", "Aceptar");
            Selection.activeGameObject = bridge.PuzzleController.gameObject;
            return;
        }

        Sprite mapComplete = FindCompletedMap();
        Sprite[] sixSprites = LoadSixSprites();
        Sprite[] nineSprites = LoadNineSprites();
        Sprite mapPieces6 = LoadSpriteByPrefix("MapPieces6");
        Sprite mapPieces9 = LoadSpriteByPrefix("MapPieces9");
        if (sixSprites.Any(sprite => sprite == null))
        {
            EditorUtility.DisplayDialog("Challenge 07", "Faltan imágenes obligatorias. Ejecuta VALIDATE GAME y revisa la consola.", "Aceptar");
            return;
        }
        if (mapComplete == null) mapComplete = mapPieces6 ?? sixSprites[0];

        AudioClip ambient = FindAudio("ambiente");
        AudioClip pickup = FindAudio("paper");
        AudioClip correct = FindAudio("correcta");
        AudioClip wrong = FindAudio("error");
        AudioClip hint = FindAudio("iluminar");
        AudioClip button = FindAudio("SonidoBoton");
        AudioClip victory = FindAudio("victoria");

        GameObject prefab = BuildPrefab(mapComplete, sixSprites, nineSprites, ambient, pickup, correct, wrong, hint, button, victory);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Challenge 07", "No se pudo crear Challenge07UI.prefab.", "Aceptar");
            return;
        }

        Transform canvasParent = EnsureCanvas(selected).transform;
        EnsureEventSystem();
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, canvasParent) as GameObject;
        if (instance == null) return;
        Undo.RegisterCreatedObjectUndo(instance, "Install Challenge 07");
        instance.name = "Challenge07UI";
        Challenge07PuzzleController controller = instance.GetComponent<Challenge07PuzzleController>();
        controller.SetGameBridge(bridge);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(bridge);
        PrefabUtility.RecordPrefabInstancePropertyModifications(bridge);
        PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        EditorSceneManager.MarkSceneDirty(selected.scene);
        EditorSceneManager.SaveScene(selected.scene);
        Selection.activeGameObject = instance;

        string references = $"MapPieces6: {(mapPieces6 != null ? "OK" : "faltante")} | MapPieces9: {(mapPieces9 != null ? "OK" : "faltante")}";
        Debug.Log($"Challenge07 instalado. 6 piezas: {string.Join(", ", sixSprites.Select(s => s.name))}. 9 piezas: {string.Join(", ", nineSprites.Select(s => s.name))}. {references}", instance);
        EditorUtility.DisplayDialog("Challenge 07 listo", "Juego completo instalado y escena guardada.\n\nModo predeterminado: 6 piezas\nPrefab: " + PrefabPath + "\n\nPulsa Play para probar.", "Aceptar");
    }

    [MenuItem(RootMenu + "BUILD DEMO IN SELECTED CANVAS")]
    public static void BuildDemoInSelectedCanvas()
    {
        Canvas canvas = null;
        if (Selection.activeGameObject != null)
        {
            canvas = Selection.activeGameObject.GetComponent<Canvas>() ?? Selection.activeGameObject.GetComponentInParent<Canvas>();
        }
        if (canvas == null)
        {
            canvas = Object.FindAnyObjectByType<Canvas>();
        }
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Challenge 07 Demo", "Selecciona o crea un Canvas en la Hierarchy.", "Aceptar");
            return;
        }
        if (canvas.transform.Find("Challenge07UI") != null)
        {
            EditorUtility.DisplayDialog("Challenge 07 Demo", "Este Canvas ya contiene Challenge07UI.", "Aceptar");
            return;
        }

        Sprite[] six = LoadSixSprites();
        if (six.Any(sprite => sprite == null))
        {
            EditorUtility.DisplayDialog("Challenge 07 Demo", "No fue posible cargar las seis piezas. Revisa las rutas impresas en Console.", "Aceptar");
            return;
        }
        Sprite[] nine = LoadNineSprites();
        Sprite map = FindCompletedMap() ?? LoadSpriteByPrefix("MapPieces6") ?? six[0];
        AudioClip ambient = FindAudio("ambiente");
        AudioClip pickup = FindAudio("paper");
        AudioClip correct = FindAudio("correcta");
        AudioClip wrong = FindAudio("error");
        AudioClip hint = FindAudio("iluminar");
        AudioClip button = FindAudio("SonidoBoton");
        AudioClip victory = FindAudio("victoria");
        GameObject prefab = BuildPrefab(map, six, nine, ambient, pickup, correct, wrong, hint, button, victory);
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, canvas.transform) as GameObject;
        if (instance == null) return;
        Undo.RegisterCreatedObjectUndo(instance, "Build Challenge 07 Demo");
        instance.name = "Challenge07UI";
        Challenge07PuzzleController controller = instance.GetComponent<Challenge07PuzzleController>();
        controller.ConfigureStandaloneDemo(true);
        instance.SetActive(true);
        EditorUtility.SetDirty(controller);
        PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        EnsureEventSystem();
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveScene(canvas.gameObject.scene);
        Selection.activeGameObject = instance;
        EditorUtility.DisplayDialog("Challenge 07 Demo", "Demo instalada y escena guardada. Pulsa Play.", "Aceptar");
    }

    [MenuItem(RootMenu + "VALIDATE HOUSE")]
    public static void ValidateGame()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MenuPrincipal")
        {
            Debug.Log("Escena actual es MenuPrincipal (Demo). Validando como Demo...");
            ValidateDemo();
            return;
        }

        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();

        // Find House (6) automatically
        GameObject house = FindHouse07();
        Challenge07GameBridge bridge = null;

        if (house != null)
        {
            // Remove incorrect Challenge07GameBridge on other objects
            Challenge07GameBridge[] allBridges = Object.FindObjectsByType<Challenge07GameBridge>(FindObjectsInactive.Include);
            foreach (var b in allBridges)
            {
                if (b.gameObject != house)
                {
                    Debug.LogWarning($"Removiendo Challenge07GameBridge incorrecto de {b.gameObject.name}", b.gameObject);
                    Object.DestroyImmediate(b);
                }
            }

            bridge = house.GetComponent<Challenge07GameBridge>();
            if (bridge == null)
            {
                bridge = Undo.AddComponent<Challenge07GameBridge>(house);
                Debug.Log("Challenge07GameBridge añadido automáticamente a House (6).", house);
            }
        }
        else
        {
            errors.Add("No se encontró el objeto de la Casa 7 (House (6)) en la escena.");
        }

        Challenge07PuzzleController controller = Object.FindAnyObjectByType<Challenge07PuzzleController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            errors.Add("Falta Challenge07PuzzleController en la escena.");
        }

        if (bridge != null && controller != null)
        {
            if (controller.GetComponentInParent<Canvas>() == null)
            {
                errors.Add("Challenge07PuzzleController no está dentro de un Canvas.");
            }
            controller.SetGameBridge(bridge);
            controller.ConfigureStandaloneDemo(false);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(bridge);
        }

        if (FindCompletedMap() == null) warnings.Add("No se identificó MapComplete; se usará la mejor referencia disponible.");
        if (LoadSpriteByPrefix("MapPieces6") == null) warnings.Add("No se encontró MapPieces6; no impide jugar.");
        if (LoadSpriteByPrefix("MapPieces9") == null) warnings.Add("No se encontró MapPieces9; no impide jugar.");
        if (LoadSixSprites().Any(sprite => sprite == null)) errors.Add("Faltan piezas Mapa_01 a Mapa_06.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) errors.Add("Falta Challenge07UI.prefab.");

        if (controller != null)
        {
            Transform root = controller.transform;
            int six = root.Find("MainFrame/GamePanel/Content/RemainingPiecesPanel/Pieces6")?.GetComponentsInChildren<MapPuzzlePiece>(true).Length ?? 0;
            int targets6 = root.Find("MainFrame/GamePanel/Content/PuzzleBoard/Targets6")?.childCount ?? 0;
            if (six != 6) errors.Add($"Piezas modo 6: {six}/6.");
            if (targets6 != 6) errors.Add($"Targets modo 6: {targets6}/6.");
            if (root.Find("MainFrame/GamePanel/BottomBar/HintButton")?.GetComponent<Button>() == null) errors.Add("Falta botón PISTA.");
            if (root.Find("MainFrame/GamePanel/BottomBar/RestartButton")?.GetComponent<Button>() == null) errors.Add("Falta botón REINICIAR.");
            if (root.Find("MainFrame/VictoryPanel/ContinueButton")?.GetComponent<Button>() == null) errors.Add("Falta botón CONTINUAR.");
            if (root.Find("MainFrame/GamePanel/BottomBar/ProgressText")?.GetComponent<TMP_Text>() == null) errors.Add("Falta ProgressText.");
        }

        if (Object.FindAnyObjectByType<EventSystem>() == null) warnings.Add("No hay EventSystem.");

        string report = errors.Count == 0 ? "INSTALACIÓN DE CASA 7 TOTALMENTE VÁLIDA Y CONECTADA" : "ERRORES:\n- " + string.Join("\n- ", errors);
        if (warnings.Count > 0) report += "\n\nADVERTENCIAS:\n- " + string.Join("\n- ", warnings);
        Debug.Log("Challenge07 VALIDATE HOUSE\n" + report, controller);
        EditorUtility.DisplayDialog("Challenge 07 — VALIDATE HOUSE", report, "Aceptar");
    }

    [MenuItem(RootMenu + "VALIDATE DEMO")]
    public static void ValidateDemo()
    {
        List<string> errors = new List<string>();
        Canvas canvas = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Canvas>() ?? Selection.activeGameObject.GetComponentInParent<Canvas>() : null;
        if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();
        Challenge07PuzzleController controller = canvas != null ? canvas.GetComponentInChildren<Challenge07PuzzleController>(true) : null;
        if (controller == null) controller = Object.FindAnyObjectByType<Challenge07PuzzleController>(FindObjectsInactive.Include);

        if (canvas == null) errors.Add("Falta Canvas en la escena.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) errors.Add("Falta Challenge07UI.prefab.");
        if (controller == null) errors.Add("Falta Challenge07PuzzleController.");
        if (LoadSixSprites().Any(sprite => sprite == null)) errors.Add("No se cargaron las seis piezas.");
        if (controller != null)
        {
            Transform root = controller.transform;
            int pieces = root.Find("MainFrame/GamePanel/Content/RemainingPiecesPanel/Pieces6")?.GetComponentsInChildren<MapPuzzlePiece>(true).Length ?? 0;
            int targets = root.Find("MainFrame/GamePanel/Content/PuzzleBoard/Targets6")?.childCount ?? 0;
            if (pieces != 6) errors.Add($"Piezas: {pieces}/6.");
            if (targets != 6) errors.Add($"Targets: {targets}/6.");
            if (root.Find("MainFrame/GamePanel/BottomBar/HintButton") == null) errors.Add("Falta PISTA.");
            if (root.Find("MainFrame/GamePanel/BottomBar/RestartButton") == null) errors.Add("Falta REINICIAR.");
            if (root.Find("MainFrame/VictoryPanel/ContinueButton") == null) errors.Add("Falta CONTINUAR.");
        }
        if (Object.FindAnyObjectByType<EventSystem>() == null) errors.Add("Falta EventSystem.");
        string report = errors.Count == 0 ? "DEMO VÁLIDA Y JUGABLE" : "ERRORES:\n- " + string.Join("\n- ", errors);
        Debug.Log("Challenge07 VALIDATE DEMO\n" + report, controller);
        EditorUtility.DisplayDialog("Challenge 07 — VALIDATE DEMO", report, "Aceptar");
    }

    private static GameObject FindHouse07()
    {
        GameObject house = GameObject.Find("House (6)");
        if (house != null) return house;

        Challenge07GameBridge bridge = Object.FindAnyObjectByType<Challenge07GameBridge>(FindObjectsInactive.Include);
        if (bridge != null) return bridge.gameObject;

        var allObjs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allObjs)
        {
            if (go.name == "House (6)" && !EditorUtility.IsPersistent(go))
            {
                return go;
            }
        }
        return null;
    }

    private static GameObject BuildPrefab(Sprite map, Sprite[] six, Sprite[] nine, AudioClip ambient, AudioClip pickup, AudioClip correct, AudioClip wrong, AudioClip hint, AudioClip button, AudioClip victory)
    {
        GameObject temporaryCanvas = new GameObject("Challenge07PrefabBuilder", typeof(RectTransform), typeof(Canvas));
        try
        {
            Challenge07PuzzleController controller = Challenge07UIFactory.Create(temporaryCanvas.transform, null, map, six, nine, ambient, pickup, correct, wrong, hint, button, button, victory);
            if (controller == null) return null;
            controller.gameObject.SetActive(false);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(controller.gameObject, PrefabPath);
            AssetDatabase.SaveAssets();
            return prefab;
        }
        finally
        {
            Object.DestroyImmediate(temporaryCanvas);
        }
    }

    private static Canvas EnsureCanvas(GameObject selected)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null) return canvas;
        GameObject canvasObject = new GameObject("ChallengesCanvas", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Challenges Canvas");
        canvasObject.transform.SetParent(selected.transform, false);
        canvas = Undo.AddComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = Undo.AddComponent<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Undo.AddComponent<GraphicRaycaster>(canvasObject);
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem != null) return;
        GameObject eventObject = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(eventObject, "Create EventSystem");
        eventSystem = Undo.AddComponent<EventSystem>(eventObject);
        Undo.AddComponent<InputSystemUIInputModule>(eventObject);
    }

    private static Sprite FindCompletedMap()
    {
        string preferredPath = FindCandidatePaths().FirstOrDefault(path =>
        {
            string name = NormalizeAssetName(path);
            return name.Contains("mapcomplete") || name.Contains("mapacompleto") || name.Contains("mp2");
        });
        Sprite preferred = LoadSpriteAtPath(preferredPath);
        if (preferred != null)
        {
            Debug.Log($"Challenge07 mapa completo: {preferredPath}");
            return preferred;
        }
        string bestPath = FindCandidatePaths()
            .Where(path => path.Replace('\\', '/').StartsWith(ArtFolder, System.StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(path =>
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                return texture != null ? (long)texture.width * texture.height : 0L;
            })
            .FirstOrDefault();
        return LoadSpriteAtPath(bestPath);
    }

    private static Sprite[] LoadSixSprites()
    {
        Sprite[] sprites = new Sprite[6];
        List<string> paths = FindCandidatePaths().ToList();
        for (int i = 0; i < sprites.Length; i++)
        {
            string prefix = $"mapa{i + 1:00}";
            string path = paths.FirstOrDefault(candidate => NormalizeAssetName(candidate).StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase));
            sprites[i] = LoadSpriteAtPath(path);
            if (!string.IsNullOrEmpty(path)) Debug.Log($"Challenge07 pieza {i + 1}: {path}");
        }
        return sprites;
    }

    private static Sprite[] LoadNineSprites()
    {
        Sprite[] sprites = new Sprite[9];
        foreach (string path in FindCandidatePaths())
        {
            string normalized = NormalizeAssetName(path);
            const string prefix = "puzzle9piece";
            if (!normalized.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) continue;
            string digits = new string(normalized.Substring(prefix.Length).TakeWhile(char.IsDigit).ToArray());
            if (!int.TryParse(digits, out int number) || number < 1 || number > 9) continue;
            sprites[number - 1] = LoadSpriteAtPath(path);
            Debug.Log($"Challenge07 pieza 9/{number}: {path}");
        }
        return sprites;
    }

    private static Sprite LoadSpriteByPrefix(string prefix)
    {
        string normalizedPrefix = NormalizeText(prefix);
        string path = FindCandidatePaths().FirstOrDefault(candidate =>
            NormalizeAssetName(candidate).StartsWith(normalizedPrefix, System.StringComparison.OrdinalIgnoreCase));
        return LoadSpriteAtPath(path);
    }

    private static IEnumerable<string> FindCandidatePaths()
    {
        return AssetDatabase.FindAssets("t:Texture2D")
            .Concat(AssetDatabase.FindAssets("t:Sprite"))
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrEmpty(path) && path.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase))
            .Distinct(System.StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeAssetName(string path)
    {
        return NormalizeText(System.IO.Path.GetFileNameWithoutExtension(path));
    }

    private static string NormalizeText(string value)
    {
        return new string((value ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }

    private static Sprite LoadSpriteAtPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        ConfigureTexture(path);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ConfigureTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || !importer.alphaIsTransparency || importer.mipmapEnabled || settings.spriteMeshType != SpriteMeshType.FullRect || importer.textureCompression != TextureImporterCompression.Uncompressed;
        if (!changed) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static AudioClip FindAudio(string name)
    {
        string guid = AssetDatabase.FindAssets(name + " t:AudioClip").FirstOrDefault();
        return string.IsNullOrEmpty(guid) ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
    }
}
#endif
