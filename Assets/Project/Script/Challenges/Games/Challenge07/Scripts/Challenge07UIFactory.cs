using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Challenge07UIFactory
{
    private static readonly Color Ink = new Color(0.16f, 0.075f, 0.025f, 1f);
    private static readonly Color Gold = new Color(0.88f, 0.59f, 0.18f, 1f);
    private static readonly Color Parchment = new Color(0.74f, 0.55f, 0.29f, 1f);
    private static readonly Color DarkWood = new Color(0.075f, 0.032f, 0.012f, 1f);

    public static Challenge07PuzzleController Create(
        Transform parent,
        Challenge07GameBridge bridge,
        Sprite mapComplete = null,
        Sprite[] sixPieceSprites = null,
        Sprite[] ninePieceSprites = null,
        AudioClip ambientMusic = null,
        AudioClip pickupClip = null,
        AudioClip correctClip = null,
        AudioClip wrongClip = null,
        AudioClip hintClip = null,
        AudioClip hoverClip = null,
        AudioClip clickClip = null,
        AudioClip victoryClip = null)
    {
        if (parent == null) return null;

        Transform existing = parent.name == "Challenge07UI" ? parent : parent.Find("Challenge07UI");
        if (existing != null)
        {
            Challenge07PuzzleController existingController = existing.GetComponent<Challenge07PuzzleController>();
            if (existingController != null && bridge != null) bridge.ConfigureRuntime(existingController);
            return existingController;
        }

        GameObject root = CreateUI("Challenge07UI", parent);
        Stretch(root.GetComponent<RectTransform>());
        CanvasGroup rootGroup = root.AddComponent<CanvasGroup>();
        AddImage(root, new Color(0.008f, 0.012f, 0.01f, 0.96f));

        GameObject darkenerObject = CreateUI("BackgroundDarkener", root.transform);
        Stretch(darkenerObject.GetComponent<RectTransform>());
        AddImage(darkenerObject, Color.black).raycastTarget = false;
        CanvasGroup darkener = darkenerObject.AddComponent<CanvasGroup>();
        darkener.alpha = 0f;
        darkener.blocksRaycasts = false;

        GameObject mainFrame = CreatePanel("MainFrame", root.transform, new Vector2(0.5f, 0.5f), new Vector2(1720f, 980f), DarkWood);
        AddOutline(mainFrame, Gold, 5f);

        GameObject topHeader = CreatePanel("Header", mainFrame.transform, new Vector2(0.5f, 1f), new Vector2(1640f, 145f), new Color(0.115f, 0.052f, 0.018f, 1f));
        topHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -82f);
        AddOutline(topHeader, new Color(0.42f, 0.25f, 0.08f, 1f), 2f);

        GameObject missionIcon = CreatePanel("MissionIcon", topHeader.transform, new Vector2(0f, 0.5f), new Vector2(92f, 100f), new Color(0.12f, 0.22f, 0.25f, 1f));
        missionIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(65f, 0f);
        AddOutline(missionIcon, Gold, 3f);
        TMP_Text missionGlyph = CreateText("Icon", missionIcon.transform, "VII", 32f, Gold, FontStyles.Bold);
        Stretch(missionGlyph.rectTransform);

        TMP_Text missionText = CreateText("MissionText", topHeader.transform, "MISIÓN\n1/1", 22f, Gold, FontStyles.Bold);
        SetRect(missionText.rectTransform, new Vector2(0f, 0.5f), new Vector2(165f, 0f), new Vector2(120f, 90f));

        GameObject titleBanner = CreatePanel("TitleBanner", topHeader.transform, new Vector2(0.5f, 0.5f), new Vector2(820f, 112f), Parchment);
        AddOutline(titleBanner, new Color(0.38f, 0.20f, 0.055f, 1f), 3f);
        TMP_Text titleText = CreateText("TitleText", titleBanner.transform, "RECONSTRUIR EL MAPA PERDIDO", 43f, Ink, FontStyles.Bold);
        Stretch(titleText.rectTransform);

        GameObject scorePanel = CreatePanel("ScorePanel", topHeader.transform, new Vector2(1f, 0.5f), new Vector2(170f, 70f), new Color(0.12f, 0.055f, 0.02f, 1f));
        scorePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-190f, 0f);
        AddOutline(scorePanel, Gold, 2f);
        TMP_Text score = CreateText("ScoreText", scorePanel.transform, "★  6", 29f, Gold, FontStyles.Bold);
        Stretch(score.rectTransform);

        Button settingsButton = CreateButton("SettingsButton", topHeader.transform, "⚙", new Vector2(1f, 0.5f), new Vector2(-62f, 0f), new Vector2(72f, 72f), null, hoverClip, clickClip);
        settingsButton.interactable = false;

        GameObject instructionBanner = CreatePanel("InstructionBanner", mainFrame.transform, new Vector2(0.5f, 1f), new Vector2(1580f, 62f), Parchment);
        instructionBanner.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -187f);
        AddOutline(instructionBanner, new Color(0.38f, 0.20f, 0.055f, 1f), 2f);
        TMP_Text instructionText = CreateText("InstructionText", instructionBanner.transform, "Arrastra las piezas para reconstruir el mapa y encontrar el camino al castillo.", 24f, Ink, FontStyles.Italic);
        Stretch(instructionText.rectTransform);

        GameObject contentArea = CreateUI("Content", mainFrame.transform);
        SetRect(contentArea.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(1580f, 650f));

        GameObject puzzleBoard = CreatePanel("PuzzleBoard", contentArea.transform, new Vector2(0f, 0.5f), new Vector2(850f, 650f), new Color(0.24f, 0.12f, 0.035f, 1f));
        puzzleBoard.GetComponent<RectTransform>().anchoredPosition = new Vector2(425f, 0f);
        AddOutline(puzzleBoard, Gold, 3f);
        Image mapBackground = CreatePanel("BoardBackground", puzzleBoard.transform, new Vector2(0.5f, 0.5f), new Vector2(780f, 600f), new Color(0.66f, 0.49f, 0.25f, 1f)).GetComponent<Image>();
        mapBackground.sprite = mapComplete;
        mapBackground.preserveAspect = true;
        if (mapComplete != null) mapBackground.color = new Color(0.30f, 0.22f, 0.11f, 0.32f);

        GameObject targets6Root = CreateUI("Targets6", puzzleBoard.transform);
        Stretch(targets6Root.GetComponent<RectTransform>());
        RectTransform[] targets6 = CreateTargets(targets6Root.transform, "Target6", 2, 3, new Vector2(300f, 190f), new Vector2(295f, 185f));
        GameObject targets9Root = CreateUI("Targets9", puzzleBoard.transform);
        Stretch(targets9Root.GetComponent<RectTransform>());
        RectTransform[] targets9 = CreateTargets(targets9Root.transform, "Target9", 3, 3, new Vector2(190f, 190f), new Vector2(185f, 185f));
        targets9Root.SetActive(false);

        GameObject castleReveal = CreatePanel("CastleReveal", puzzleBoard.transform, new Vector2(0.5f, 0.64f), new Vector2(175f, 175f), new Color(0.12f, 0.04f, 0.01f, 0.9f));
        AddOutline(castleReveal, Gold, 4f);
        TMP_Text castleText = CreateText("CastleMark", castleReveal.transform, "CASTILLO", 24f, Gold, FontStyles.Bold);
        Stretch(castleText.rectTransform);
        castleReveal.SetActive(false);

        ParticleSystem victoryParticles = CreateParticles("VictoryParticles", puzzleBoard.transform, 70, 2.1f);
        GameObject placedPiecesRoot = CreateUI("PlacedPiecesRoot", puzzleBoard.transform);
        Stretch(placedPiecesRoot.GetComponent<RectTransform>());
        Image boardCompletedMap = CreatePanel("CompletedMap", puzzleBoard.transform, new Vector2(0.5f, 0.5f), new Vector2(780f, 600f), Color.white).GetComponent<Image>();
        boardCompletedMap.sprite = mapComplete;
        boardCompletedMap.preserveAspect = true;
        boardCompletedMap.raycastTarget = false;
        boardCompletedMap.gameObject.SetActive(false);

        GameObject piecesPanel = CreatePanel("RemainingPiecesPanel", contentArea.transform, new Vector2(1f, 0.5f), new Vector2(660f, 650f), Parchment);
        piecesPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-330f, 0f);
        AddOutline(piecesPanel, new Color(0.40f, 0.22f, 0.065f, 1f), 3f);
        TMP_Text remainingTitle = CreateText("RemainingTitle", piecesPanel.transform, "PIEZAS RESTANTES", 27f, Ink, FontStyles.Bold);
        SetRect(remainingTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(560f, 48f));

        AudioSource effectsSource = root.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        AudioSource musicSource = root.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;

        Challenge07PuzzleController controller = root.AddComponent<Challenge07PuzzleController>();
        GameObject pieces6Root = CreateUI("Pieces6", piecesPanel.transform);
        Stretch(pieces6Root.GetComponent<RectTransform>());
        GameObject pieces9Root = CreateUI("Pieces9", piecesPanel.transform);
        Stretch(pieces9Root.GetComponent<RectTransform>());
        MapPuzzlePiece[] pieces6 = CreatePieces(controller, pieces6Root.transform, "Piece6", sixPieceSprites, targets6, 2, 3, effectsSource, pickupClip, correctClip, wrongClip);
        MapPuzzlePiece[] pieces9 = CreatePieces(controller, pieces9Root.transform, "Piece9", ninePieceSprites, targets9, 3, 3, effectsSource, pickupClip, correctClip, wrongClip);
        pieces9Root.SetActive(false);

        GameObject bottomBar = CreatePanel("BottomBar", mainFrame.transform, new Vector2(0.5f, 0f), new Vector2(1580f, 105f), new Color(0.11f, 0.048f, 0.016f, 1f));
        bottomBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 65f);
        AddOutline(bottomBar, new Color(0.42f, 0.25f, 0.08f, 1f), 2f);
        Button hintButton = CreateButton("HintButton", bottomBar.transform, "PISTA", new Vector2(0f, 0.5f), new Vector2(150f, 0f), new Vector2(240f, 68f), effectsSource, hoverClip, clickClip);
        TMP_Text hintCounter = CreateText("HintCounter", bottomBar.transform, "3", 28f, Gold, FontStyles.Bold);
        SetRect(hintCounter.rectTransform, new Vector2(0f, 0.5f), new Vector2(300f, 0f), new Vector2(60f, 60f));
        TMP_Text objectiveText = CreateText("ObjectiveText", bottomBar.transform, "Objetivo: Completa el mapa para llegar al castillo.", 22f, Gold, FontStyles.Italic);
        SetRect(objectiveText.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 70f));
        TMP_Text progressText = CreateText("ProgressText", bottomBar.transform, "FRAGMENTOS 0/6", 22f, Gold, FontStyles.Bold);
        SetRect(progressText.rectTransform, new Vector2(1f, 0.5f), new Vector2(-380f, 0f), new Vector2(250f, 60f));
        Button restartButton = CreateButton("RestartButton", bottomBar.transform, "REINICIAR", new Vector2(1f, 0.5f), new Vector2(-145f, 0f), new Vector2(245f, 68f), effectsSource, hoverClip, clickClip);
        GameObject difficultyPanel = CreateUI("DifficultyPanel", bottomBar.transform);
        SetRect(difficultyPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, 30f), new Vector2(330f, 42f));
        Button sixPiecesButton = CreateButton("SixPiecesButton", difficultyPanel.transform, "6 PIEZAS", new Vector2(0f, 0.5f), new Vector2(78f, 0f), new Vector2(150f, 38f), effectsSource, hoverClip, clickClip);
        Button ninePiecesButton = CreateButton("NinePiecesButton", difficultyPanel.transform, "9 PIEZAS", new Vector2(1f, 0.5f), new Vector2(-78f, 0f), new Vector2(150f, 38f), effectsSource, hoverClip, clickClip);

        GameObject gamePanel = CreateUI("GamePanel", mainFrame.transform);
        Stretch(gamePanel.GetComponent<RectTransform>());
        contentArea.transform.SetParent(gamePanel.transform, false);
        bottomBar.transform.SetParent(gamePanel.transform, false);

        GameObject modeSelectionPanel = CreatePanel("ModeSelectionPanel", mainFrame.transform, new Vector2(0.5f, 0.43f), new Vector2(1500f, 650f), new Color(0.055f, 0.025f, 0.012f, 0.98f));
        AddOutline(modeSelectionPanel, Gold, 4f);
        TMP_Text modeTitle = CreateText("ModeTitle", modeSelectionPanel.transform, "SELECCIONA EL MODO", 36f, Gold, FontStyles.Bold);
        SetRect(modeTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(700f, 60f));
        difficultyPanel.transform.SetParent(modeSelectionPanel.transform, false);
        SetRect(difficultyPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(430f, 70f));
        SetRect(sixPiecesButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(105f, 0f), new Vector2(200f, 62f));
        SetRect(ninePiecesButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(-105f, 0f), new Vector2(200f, 62f));
        GameObject instructionsPanel = CreateUI("InstructionsPanel", modeSelectionPanel.transform);
        SetRect(instructionsPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -45f), new Vector2(1340f, 260f));
        string[] stepTitles = { "1. ARRASTRA", "2. COLOCA", "3. ACIERTA", "4. COMPLETA" };
        string[] stepDescriptions = { "Arrastra las piezas\ncon el mouse.", "Suelta la pieza en\nel lugar correcto.", "La pieza encaja\ny queda fija.", "Completa todas las\npiezas para ganar." };
        for (int i = 0; i < 4; i++)
        {
            GameObject step = CreatePanel($"Step{i + 1:00}", instructionsPanel.transform, new Vector2(0f, 0.5f), new Vector2(300f, 215f), Parchment);
            step.GetComponent<RectTransform>().anchoredPosition = new Vector2(170f + i * 335f, 0f);
            AddOutline(step, Gold, 2f);
            TMP_Text stepTitle = CreateText("Title", step.transform, stepTitles[i], 23f, Ink, FontStyles.Bold);
            SetRect(stepTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(270f, 45f));
            TMP_Text stepBody = CreateText("Description", step.transform, stepDescriptions[i], 20f, Ink, FontStyles.Normal);
            SetRect(stepBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -25f), new Vector2(270f, 110f));
        }
        Button startButton = CreateButton("StartButton", modeSelectionPanel.transform, "COMENZAR", new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(300f, 66f), effectsSource, hoverClip, clickClip);
        gamePanel.SetActive(false);

        GameObject victoryPanel = CreatePanel("VictoryPanel", mainFrame.transform, new Vector2(0.5f, 0.5f), new Vector2(800f, 610f), DarkWood);
        AddOutline(victoryPanel, Gold, 5f);
        GameObject victoryBackground = CreatePanel("VictoryBackground", victoryPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(750f, 560f), Parchment);
        AddOutline(victoryBackground, new Color(0.38f, 0.20f, 0.055f, 1f), 2f);
        TMP_Text victoryTitle = CreateText("VictoryTitle", victoryPanel.transform, "¡MAPA RECONSTRUIDO!", 39f, Gold, FontStyles.Bold);
        SetRect(victoryTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(680f, 64f));
        TMP_Text victoryMessage = CreateText("VictoryMessage", victoryPanel.transform, "La ubicación del castillo central ha sido revelada.", 23f, Ink, FontStyles.Italic);
        SetRect(victoryMessage.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(650f, 50f));
        Image castleImage = CreatePanel("CastleImage", victoryPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(440f, 330f), Color.white).GetComponent<Image>();
        castleImage.rectTransform.anchoredPosition = new Vector2(0f, -35f);
        castleImage.sprite = mapComplete;
        castleImage.preserveAspect = true;
        Button continueButton = CreateButton("ContinueButton", victoryPanel.transform, "CONTINUAR", new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(280f, 64f), effectsSource, hoverClip, clickClip);

        Challenge07UIAnimator uiAnimator = root.AddComponent<Challenge07UIAnimator>();
        Challenge07Audio audioController = root.AddComponent<Challenge07Audio>();
        audioController.Configure(musicSource, effectsSource, ambientMusic, pickupClip, correctClip, wrongClip, hintClip, clickClip, victoryClip);
        uiAnimator.Configure(rootGroup, darkener, victoryPanel.GetComponent<RectTransform>());
        victoryPanel.SetActive(false);

        controller.ConfigureRuntime(bridge, root, pieces6, pieces9, pieces6Root, pieces9Root, targets6Root, targets9Root, modeSelectionPanel, gamePanel, piecesPanel, progressText, victoryPanel, restartButton, hintButton, continueButton, sixPiecesButton, ninePiecesButton, startButton, hintCounter, boardCompletedMap, castleReveal, darkener, victoryParticles, musicSource, effectsSource, ambientMusic, victoryClip, hintClip, uiAnimator);
        if (bridge != null) bridge.ConfigureRuntime(controller);
        return controller;
    }

    private static RectTransform[] CreateTargets(Transform parent, string prefix, int columns, int rows, Vector2 spacing, Vector2 size)
    {
        int count = columns * rows;
        RectTransform[] result = new RectTransform[count];
        for (int i = 0; i < count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            Vector2 position = new Vector2((column - (columns - 1) * 0.5f) * spacing.x, ((rows - 1) * 0.5f - row) * spacing.y);
            GameObject target = CreatePanel($"{prefix}_{i + 1:00}", parent, new Vector2(0.5f, 0.5f), size, new Color(0.12f, 0.07f, 0.025f, 0.60f));
            target.GetComponent<RectTransform>().anchoredPosition = position;
            target.GetComponent<Image>().raycastTarget = false;
            AddOutline(target, new Color(Gold.r, Gold.g, Gold.b, 0.38f), 1.5f);
            result[i] = target.GetComponent<RectTransform>();
        }
        return result;
    }

    private static MapPuzzlePiece[] CreatePieces(Challenge07PuzzleController controller, Transform parent, string prefix, Sprite[] sprites, RectTransform[] targets, int columns, int rows, AudioSource effectsSource, AudioClip pickup, AudioClip correct, AudioClip wrong)
    {
        int count = columns * rows;
        MapPuzzlePiece[] result = new MapPuzzlePiece[count];
        int[] order6 = { 4, 1, 5, 0, 3, 2 };
        int[] order9 = { 7, 2, 5, 0, 8, 3, 6, 1, 4 };
        int[] order = count == 6 ? order6 : order9;
        float spacingX = columns == 2 ? 230f : 175f;
        float spacingY = rows == 3 ? 175f : 140f;
        Vector2 pieceSize = columns == 2 ? new Vector2(190f, 120f) : new Vector2(145f, 145f);

        for (int i = 0; i < count; i++)
        {
            int slot = order[i];
            int column = slot % columns;
            int row = slot / columns;
            Vector2 position = new Vector2((column - (columns - 1) * 0.5f) * spacingX, 115f - row * spacingY);
            GameObject pieceObject = CreatePanel($"{prefix}_{i + 1:00}", parent, new Vector2(0.5f, 0.5f), pieceSize, new Color(0.78f, 0.61f, 0.34f, 1f));
            pieceObject.GetComponent<RectTransform>().anchoredPosition = position;
            Image image = pieceObject.GetComponent<Image>();
            Sprite sprite = sprites != null && i < sprites.Length ? sprites[i] : null;
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = true;
            AddOutline(pieceObject, new Color(0.30f, 0.14f, 0.035f, 1f), 2f);
            Shadow shadow = pieceObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.62f);
            shadow.effectDistance = new Vector2(5f, -5f);
            pieceObject.AddComponent<CanvasGroup>();
            if (sprite == null)
            {
                TMP_Text fallback = CreateText("PieceNumber", pieceObject.transform, (i + 1).ToString(), 35f, Ink, FontStyles.Bold);
                Stretch(fallback.rectTransform);
            }
            ParticleSystem particles = CreateParticles("CorrectParticles", pieceObject.transform, 18, 0.8f);
            MapPuzzlePiece piece = pieceObject.AddComponent<MapPuzzlePiece>();
            piece.ConfigureRuntime(controller, targets[i], effectsSource, pickup, correct, wrong, particles, sprite);
            result[i] = piece;
        }
        return result;
    }

    private static GameObject CreateUI(string name, Transform parent)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        return item;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject panel = CreateUI(name, parent);
        SetRect(panel.GetComponent<RectTransform>(), anchor, Vector2.zero, size);
        AddImage(panel, color);
        return panel;
    }

    private static Image AddImage(GameObject item, Color color)
    {
        Image image = item.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float size, Color color, FontStyles style)
    {
        GameObject item = CreateUI(name, parent);
        TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, AudioSource source, AudioClip hover, AudioClip click)
    {
        GameObject item = CreatePanel(name, parent, anchor, size, new Color(0.20f, 0.078f, 0.018f, 1f));
        item.GetComponent<RectTransform>().anchoredPosition = position;
        AddOutline(item, Gold, 2f);
        Button button = item.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.80f, 0.40f, 1f);
        colors.pressedColor = new Color(0.72f, 0.50f, 0.20f, 1f);
        button.colors = colors;
        TMP_Text text = CreateText("Text", item.transform, label, 24f, Gold, FontStyles.Bold);
        Stretch(text.rectTransform);
        item.AddComponent<Challenge07ButtonAudio>().Configure(source, hover, click);
        return button;
    }

    private static ParticleSystem CreateParticles(string name, Transform parent, int count, float lifetime)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(ParticleSystem));
        item.transform.SetParent(parent, false);
        ParticleSystem particles = item.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.4f;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(40f, 105f);
        main.startSize = new ParticleSystem.MinMaxCurve(5f, 12f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.58f, 0.08f), new Color(1f, 0.95f, 0.45f));
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 38f;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return particles;
    }

    private static void AddOutline(GameObject item, Color color, float size)
    {
        Outline outline = item.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(size, -size);
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
