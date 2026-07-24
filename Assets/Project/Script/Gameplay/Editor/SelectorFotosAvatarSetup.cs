#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SelectorFotosAvatarSetup
{
    private const string IconsFolder = "Assets/Project/UI/Icons";
    private const string ScenePath = "Assets/Project/Scenes/Menus/Frontend/SeleccionAvatar.unity";
    private static readonly string[] ExtensionesValidas = { ".png", ".jpg", ".jpeg" };

    [MenuItem("Castle Hounds/Configurar Selector de Fotos")]
    public static void Configurar()
    {
        if (!AsegurarEscenaAbierta(out Scene scene))
        {
            return;
        }

        StringBuilder reporte = new StringBuilder();

        AvatarSelector avatarSelector = UnityEngine.Object.FindFirstObjectByType<AvatarSelector>(FindObjectsInactive.Include);
        TMPro.TMP_Text nombreExistente = null;
        Image barraVidaExistente = null;
        Image barraAtaqueExistente = null;
        Image barraVelocidadExistente = null;
        AvatarSelector.AvatarPresentation[] presentacionesExistentes = null;
        Transform spawnPoint = null;

        if (avatarSelector != null)
        {
            spawnPoint = avatarSelector.avatarSpawnPoint;
            nombreExistente = avatarSelector.avatarNombre;
            barraVidaExistente = avatarSelector.vidaBarra;
            barraAtaqueExistente = avatarSelector.ataqueBarra;
            barraVelocidadExistente = avatarSelector.velocidadBarra;
            presentacionesExistentes = avatarSelector.presentaciones;

            int eliminados = EliminarPreviewCentral(avatarSelector);
            avatarSelector.enabled = false;
            EditorUtility.SetDirty(avatarSelector);

            reporte.AppendLine($"- Script que creaba el avatar 3D central: AvatarSelector ({AbsolutePathOf(avatarSelector)}), metodo ActualizarModelo() -> Instantiate(presentacion.modeloPreview, avatarSpawnPoint).");
            reporte.AppendLine($"- Componente 'AvatarSelector' desactivado (enabled = false). Instancias de preview central eliminadas: {eliminados}.");
        }
        else
        {
            reporte.AppendLine("- No se encontro ningun componente AvatarSelector en la escena (nada que desactivar).");
        }

        GameObject punto = ConfigurarJerarquiaFoto(spawnPoint, out GameObject canvasGO, out Image fotoImage);
        reporte.AppendLine($"- Jerarquia asegurada: {punto.name}/{canvasGO.name}/{fotoImage.gameObject.name}.");

        string[] rutas = ObtenerRutasAvatares();
        if (rutas.Length == 0)
        {
            Debug.LogError($"No se encontraron imagenes 'Avatar*' en {IconsFolder}");
            return;
        }

        foreach (string ruta in rutas)
        {
            ConfigurarImportadorComoSprite(ruta);
        }
        AssetDatabase.Refresh();

        Sprite[] sprites = rutas
            .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
            .Where(sprite => sprite != null)
            .OrderBy(sprite => sprite.name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        SelectorFotosAvatar selector = punto.GetComponent<SelectorFotosAvatar>();
        if (selector == null)
        {
            selector = punto.AddComponent<SelectorFotosAvatar>();
        }

        SerializedObject serializedSelector = new SerializedObject(selector);
        serializedSelector.FindProperty("imagenCentral").objectReferenceValue = fotoImage;
        serializedSelector.FindProperty("textoNombre").objectReferenceValue = nombreExistente;
        serializedSelector.FindProperty("barraVida").objectReferenceValue = barraVidaExistente;
        serializedSelector.FindProperty("barraAtaque").objectReferenceValue = barraAtaqueExistente;
        serializedSelector.FindProperty("barraVelocidad").objectReferenceValue = barraVelocidadExistente;

        SerializedProperty avataresProp = serializedSelector.FindProperty("avatares");
        avataresProp.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            string nombre = FormatearNombre(sprites[i].name);
            AvatarSelector.AvatarPresentation presentacion = BuscarPresentacion(presentacionesExistentes, nombre);

            SerializedProperty elemento = avataresProp.GetArrayElementAtIndex(i);
            elemento.FindPropertyRelative("nombre").stringValue = nombre;
            elemento.FindPropertyRelative("foto").objectReferenceValue = sprites[i];
            elemento.FindPropertyRelative("vida").floatValue = presentacion != null ? presentacion.vida : 0.75f;
            elemento.FindPropertyRelative("ataque").floatValue = presentacion != null ? presentacion.ataque : 0.65f;
            elemento.FindPropertyRelative("velocidad").floatValue = presentacion != null ? presentacion.velocidad : 0.8f;
        }
        serializedSelector.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(selector);

        int botonesReconectados = ReconectarBotones(avatarSelector, selector);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        reporte.AppendLine($"- Fotografias 'Avatar*' encontradas y configuradas como Sprite/Single: {sprites.Length} ({string.Join(", ", sprites.Select(s => s.name))}).");
        reporte.AppendLine($"- Botones reconectados a SelectorFotosAvatar: {botonesReconectados}.");
        reporte.AppendLine($"- Escena guardada: {scene.path}.");

        Debug.Log($"Configurar Selector de Fotos completado:\n{reporte}");
    }

    private static bool AsegurarEscenaAbierta(out Scene scene)
    {
        scene = SceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded)
        {
            return true;
        }

        Scene activa = EditorSceneManager.GetActiveScene();
        if (activa.isDirty)
        {
            bool continuar = EditorUtility.DisplayDialog(
                "Escena sin guardar",
                $"La escena abierta '{activa.name}' tiene cambios sin guardar.\n¿Guardarla antes de abrir SeleccionAvatar?",
                "Guardar y continuar",
                "Cancelar");

            if (!continuar)
            {
                return false;
            }

            EditorSceneManager.SaveOpenScenes();
        }

        scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        return scene.IsValid();
    }

    private static int EliminarPreviewCentral(AvatarSelector avatarSelector)
    {
        int eliminados = 0;

        if (avatarSelector.modeloInicialEnEscena != null)
        {
            UnityEngine.Object.DestroyImmediate(avatarSelector.modeloInicialEnEscena);
            avatarSelector.modeloInicialEnEscena = null;
            eliminados++;
        }

        if (avatarSelector.avatarSpawnPoint != null)
        {
            Transform spawn = avatarSelector.avatarSpawnPoint;
            for (int i = spawn.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(spawn.GetChild(i).gameObject);
                eliminados++;
            }
        }

        return eliminados;
    }

    private static GameObject ConfigurarJerarquiaFoto(Transform spawnPoint, out GameObject canvasGO, out Image fotoImage)
    {
        Transform parenteNoRotante = spawnPoint != null ? spawnPoint.parent : null;
        Vector3 posicionMundo = spawnPoint != null
            ? spawnPoint.position + new Vector3(0f, 1.6f, 0f)
            : new Vector3(0f, 2.1f, 1.1f);

        GameObject punto = GameObject.Find("PuntoFotoAvatar");
        if (punto == null)
        {
            punto = new GameObject("PuntoFotoAvatar");
        }
        punto.SetActive(true);

        if (parenteNoRotante != null && punto.transform.parent != parenteNoRotante)
        {
            punto.transform.SetParent(parenteNoRotante, false);
        }
        punto.transform.position = posicionMundo;
        punto.transform.rotation = Quaternion.identity;

        canvasGO = ObtenerOCrearHijo(punto.transform, "CanvasFotoAvatar",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.SetActive(true);

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasGO.AddComponent<Canvas>();
        }
        if (canvasGO.GetComponent<CanvasScaler>() == null)
        {
            canvasGO.AddComponent<CanvasScaler>();
        }
        if (canvasGO.GetComponent<GraphicRaycaster>() == null)
        {
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 20;

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400f, 550f);
        canvasRect.localScale = new Vector3(0.003f, 0.003f, 0.003f);
        canvasRect.localPosition = Vector3.zero;

        Camera camara = Camera.main;
        if (camara == null)
        {
            GameObject camaraGO = GameObject.Find("SelectionCamera");
            if (camaraGO != null)
            {
                camara = camaraGO.GetComponent<Camera>();
            }
        }
        if (camara != null)
        {
            canvasGO.transform.rotation = camara.transform.rotation;
        }

        fotoImage = ObtenerOCrearFotoSeleccionada(canvasGO.transform);

        // Asegura que ningun padre este desactivado ni con CanvasGroup en alpha 0.
        for (Transform actual = fotoImage.transform; actual != null; actual = actual.parent)
        {
            actual.gameObject.SetActive(true);
            CanvasGroup grupo = actual.GetComponent<CanvasGroup>();
            if (grupo != null)
            {
                grupo.alpha = 1f;
                grupo.interactable = true;
                grupo.blocksRaycasts = true;
            }

            if (actual == punto.transform)
            {
                break;
            }
        }

        return punto;
    }

    private static string[] ObtenerRutasAvatares()
    {
        return Directory.GetFiles(IconsFolder)
            .Where(path => ExtensionesValidas.Contains(Path.GetExtension(path).ToLowerInvariant()))
            .Where(path => Path.GetFileNameWithoutExtension(path).StartsWith("Avatar", StringComparison.Ordinal))
            .Select(path => path.Replace('\\', '/'))
            .OrderBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void ConfigurarImportadorComoSprite(string ruta)
    {
        TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool cambios = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            cambios = true;
        }
        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            cambios = true;
        }

        if (cambios)
        {
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }

    private static GameObject ObtenerOCrearHijo(Transform padre, string nombre, params Type[] componentes)
    {
        Transform existente = padre.Find(nombre);
        if (existente != null)
        {
            return existente.gameObject;
        }

        GameObject hijo = new GameObject(nombre, componentes);
        hijo.transform.SetParent(padre, false);
        return hijo;
    }

    private static Image ObtenerOCrearFotoSeleccionada(Transform canvas)
    {
        Image[] existentes = UnityEngine.Object
            .FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(imagen => imagen.gameObject.name == "FotoSeleccionada")
            .ToArray();

        Image foto;
        if (existentes.Length > 0)
        {
            foto = existentes[0];
            foto.transform.SetParent(canvas, false);
            for (int i = 1; i < existentes.Length; i++)
            {
                Debug.LogWarning("Se elimino una FotoSeleccionada duplicada.");
                UnityEngine.Object.DestroyImmediate(existentes[i].gameObject);
            }
        }
        else
        {
            GameObject go = new GameObject("FotoSeleccionada", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(canvas, false);
            foto = go.GetComponent<Image>();
        }

        RectTransform rect = (RectTransform)foto.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300f, 450f);
        rect.anchoredPosition = Vector2.zero;
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        foto.preserveAspect = true;
        foto.color = Color.white;
        foto.gameObject.SetActive(true);

        return foto;
    }

    private static string FormatearNombre(string nombreSprite)
    {
        return nombreSprite.StartsWith("Avatar", StringComparison.Ordinal)
            ? nombreSprite.Substring("Avatar".Length)
            : nombreSprite;
    }

    private static AvatarSelector.AvatarPresentation BuscarPresentacion(
        AvatarSelector.AvatarPresentation[] presentaciones, string nombreSprite)
    {
        if (presentaciones == null)
        {
            return null;
        }

        string clave = Normalizar(nombreSprite);
        foreach (AvatarSelector.AvatarPresentation presentacion in presentaciones)
        {
            if (presentacion != null && Normalizar(presentacion.nombre) == clave)
            {
                return presentacion;
            }
        }

        return null;
    }

    private static string Normalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return string.Empty;
        }

        return new string(texto.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }

    private static int ReconectarBotones(AvatarSelector avatarSelector, SelectorFotosAvatar selector)
    {
        int reconectados = 0;
        Button[] botones = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button boton in botones)
        {
            if (avatarSelector != null)
            {
                if (ReemplazarListener(boton.onClick, avatarSelector, "AnteriorAvatar", selector.Anterior))
                {
                    reconectados++;
                }
                if (ReemplazarListener(boton.onClick, avatarSelector, "SiguienteAvatar", selector.Siguiente))
                {
                    reconectados++;
                }
                if (ReemplazarListener(boton.onClick, avatarSelector, "SeleccionarAvatar", selector.Seleccionar))
                {
                    reconectados++;
                }
            }
            EditorUtility.SetDirty(boton);
        }

        return reconectados;
    }

    private static bool ReemplazarListener(UnityEventBase evento, UnityEngine.Object objetivoViejo, string metodoViejo, UnityAction nuevoMetodo)
    {
        bool encontrado = false;
        for (int i = evento.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (evento.GetPersistentTarget(i) == objetivoViejo && evento.GetPersistentMethodName(i) == metodoViejo)
            {
                UnityEventTools.RemovePersistentListener(evento, i);
                encontrado = true;
            }
        }

        if (!encontrado)
        {
            return false;
        }

        UnityEngine.Object nuevoObjetivo = nuevoMetodo.Target as UnityEngine.Object;
        bool yaExiste = false;
        for (int i = 0; i < evento.GetPersistentEventCount(); i++)
        {
            if (evento.GetPersistentTarget(i) == nuevoObjetivo && evento.GetPersistentMethodName(i) == nuevoMetodo.Method.Name)
            {
                yaExiste = true;
                break;
            }
        }

        if (!yaExiste)
        {
            UnityEventTools.AddPersistentListener((UnityEvent)evento, nuevoMetodo);
        }

        return true;
    }

    private static string AbsolutePathOf(Component componente)
    {
        Transform actual = componente.transform;
        string ruta = actual.name;
        while (actual.parent != null)
        {
            actual = actual.parent;
            ruta = actual.name + "/" + ruta;
        }
        return ruta;
    }
}
#endif
