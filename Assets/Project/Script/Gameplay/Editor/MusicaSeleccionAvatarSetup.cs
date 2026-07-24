#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MusicaSeleccionAvatarSetup
{
    private const string ScenePath = "Assets/Project/Scenes/Menus/Frontend/SeleccionAvatar.unity";
    private const string MusicFolder = "Assets/Project/Audios/Music";
    private const string ClipName = "valorant-ace-sound";
    private const string ObjetoNombre = "MusicaSeleccionAvatar";
    private static readonly string[] ExtensionesValidas = { ".mp3", ".wav", ".ogg", ".aiff", ".aif" };

    [MenuItem("Castle Hounds/Configurar Música Selección Avatar")]
    public static void Configurar()
    {
        if (!AsegurarEscenaAbierta(out Scene scene))
        {
            return;
        }

        StringBuilder reporte = new StringBuilder();

        AudioClip clip = BuscarClip();
        if (clip == null)
        {
            Debug.LogError($"No se encontro el AudioClip '{ClipName}' en {MusicFolder}");
            return;
        }
        reporte.AppendLine($"- AudioClip encontrado: {AssetDatabase.GetAssetPath(clip)}");

        GameObject musica = BuscarEnEscena(scene, ObjetoNombre);
        bool creado = musica == null;
        if (musica == null)
        {
            musica = new GameObject(ObjetoNombre);
        }
        musica.SetActive(true);
        reporte.AppendLine(creado
            ? $"- Objeto '{ObjetoNombre}' creado (sin DontDestroyOnLoad, local a esta escena)."
            : $"- Objeto '{ObjetoNombre}' ya existia, reutilizado.");

        AudioSource[] fuentes = musica.GetComponents<AudioSource>();
        AudioSource fuente;
        if (fuentes.Length == 0)
        {
            fuente = musica.AddComponent<AudioSource>();
        }
        else
        {
            fuente = fuentes[0];
            for (int i = 1; i < fuentes.Length; i++)
            {
                Debug.LogWarning($"Se elimino un AudioSource duplicado en '{ObjetoNombre}'.");
                Object.DestroyImmediate(fuentes[i]);
            }
        }

        fuente.clip = clip;
        fuente.playOnAwake = true;
        fuente.loop = true;
        fuente.spatialBlend = 0f;
        fuente.volume = 0.20f;
        fuente.mute = false;
        fuente.bypassEffects = false;
        fuente.priority = 128;
        fuente.pitch = 1f;
        fuente.dopplerLevel = 0f;
        fuente.reverbZoneMix = 0f;
        EditorUtility.SetDirty(fuente);

        reporte.AppendLine($"- AudioSource configurado en '{ObjetoNombre}' (clip={clip.name}, loop=true, playOnAwake=true, volume=0.20, spatialBlend=0).");

        if (musica.GetComponent<PausaMusicaGlobal>() == null)
        {
            musica.AddComponent<PausaMusicaGlobal>();
        }
        reporte.AppendLine("- PausaMusicaGlobal asegurado: pausa la musica del AudioManager global mientras esta escena esta activa y la reanuda al salir, para evitar superposicion.");

        int listenersDesactivados = RevisarAudioListeners(scene, reporte);
        reporte.AppendLine($"- AudioListener duplicados desactivados: {listenersDesactivados}.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        reporte.AppendLine($"- Escena guardada: {scene.path}.");

        Debug.Log($"Configurar Música Selección Avatar completado:\n{reporte}");
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

    private static AudioClip BuscarClip()
    {
        string ruta = Directory.GetFiles(MusicFolder)
            .Where(path => ExtensionesValidas.Contains(Path.GetExtension(path).ToLowerInvariant()))
            .Where(path => Path.GetFileNameWithoutExtension(path) == ClipName)
            .Select(path => path.Replace('\\', '/'))
            .FirstOrDefault();

        return string.IsNullOrEmpty(ruta) ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
    }

    private static GameObject BuscarEnEscena(Scene scene, string nombre)
    {
        foreach (GameObject raiz in scene.GetRootGameObjects())
        {
            GameObject encontrado = BuscarEnHijos(raiz.transform, nombre);
            if (encontrado != null)
            {
                return encontrado;
            }
        }
        return null;
    }

    private static GameObject BuscarEnHijos(Transform actual, string nombre)
    {
        if (actual.name == nombre)
        {
            return actual.gameObject;
        }
        for (int i = 0; i < actual.childCount; i++)
        {
            GameObject encontrado = BuscarEnHijos(actual.GetChild(i), nombre);
            if (encontrado != null)
            {
                return encontrado;
            }
        }
        return null;
    }

    private static int RevisarAudioListeners(Scene scene, StringBuilder reporte)
    {
        AudioListener[] listeners = Object
            .FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(listener => listener.gameObject.scene == scene)
            .ToArray();

        if (listeners.Length == 0)
        {
            reporte.AppendLine("- No se encontro ningun AudioListener en la escena (revisa que SelectionCamera tenga uno).");
            return 0;
        }

        AudioListener preferido = listeners.FirstOrDefault(l => l.gameObject.name == "SelectionCamera") ?? listeners[0];
        if (!preferido.enabled)
        {
            preferido.enabled = true;
            EditorUtility.SetDirty(preferido);
        }

        int desactivados = 0;
        foreach (AudioListener listener in listeners)
        {
            if (listener == preferido)
            {
                continue;
            }
            if (listener.enabled)
            {
                listener.enabled = false;
                EditorUtility.SetDirty(listener);
                desactivados++;
                Debug.LogWarning($"AudioListener duplicado desactivado en '{listener.gameObject.name}'.");
            }
        }

        reporte.AppendLine($"- AudioListener activo: '{preferido.gameObject.name}'. Total encontrados en la escena: {listeners.Length}.");
        return desactivados;
    }
}
#endif
