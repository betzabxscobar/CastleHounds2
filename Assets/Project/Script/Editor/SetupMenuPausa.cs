using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEditor;

public class SetupMenuPausa
{
    [MenuItem("CastleHounds/Instalar Menu Pausa")]
    public static void Instalar()
    {
        if (EditorUtility.DisplayDialog(
            "Instalar Menu de Pausa",
            "Esto creara un Canvas_Pausa en la escena actual con todos los paneles y botones configurados.\n\nContinuar?",
            "Si", "Cancelar"))
        {
            CrearCanvasPausa();
        }
    }

    private static void CrearCanvasPausa()
    {
        GameObject canvasPausa = new GameObject("Canvas_Pausa");
        Canvas canvas = canvasPausa.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasPausa.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasPausa.AddComponent<GraphicRaycaster>();

        PauseMenu pauseMenu = canvasPausa.AddComponent<PauseMenu>();
        canvasPausa.AddComponent<PlayerControlLock>();

        GameObject panelPausa = CrearPanel("PanelPausa", canvasPausa.transform, true);
        Image bgPausa = panelPausa.AddComponent<Image>();
        bgPausa.color = new Color(0, 0, 0, 0.6f);
        AnclarFill(panelPausa.GetComponent<RectTransform>());

        GameObject panelMenu = CrearPanel("PanelMenu", canvasPausa.transform, true);
        panelMenu.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        panelMenu.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        panelMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 700);

        GameObject panelOpciones = CrearPanel("PanelOpciones", canvasPausa.transform, false);
        panelOpciones.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        panelOpciones.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        panelOpciones.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 600);

        GameObject panelControles = CrearPanel("PanelControles", canvasPausa.transform, false);
        panelControles.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        panelControles.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        panelControles.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 600);

        CrearTitulo("Img_Titulo", panelMenu.transform, "UI/Titulos/Titulo_Pausa");
        CrearBoton("Btn_Continuar", panelMenu.transform, "UI/Botones/Btn_Continuar", 120, pauseMenu, "ContinuarJuego");
        CrearBoton("Btn_Opciones", panelMenu.transform, "UI/Botones/Btn_Opciones", 20, pauseMenu, "AbrirOpciones");
        CrearBoton("Btn_Controles", panelMenu.transform, "UI/Botones/Btn_Volver", -80, pauseMenu, "AbrirControles");
        CrearBoton("Btn_MenuPrincipal", panelMenu.transform, "UI/Botones/Btn_MenuPrincipal", -180, pauseMenu, "IrMenuPrincipal");

        CrearTitulo("Img_Titulo_Op", panelOpciones.transform, "UI/Titulos/Titulo_Opciones");
        CrearBoton("BtnMusica", panelOpciones.transform, "UI/Botones/BtnMusica", 80, pauseMenu, "CambiarMusica");
        CrearBoton("BtnSonidos", panelOpciones.transform, "UI/Botones/BtnSonidos", 0, pauseMenu, "CambiarSonidos");
        CrearBoton("BtnPantallaCompleta", panelOpciones.transform, "UI/Botones/BtnPantallaCompleta", -80, pauseMenu, "CambiarPantallaCompleta");
        CrearBoton("Btn_Volver_Op", panelOpciones.transform, "UI/Botones/Btn_Volver", -180, pauseMenu, "VolverMenu");

        CrearTextoControles(panelControles.transform);
        CrearBoton("Btn_Volver_Cont", panelControles.transform, "UI/Botones/Btn_Volver", -220, pauseMenu, "VolverMenu");

        pauseMenu.panelPausa = panelPausa;
        pauseMenu.panelMenu = panelMenu;
        pauseMenu.panelOpciones = panelOpciones;
        pauseMenu.panelControles = panelControles;

        panelPausa.SetActive(false);
        panelMenu.SetActive(false);
        panelOpciones.SetActive(false);
        panelControles.SetActive(false);

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        Debug.Log("[CastleHounds] Menu de Pausa instalado correctamente.");
        EditorUtility.DisplayDialog("Listo", "Menu de Pausa instalado en la escena.\n\nRecuerda:\n1. Asignar PlayerController al PlayerControlLock\n2. Asignar los sprites en los botones si no se cargaron", "OK");
    }

    private static GameObject CrearPanel(string nombre, Transform padre, bool activo)
    {
        GameObject panel = new GameObject(nombre);
        panel.layer = 5;
        panel.transform.SetParent(padre, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        panel.AddComponent<CanvasGroup>();
        panel.SetActive(activo);
        return panel;
    }

    private static void CrearTitulo(string nombre, Transform padre, string spritePath)
    {
        GameObject titulo = new GameObject(nombre);
        titulo.layer = 5;
        titulo.transform.SetParent(padre, false);
        RectTransform rt = titulo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -80);
        rt.sizeDelta = new Vector2(500, 150);
        titulo.AddComponent<CanvasRenderer>();
        Image img = titulo.AddComponent<Image>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project/" + spritePath + ".png");
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        img.raycastTarget = false;
    }

    private static void CrearBoton(string nombre, Transform padre, string spritePath, float yOffset, PauseMenu pauseMenu, string metodo)
    {
        GameObject btn = new GameObject(nombre);
        btn.layer = 5;
        btn.transform.SetParent(padre, false);
        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yOffset);
        rt.sizeDelta = new Vector2(400, 80);
        btn.AddComponent<CanvasRenderer>();
        Image img = btn.AddComponent<Image>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project/" + spritePath + ".png");
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        Button button = btn.AddComponent<Button>();
        button.targetGraphic = img;

        ConfigurarOnClick(button, pauseMenu, metodo);

        btn.AddComponent<AlphaButtonClick>();
    }

    private static void ConfigurarOnClick(Button button, PauseMenu pauseMenu, string metodo)
    {
        SerializedObject so = new SerializedObject(button);
        SerializedProperty calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.InsertArrayElementAtIndex(0);
        SerializedProperty call = calls.GetArrayElementAtIndex(0);

        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[0].m_Target").objectReferenceValue = pauseMenu;
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[0].m_TargetAssemblyTypeName").stringValue = "PauseMenu, Assembly-CSharp";
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[0].m_MethodName").stringValue = metodo;
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[0].m_Mode").intValue = 1;
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[0].m_Arguments.m_ObjectArgumentAssemblyTypeName").stringValue = "UnityEngine.Object, UnityEngine";
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[0].m_CallState").intValue = 2;

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CrearTextoControles(Transform padre)
    {
        GameObject txt = new GameObject("Txt_Info");
        txt.layer = 5;
        txt.transform.SetParent(padre, false);
        RectTransform rt = txt.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta = new Vector2(-40, -120);
        txt.AddComponent<CanvasRenderer>();

        var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        if (tmpType != null)
        {
            var tmp = (MonoBehaviour)txt.AddComponent(tmpType);
            var textProp = tmpType.GetProperty("text");
            if (textProp != null)
                textProp.SetValue(tmp, "WASD - Mover\nShift - Correr\nClick Izq - Atacar\nEscape - Pausa");
            var sizeProp = tmpType.GetProperty("fontSize");
            if (sizeProp != null)
                sizeProp.SetValue(tmp, 36f);
        }
        else
        {
            Text legacyText = txt.AddComponent<Text>();
            legacyText.text = "WASD - Mover\nShift - Correr\nClick Izq - Atacar\nEscape - Pausa";
            legacyText.fontSize = 36;
            legacyText.alignment = TextAnchor.MiddleCenter;
            legacyText.color = Color.white;
        }
    }

    private static void AnclarFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
