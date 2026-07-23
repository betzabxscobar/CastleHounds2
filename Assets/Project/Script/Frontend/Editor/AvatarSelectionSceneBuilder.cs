#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AvatarSelectionSceneBuilder
{
    private const string ScenePath = "Assets/Project/Scenes/Menus/Frontend/SeleccionAvatar.unity";
    private const string GeneratedPath = "Assets/Project/Scenes/Menus/Frontend/SeleccionAvatarGenerated";
    private static Material stone;
    private static Material darkStone;
    private static Material gold;
    private static Material blueGlow;
    private static Material redGlow;
    private static Material moon;

    [MenuItem("Castle Hounds/Reconstruir Seleccion Avatar 3D")]
    public static void Build()
    {
        Directory.CreateDirectory(GeneratedPath);
        CreateMaterials();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "SeleccionAvatar";

        GameObject environment = new GameObject("SeleccionAvatarEnvironment");
        BuildEnvironment(environment.transform);
        Camera camera = BuildCamera();
        BuildLighting(environment.transform);
        BuildUI(environment.transform);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.018f;
        RenderSettings.fogColor = new Color(0.018f, 0.027f, 0.045f);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.018f, 0.033f, 0.065f);
        RenderSettings.ambientEquatorColor = new Color(0.012f, 0.013f, 0.02f);
        RenderSettings.ambientGroundColor = new Color(0.004f, 0.003f, 0.003f);

        SelectionAtmosphere atmosphere = environment.AddComponent<SelectionAtmosphere>();
        SerializedObject atmosphereSO = new SerializedObject(atmosphere);
        atmosphereSO.FindProperty("platformRunes").objectReferenceValue =
            GameObject.Find("PlatformRunes").transform;
        Light[] torches = environment.GetComponentsInChildren<Light>();
        SerializedProperty lights = atmosphereSO.FindProperty("torchLights");
        lights.arraySize = torches.Length;
        for (int i = 0; i < torches.Length; i++) lights.GetArrayElementAtIndex(i).objectReferenceValue = torches[i];
        atmosphereSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        CaptureGameCamera(camera);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = camera.gameObject;
        Debug.Log($"Seleccion de avatar 3D reconstruida: {ScenePath}");
    }

    public static void BuildBatch()
    {
        Build();
        EditorApplication.Exit(0);
    }

    private static void CreateMaterials()
    {
        stone = MaterialAsset("M_Selection_Stone", new Color(0.11f, 0.105f, 0.11f), 0.05f, 0.48f);
        darkStone = MaterialAsset("M_Selection_DarkStone", new Color(0.012f, 0.014f, 0.019f), 0.02f, 0.48f);
        gold = MaterialAsset("M_Selection_AntiqueGold", new Color(0.64f, 0.34f, 0.085f), 0.72f, 0.38f,
            new Color(0.16f, 0.065f, 0.01f));
        blueGlow = MaterialAsset("M_Selection_BlueSpectral", new Color(0.03f, 0.24f, 0.45f), 0.25f, 0.3f,
            new Color(0.02f, 0.45f, 1.5f));
        redGlow = MaterialAsset("M_Selection_RedSpectral", new Color(0.42f, 0.035f, 0.012f), 0.2f, 0.35f,
            new Color(1.6f, 0.08f, 0.01f));
        moon = MaterialAsset("M_Selection_Moon", new Color(0.65f, 0.75f, 0.9f), 0f, 0.2f,
            new Color(1.4f, 1.65f, 2.1f));
    }

    private static Material MaterialAsset(string name, Color color, float metallic, float smoothness,
        Color? emission = null)
    {
        string path = $"{GeneratedPath}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        if (emission.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void BuildEnvironment(Transform root)
    {
        Transform castle = Group("RuinedCastle", root);
        Material floor = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Project/Scenes/Castillo/Scenes/Imagenes/Piso Castillo.mat") ?? darkStone;
        Primitive("StoneFloor", PrimitiveType.Cube, root, new Vector3(0, -0.45f, 2f), new Vector3(28, 0.5f, 20), floor);
        BuildModularCastle(castle);

        Transform platform = Group("CentralPlatform", root);
        Primitive("PlatformBase", PrimitiveType.Cylinder, platform, new Vector3(0, 0.05f, 1.1f), new Vector3(5.4f, 0.45f, 5.4f), stone);
        Primitive("PlatformGoldRim", PrimitiveType.Cylinder, platform, new Vector3(0, 0.3f, 1.1f), new Vector3(4.8f, 0.12f, 4.8f), gold);
        Primitive("PlatformTop", PrimitiveType.Cylinder, platform, new Vector3(0, 0.39f, 1.1f), new Vector3(4.45f, 0.12f, 4.45f), darkStone);
        GameObject runes = Primitive("PlatformRunes", PrimitiveType.Cylinder, platform, new Vector3(0, 0.47f, 1.1f), new Vector3(3.75f, 0.025f, 3.75f), gold);
        BuildOrnamentalPlatform(platform);

        Transform avatarSpawn = Group("AvatarSpawnPoint", root);
        avatarSpawn.position = new Vector3(0, 0.48f, 1.1f);
        Marker("LeftGuardianPosition", root, new Vector3(-5.2f, 0.1f, 1.2f));
        Marker("RightGuardianPosition", root, new Vector3(5.2f, 0.1f, 1.2f));

        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 9; i++)
            {
                GameObject link = Primitive($"Chain_{side}_{i}", PrimitiveType.Capsule, castle,
                    new Vector3(side * (5.8f - i * 0.18f), 7.2f - i * 0.55f, 3.5f + i * 0.2f),
                    new Vector3(0.09f, 0.23f, 0.09f), gold);
                link.transform.rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 90 : 0);
            }
        }

        Primitive("Moon", PrimitiveType.Sphere, root, new Vector3(0, 7.75f, 9.2f), Vector3.one * 1.45f, moon);
        BuildBanners(castle);

        CreateGuardian(root, "LeftGuardian_Blue", new Vector3(-5.1f, 0.05f, 1.65f), blueGlow, new Color(0.05f, 0.45f, 1f));
        CreateGuardian(root, "RightGuardian_Red", new Vector3(5.1f, 0.05f, 1.65f), redGlow, new Color(1f, 0.12f, 0.025f));
        CreateFog(root);
    }

    private static void BuildOrnamentalPlatform(Transform platform)
    {
        for (int ring = 0; ring < 2; ring++)
        {
            float radius = ring == 0 ? 2.2f : 2.55f;
            for (int i = 0; i < 32; i++)
            {
                float angle = i * Mathf.PI * 2f / 32f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, .55f, 1.1f + Mathf.Sin(angle) * radius);
                GameObject segment = Primitive($"GoldRing_{ring}_{i}", PrimitiveType.Cube, platform, position,
                    new Vector3(.38f, .055f, .1f), gold);
                segment.transform.rotation = Quaternion.Euler(0, 90f - angle * Mathf.Rad2Deg, 0);
            }
        }

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            GameObject rune = Primitive($"Rune_{i}", PrimitiveType.Cube, platform,
                new Vector3(Mathf.Cos(angle) * 1.45f, .565f, 1.1f + Mathf.Sin(angle) * 1.45f),
                new Vector3(.07f, .025f, .75f), gold);
            rune.transform.rotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0);
        }
    }

    private static void BuildModularCastle(Transform parent)
    {
        string wallPath = "Assets/Project/Scenes/Castillo/Prefabs/Wall.prefab";
        string columnPath = "Assets/Project/Scenes/Castillo/Prefabs/Column.prefab";
        string archPath = "Assets/Project/Scenes/Castillo/Prefabs/Double Door Frame.prefab";

        for (int side = -1; side <= 1; side += 2)
        {
            PlacePrefabFitted($"MedievalWall_{side}", wallPath, parent,
                new Vector3(side * 8.8f, 2.8f, 4.2f), new Vector3(5.5f, 6.2f, 1.1f),
                Quaternion.Euler(0, side < 0 ? 82f : -82f, 0));
            PlacePrefabFitted($"MedievalColumnFront_{side}", columnPath, parent,
                new Vector3(side * 7.4f, 2.8f, -0.8f), new Vector3(1.5f, 6.1f, 1.5f), Quaternion.identity);
            PlacePrefabFitted($"MedievalColumnBack_{side}", columnPath, parent,
                new Vector3(side * 7.4f, 2.8f, 7.5f), new Vector3(1.5f, 6.1f, 1.5f), Quaternion.identity);
        }

        PlacePrefabFitted("GrandArchCenter", archPath, parent,
            new Vector3(0, 3.2f, 8.45f), new Vector3(5.1f, 6.4f, 1.2f), Quaternion.identity);
        PlacePrefabFitted("GrandArchLeft", archPath, parent,
            new Vector3(-5.2f, 3f, 8.25f), new Vector3(4.2f, 5.8f, 1.1f), Quaternion.identity);
        PlacePrefabFitted("GrandArchRight", archPath, parent,
            new Vector3(5.2f, 3f, 8.25f), new Vector3(4.2f, 5.8f, 1.1f), Quaternion.identity);
        PlacePrefabFitted("BackWallFarLeft", wallPath, parent,
            new Vector3(-8.35f, 3f, 8.75f), new Vector3(3.1f, 6f, 1f), Quaternion.identity);
        PlacePrefabFitted("BackWallInnerLeft", wallPath, parent,
            new Vector3(-2.75f, 3f, 9.05f), new Vector3(2.1f, 6f, 1f), Quaternion.identity);
        PlacePrefabFitted("BackWallInnerRight", wallPath, parent,
            new Vector3(2.75f, 3f, 9.05f), new Vector3(2.1f, 6f, 1f), Quaternion.identity);
        PlacePrefabFitted("BackWallFarRight", wallPath, parent,
            new Vector3(8.35f, 3f, 8.75f), new Vector3(3.1f, 6f, 1f), Quaternion.identity);
        PlacePrefabFitted("CastleBackdrop", "Assets/Project/Scenes/Castillo/Prefabs/Castle.prefab", parent,
            new Vector3(0, 4.2f, 12.2f), new Vector3(22f, 8.5f, 4.5f), Quaternion.identity);
        PlacePrefabFitted("SideTowerLeft", "Assets/Project/Scenes/Castillo/Prefabs/Tower A.prefab", parent,
            new Vector3(-9.2f, 4f, 5.6f), new Vector3(3.5f, 8f, 3.5f), Quaternion.Euler(0, 20, 0));
        PlacePrefabFitted("SideTowerRight", "Assets/Project/Scenes/Castillo/Prefabs/Tower A.prefab", parent,
            new Vector3(9.2f, 4f, 5.6f), new Vector3(3.5f, 8f, 3.5f), Quaternion.Euler(0, -20, 0));
    }

    private static GameObject PlacePrefabFitted(string name, string path, Transform parent, Vector3 center,
        Vector3 targetSize, Quaternion rotation)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.SetPositionAndRotation(Vector3.zero, rotation);
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            instance.transform.position = center;
            return instance;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        Vector3 safe = new Vector3(Mathf.Max(bounds.size.x, .01f), Mathf.Max(bounds.size.y, .01f), Mathf.Max(bounds.size.z, .01f));
        float uniform = Mathf.Min(targetSize.x / safe.x, Mathf.Min(targetSize.y / safe.y, targetSize.z / safe.z));
        instance.transform.localScale *= uniform;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        instance.transform.position += center - bounds.center;
        return instance;
    }

    private static void BuildBanners(Transform parent)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject banner = Primitive($"TornBanner_{side}", PrimitiveType.Cube, parent,
                new Vector3(side * 6.65f, 4.45f, 7.6f), new Vector3(1.1f, 2.7f, 0.055f), darkStone);
            banner.transform.rotation = Quaternion.Euler(0, side * -8f, side * 2f);
            Primitive($"BannerTrim_{side}", PrimitiveType.Cube, parent,
                new Vector3(side * 6.65f, 5.82f, 7.52f), new Vector3(1.25f, 0.08f, 0.09f), gold);
        }
    }

    private static void BuildArch(Transform parent, float x, float z, float scale)
    {
        Primitive($"ArchPillarL_{x}", PrimitiveType.Cube, parent, new Vector3(x - 1.7f * scale, 3.1f, z),
            new Vector3(0.65f * scale, 6.2f, 0.75f), stone);
        Primitive($"ArchPillarR_{x}", PrimitiveType.Cube, parent, new Vector3(x + 1.7f * scale, 3.1f, z),
            new Vector3(0.65f * scale, 6.2f, 0.75f), stone);
        for (int i = 0; i < 7; i++)
        {
            float angle = Mathf.Lerp(10f, 170f, i / 6f) * Mathf.Deg2Rad;
            float px = x + Mathf.Cos(angle) * 1.7f * scale;
            float py = 4.95f + Mathf.Sin(angle) * 1.7f * scale;
            GameObject block = Primitive($"ArchStone_{x}_{i}", PrimitiveType.Cube, parent,
                new Vector3(px, py, z), new Vector3(0.75f * scale, 0.65f * scale, 0.8f), stone);
            block.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90f);
        }
    }

    private static void CreateGuardian(Transform root, string name, Vector3 position, Material material, Color lightColor)
    {
        Transform group = Group(name, root);
        group.position = position;
        GameObject wolfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project/V2_Cristofer/Lobo/Prefab_URP/Wolf_URP.prefab");
        if (wolfPrefab != null)
        {
            GameObject wolf = (GameObject)PrefabUtility.InstantiatePrefab(wolfPrefab, group);
            wolf.name = "GuardianModel";
            wolf.transform.localPosition = Vector3.zero;
            wolf.transform.localRotation = Quaternion.Euler(0, position.x < 0 ? 35f : -35f, 0);
            foreach (Renderer renderer in wolf.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
            foreach (MonoBehaviour behaviour in wolf.GetComponentsInChildren<MonoBehaviour>()) behaviour.enabled = false;
            FitModelToHeight(wolf, 4f);
        }
        Light guardianLight = new GameObject("GuardianLight").AddComponent<Light>();
        guardianLight.transform.SetParent(group, false);
        guardianLight.transform.localPosition = new Vector3(0, 1.5f, -0.8f);
        guardianLight.type = LightType.Point;
        guardianLight.color = lightColor;
        guardianLight.intensity = 7f;
        guardianLight.range = 7f;
        guardianLight.shadows = LightShadows.Soft;
        CreateParticles("SpectralParticles", group, new Vector3(0, 0.2f, 0), lightColor, 1.4f, 28);
    }

    private static void FitModelToHeight(GameObject model, float targetHeight)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y < .001f) return;
        model.transform.localScale *= targetHeight / bounds.size.y;
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        model.transform.position += Vector3.up * (model.transform.parent.position.y - bounds.min.y);
    }

    private static void CreateFog(Transform root)
    {
        Transform fog = Group("FogParticles", root);
        CreateParticles("FloorMist", fog, new Vector3(0, 0.15f, 3f),
            new Color(0.22f, 0.32f, 0.44f, 0.12f), 0.35f, 45, new Vector3(12, 0.25f, 6));
    }

    private static ParticleSystem CreateParticles(string name, Transform parent, Vector3 localPosition, Color color,
        float size, int rate, Vector3? box = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 3.5f;
        main.startSpeed = 0.35f;
        main.startSize = size;
        main.startColor = color;
        main.maxParticles = 180;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission;
        emission.rateOverTime = rate;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = box ?? new Vector3(2.5f, 0.4f, 2f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.25f;
        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        return ps;
    }

    private static Camera BuildCamera()
    {
        GameObject go = new GameObject("SelectionCamera");
        go.tag = "MainCamera";
        Camera camera = go.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 3.25f, -13.8f);
        camera.transform.LookAt(new Vector3(0, 2.35f, 2.3f));
        camera.fieldOfView = 48f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 80f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.01f, 0.038f, 0.085f);
        camera.allowHDR = true;
        go.AddComponent<AudioListener>();
        return camera;
    }

    private static void BuildLighting(Transform root)
    {
        Light moonLight = LightObject("MoonLight", root, new Vector3(-2f, 7f, -2f), LightType.Directional,
            new Color(0.22f, 0.38f, 0.65f), 1.2f, 30f);
        moonLight.transform.rotation = Quaternion.Euler(42f, -22f, 0);
        Light avatarLight = LightObject("AvatarWarmLight", root, new Vector3(0, 5.4f, -2.5f), LightType.Spot,
            new Color(1f, 0.57f, 0.24f), 10f, 14f);
        avatarLight.spotAngle = 56f;
        avatarLight.transform.LookAt(new Vector3(0, 1.6f, 1.1f));
        avatarLight.shadows = LightShadows.Soft;
        Transform torches = Group("TorchLights", root);
        CreateTorch(torches, new Vector3(-7f, 2.15f, -1f));
        CreateTorch(torches, new Vector3(7f, 2.15f, -1f));
        CreateTorch(torches, new Vector3(-7f, 2.15f, 6f));
        CreateTorch(torches, new Vector3(7f, 2.15f, 6f));
    }

    private static void CreateTorch(Transform root, Vector3 position)
    {
        Transform torch = Group($"Torch_{position.x}_{position.z}", root);
        torch.position = position;
        Primitive("Bracket", PrimitiveType.Cylinder, torch, Vector3.zero, new Vector3(0.12f, 0.65f, 0.12f), gold);
        CreateParticles("Flame", torch, new Vector3(0, 0.65f, 0),
            new Color(1f, 0.26f, 0.03f, 0.9f), 0.28f, 34, new Vector3(0.08f, 0.08f, 0.08f));
        Light lightSource = LightObject("PointLight", torch, new Vector3(0, 0.7f, 0), LightType.Point,
            new Color(1f, 0.28f, 0.055f), 4.2f, 7f);
        lightSource.shadows = LightShadows.Soft;
    }

    private static Light LightObject(string name, Transform parent, Vector3 position, LightType type, Color color,
        float intensity, float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        Light lightSource = go.AddComponent<Light>();
        lightSource.type = type;
        lightSource.color = color;
        lightSource.intensity = intensity;
        lightSource.range = range;
        lightSource.shadows = type == LightType.Directional ? LightShadows.Soft : LightShadows.None;
        return lightSource;
    }

    private static void BuildUI(Transform environment)
    {
        GameObject canvasGO = new GameObject("Canvas_SeleccionAvatar");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 0.5f;
        canvas.sortingOrder = 20;
        canvasGO.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        TMP_Text title = Text("Title", canvasGO.transform, "SELECCIONA\nTU AVATAR", 116, TextAlignmentOptions.Center,
            new Color(0.86f, 0.66f, 0.36f));
        SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -126), new Vector2(1120, 225));
        AddTextEffects(title);

        GameObject selectorGO = new GameObject("AvatarSelector");
        AvatarSelector selector = selectorGO.AddComponent<AvatarSelector>();
        selector.avatares = LoadAvatarSprites();
        Transform spawn = GameObject.Find("AvatarSpawnPoint").transform;
        selector.avatarSpawnPoint = spawn;
        selector.presentaciones = CreatePresentations(selector.avatares);
        selector.modeloInicialEnEscena = CreateInitialAvatarPreview(spawn);

        GameObject nameFrame = Frame("AvatarNamePanel", canvasGO.transform, new Vector2(0.5f, 0),
            new Vector2(0, 303), new Vector2(500, 82));
        TMP_Text name = Text("AvatarName", nameFrame.transform, "BETZA", 44, TextAlignmentOptions.Center, new Color(0.9f, 0.73f, 0.48f));
        Stretch(name.rectTransform, 14);
        selector.avatarNombre = name;

        Button prev = NavigationButton("PreviousButton", canvasGO.transform, "<", "ANTERIOR", new Vector2(0, 0.5f),
            new Vector2(185, 15));
        Button next = NavigationButton("NextButton", canvasGO.transform, ">", "SIGUIENTE", new Vector2(1, 0.5f),
            new Vector2(-185, 15));
        Button select = Button("SelectButton", canvasGO.transform, "SELECCIONAR", new Vector2(0.5f, 0),
            new Vector2(0, 88), new Vector2(760, 132), 58);
        Button back = Button("BackButton", canvasGO.transform, "<  VOLVER", new Vector2(0, 0),
            new Vector2(185, 82), new Vector2(330, 104), 40);

        UnityEventTools.AddPersistentListener(prev.onClick, selector.AnteriorAvatar);
        UnityEventTools.AddPersistentListener(next.onClick, selector.SiguienteAvatar);
        UnityEventTools.AddPersistentListener(select.onClick, selector.SeleccionarAvatar);

        GameObject menuGO = new GameObject("MenuController");
        MenuController menu = menuGO.AddComponent<MenuController>();
        UnityEventTools.AddPersistentListener(back.onClick, menu.VolverMenu);

        BuildStats(canvasGO.transform, selector);
        BuildTitleOrnaments(canvasGO.transform);
        CreateVignette(canvasGO.transform);
        CreateEventSystem();
        EditorUtility.SetDirty(selector);
    }

    private static void BuildStats(Transform canvas, AvatarSelector selector)
    {
        GameObject stats = Frame("StatsPanel", canvas, new Vector2(1, 0), new Vector2(-300, 145), new Vector2(540, 260));
        selector.vidaBarra = StatRow(stats.transform, "HealthStat", "+  VIDA", new Vector2(0, 78),
            new Color(0.62f, 0.08f, 0.055f));
        selector.ataqueBarra = StatRow(stats.transform, "AttackStat", "X  ATAQUE", new Vector2(0, 0),
            new Color(0.75f, 0.4f, 0.06f));
        selector.velocidadBarra = StatRow(stats.transform, "SpeedStat", ">> VELOCIDAD", new Vector2(0, -78),
            new Color(0.04f, 0.37f, 0.67f));
        selector.vidaBarra.fillAmount = .86f;
        selector.ataqueBarra.fillAmount = .64f;
        selector.velocidadBarra.fillAmount = .72f;
    }

    private static Image StatRow(Transform parent, string name, string label, Vector2 position, Color color)
    {
        GameObject row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        SetRect((RectTransform)row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(500, 66));
        TMP_Text text = Text("Label", row.transform, label, 29, TextAlignmentOptions.MidlineLeft, new Color(0.9f, 0.75f, 0.52f));
        SetRect(text.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(102, 0), new Vector2(205, 56));
        Image bg = UIObject("BarBackground", row.transform, new Color(0.025f, 0.025f, 0.03f, 0.95f));
        SetRect(bg.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-124, 0), new Vector2(248, 28));
        Image fill = UIObject("Fill", bg.transform, color);
        Stretch(fill.rectTransform, 3);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        return fill;
    }

    private static AvatarSelector.AvatarPresentation[] CreatePresentations(Sprite[] sprites)
    {
        string[] names = { "BETZA", "MAYU", "MIGUEL", "CRISTOFER", "MONTOYA", "IKER", "KEVIN L.", "DELGADO", "DIEGO" };
        float[,] stats =
        {
            { .86f, .64f, .72f }, { .72f, .58f, .92f }, { .82f, .79f, .61f },
            { .94f, .73f, .55f }, { .68f, .91f, .70f }, { .78f, .67f, .86f },
            { .88f, .84f, .58f }, { .73f, .76f, .89f }, { .81f, .88f, .66f }
        };
        GameObject preview = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project/V2_Cristofer/PersonajePrincipal/Ares_Visual.prefab");
        var values = new AvatarSelector.AvatarPresentation[sprites.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = new AvatarSelector.AvatarPresentation
            {
                nombre = i < names.Length ? names[i] : sprites[i].name,
                vida = stats[i % 9, 0],
                ataque = stats[i % 9, 1],
                velocidad = stats[i % 9, 2],
                modeloPreview = preview,
                posicionLocal = new Vector3(0, 0.05f, 0),
                rotacionLocal = new Vector3(0, 180, 0),
                escalaLocal = Vector3.one,
                alturaPreview = 3.35f
            };
        }
        return values;
    }

    private static GameObject CreateInitialAvatarPreview(Transform spawn)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Project/V2_Cristofer/PersonajePrincipal/Ares_Visual.prefab");
        if (prefab == null) return null;
        GameObject preview = (GameObject)PrefabUtility.InstantiatePrefab(prefab, spawn);
        preview.name = "AvatarPreview_Initial";
        preview.transform.localPosition = Vector3.zero;
        preview.transform.localRotation = Quaternion.Euler(0, 180, 0);
        foreach (MonoBehaviour behaviour in preview.GetComponentsInChildren<MonoBehaviour>(true))
        {
            behaviour.enabled = false;
        }
        FitModelToHeight(preview, 3.35f);
        return preview;
    }

    private static Sprite[] LoadAvatarSprites()
    {
        string[] paths =
        {
            "Assets/Project/UI/Icons/AvatarBetza.png", "Assets/Project/UI/Icons/AvatarMayu.png",
            "Assets/Project/UI/Icons/AvatarMiguel.jpeg", "Assets/Project/UI/Icons/AvatarCristofer.jpeg",
            "Assets/Project/UI/Icons/AvatarMontoya.png", "Assets/Project/UI/Icons/AvatarIker.png",
            "Assets/Project/UI/Icons/AvatarKevinL.png", "Assets/Project/UI/Icons/AvatarDelgado.png",
            "Assets/Project/UI/Icons/AvatarDiego.jpeg"
        };
        Sprite[] sprites = new Sprite[paths.Length];
        for (int i = 0; i < paths.Length; i++) sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);
        return sprites;
    }

    private static Button Button(string name, Transform parent, string label, Vector2 anchor, Vector2 position,
        Vector2 size, float fontSize)
    {
        GameObject frame = Frame(name, parent, anchor, position, size);
        Button button = frame.AddComponent<Button>();
        Image target = frame.GetComponent<Image>();
        button.targetGraphic = target;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.14f, 0.12f, 0.11f, 0.96f);
        colors.highlightedColor = new Color(0.26f, 0.19f, 0.105f, 1f);
        colors.pressedColor = new Color(0.07f, 0.055f, 0.045f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.colorMultiplier = 1f;
        button.colors = colors;
        TMP_Text text = Text("Text", frame.transform, label, fontSize, TextAlignmentOptions.Center,
            new Color(0.9f, 0.72f, 0.46f));
        Stretch(text.rectTransform, 12);
        AddTextEffects(text);
        MedievalButtonFeedback feedback = frame.AddComponent<MedievalButtonFeedback>();
        feedback.Configure(frame.transform.Find("GoldBorder").GetComponent<Image>());
        return button;
    }

    private static Button NavigationButton(string name, Transform parent, string arrow, string label,
        Vector2 anchor, Vector2 position)
    {
        Image hitArea = UIObject(name, parent, new Color(0, 0, 0, .01f));
        SetRect(hitArea.rectTransform, anchor, anchor, position, new Vector2(330, 220));
        Button button = hitArea.gameObject.AddComponent<Button>();
        button.targetGraphic = hitArea;

        CircleGraphic circle = CircleObject("CircularArrowFrame", hitArea.transform, new Color(.78f, .48f, .15f, .96f));
        SetRect(circle.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 35), new Vector2(144, 144));
        CircleGraphic circleInset = CircleObject("CircularArrowInset", circle.transform, new Color(.045f, .04f, .038f, 1f));
        SetRect(circleInset.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(126, 126));

        TMP_Text arrowText = Text("Arrow", circleInset.transform, arrow, 82, TextAlignmentOptions.Center,
            new Color(.92f, .72f, .43f));
        Stretch(arrowText.rectTransform, 8);
        AddTextEffects(arrowText);

        TMP_Text labelText = Text("Label", hitArea.transform, label, 40, TextAlignmentOptions.Center,
            new Color(.9f, .71f, .44f));
        SetRect(labelText.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f),
            new Vector2(0, -68), new Vector2(310, 62));
        AddTextEffects(labelText);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, .78f, .38f, 1f);
        colors.pressedColor = new Color(.55f, .38f, .22f, 1f);
        button.colors = colors;
        MedievalButtonFeedback feedback = hitArea.gameObject.AddComponent<MedievalButtonFeedback>();
        feedback.Configure(circle);
        return button;
    }

    private static CircleGraphic CircleObject(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        CircleGraphic graphic = go.AddComponent<CircleGraphic>();
        graphic.color = color;
        graphic.raycastTarget = false;
        graphic.SetAllDirty();
        return graphic;
    }

    private static Sprite CreateCircleSprite()
    {
        const string path = GeneratedPath + "/UI_Circle.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(255, 255, 255, 0);
        Color32 white = new Color32(255, 255, 255, 255);
        float radius = size * .48f;
        Vector2 center = Vector2.one * (size - 1) * .5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            texture.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), center) <= radius ? white : clear);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void BuildTitleOrnaments(Transform canvas)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            Image line = UIObject($"TitleLine_{side}", canvas, new Color(.72f, .42f, .13f, .85f));
            SetRect(line.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1),
                new Vector2(side * 560, -218), new Vector2(310, 4));
            Image diamond = UIObject($"TitleDiamond_{side}", canvas, new Color(.9f, .62f, .25f, 1));
            SetRect(diamond.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1),
                new Vector2(side * 400, -218), new Vector2(20, 20));
            diamond.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        }
    }

    private static GameObject Frame(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        Image outer = UIObject(name, parent, new Color(0.055f, 0.05f, 0.047f, 0.96f));
        SetRect(outer.rectTransform, anchor, anchor, position, size);
        Shadow shadow = outer.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.85f);
        shadow.effectDistance = new Vector2(8, -8);
        Image border = UIObject("GoldBorder", outer.transform, new Color(0.63f, 0.39f, 0.15f, 0.65f));
        Stretch(border.rectTransform, 5);
        Image inset = UIObject("Inset", border.transform, new Color(0.045f, 0.042f, 0.04f, 0.98f));
        Stretch(inset.rectTransform, 5);
        AddFrameOrnaments(outer.transform, size);
        return outer.gameObject;
    }

    private static void AddFrameOrnaments(Transform frame, Vector2 size)
    {
        Vector2[] anchors =
        {
            new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(.5f, 0), new Vector2(.5f, 1)
        };
        for (int i = 0; i < anchors.Length; i++)
        {
            Image ornament = UIObject($"Ornament_{i}", frame, new Color(.82f, .55f, .2f, .9f));
            SetRect(ornament.rectTransform, anchors[i], anchors[i], Vector2.zero, new Vector2(18, 18));
            ornament.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            ornament.raycastTarget = false;
        }
    }

    private static void CreateVignette(Transform canvas)
    {
        Image top = UIObject("TopShade", canvas, new Color(0, 0, 0, 0.32f));
        SetRect(top.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1920, 60));
        top.raycastTarget = false;
    }

    private static void CaptureGameCamera(Camera camera)
    {
        RenderTexture texture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        RenderTexture previousTarget = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        camera.targetTexture = texture;
        RenderTexture.active = texture;
        foreach (TextMeshProUGUI text in Object.FindObjectsByType<TextMeshProUGUI>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            text.ForceMeshUpdate(true, true);
        }
        foreach (CircleGraphic circle in Object.FindObjectsByType<CircleGraphic>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            circle.SetAllDirty();
            circle.Rebuild(CanvasUpdate.PreRender);
        }
        Canvas.ForceUpdateCanvases();
        camera.Render();
        Canvas.ForceUpdateCanvases();
        camera.Render();

        Texture2D screenshot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        screenshot.Apply();
        File.WriteAllBytes("SeleccionAvatar_GameView.png", screenshot.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        texture.Release();
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(screenshot);
    }

    private static TMP_Text Text(string name, Transform parent, string value, float size,
        TextAlignmentOptions alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(14, size * 0.55f);
        text.fontSizeMax = size;
        text.raycastTarget = false;
        return text;
    }

    private static void AddTextEffects(TMP_Text text)
    {
        text.fontStyle = FontStyles.Bold;
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color32(35, 19, 8, 255);
    }

    private static Image UIObject(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset;
        rect.offsetMax = Vector2.one * -inset;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        System.Type inputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModule != null) eventSystem.AddComponent(inputModule);
        else eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static Transform Group(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void Marker(string name, Transform parent, Vector3 position)
    {
        Transform marker = Group(name, parent);
        marker.position = position;
    }

    private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return go;
    }
}
#endif
