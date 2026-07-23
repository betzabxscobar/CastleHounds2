using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IntroManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public VideoClip[] videos;

    [Header("Fade")]
    public CanvasGroup negro;
    public float duracionFade = 1f;

    [Tooltip("Segundos antes de terminar el video en los que comienza el fade.")]
    public float empezarFadeAntes = 1f;

    [Header("Escena")]
    public string siguienteNivel = "Demo";

    private int indice = 0;
    private bool cambiando = false;

    void Start()
    {
        if (videos.Length == 0)
        {
            Debug.LogError("No hay videos asignados.");
            return;
        }

        negro.alpha = 0;

        videoPlayer.loopPointReached += VideoTerminado;

        ReproducirVideo();
    }

    void Update()
    {
        if (EnterPresionado() && !cambiando)
        {
            StartCoroutine(CambiarVideo());
        }

        if (!cambiando &&
            videoPlayer.clip != null &&
            videoPlayer.isPlaying)
        {
            double restante = videoPlayer.length - videoPlayer.time;

            if (restante <= empezarFadeAntes)
            {
                StartCoroutine(CambiarVideo());
            }
        }
    }

    void ReproducirVideo()
    {
        videoPlayer.Stop();
        videoPlayer.clip = videos[indice];
        videoPlayer.isLooping = false;
        videoPlayer.Play();
    }

    void VideoTerminado(VideoPlayer vp)
    {
        if (!cambiando)
            StartCoroutine(CambiarVideo());
    }

    IEnumerator CambiarVideo()
    {
        cambiando = true;

        yield return StartCoroutine(Fade(0, 1));

        indice++;

        if (indice >= videos.Length)
        {
            SceneManager.LoadScene(siguienteNivel);
            yield break;
        }

        negro.alpha = 1;

        ReproducirVideo();

        yield return StartCoroutine(Fade(1, 0));

        cambiando = false;
    }

    IEnumerator Fade(float inicio, float fin)
    {
        float t = 0;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            negro.alpha = Mathf.Lerp(inicio, fin, t / duracionFade);
            yield return null;
        }

        negro.alpha = fin;
    }

    bool EnterPresionado()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb != null &&
            (kb.enterKey.wasPressedThisFrame ||
             kb.numpadEnterKey.wasPressedThisFrame))
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.KeypadEnter);
#else
        return false;
#endif
    }
}