using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip sonidoClick;

    private AudioSource musicaSource;
    private AudioSource sfxSource;

    private bool musicaActiva = true;
    private bool sonidosActivos = true;

    public bool SonidosActivos => sonidosActivos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicaSource = GetComponent<AudioSource>();

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        if (musicaSource.clip != null && !musicaSource.isPlaying)
        {
            musicaSource.loop = true;
            musicaSource.Play();
        }
    }

    public void ReproducirClick()
    {
        if (sonidosActivos && sonidoClick != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(sonidoClick);
        }
    }

    public void ToggleMusica()
    {
        musicaActiva = !musicaActiva;

        if (musicaSource == null) return;

        if (musicaActiva)
        {
            musicaSource.UnPause();
        }
        else
        {
            musicaSource.Pause();
        }
    }

    public void ToggleSonidos()
    {
        sonidosActivos = !sonidosActivos;
    }
}
