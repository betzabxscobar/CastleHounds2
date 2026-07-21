using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip musica;


    [Range(0f, 1f)]
    public float volumen = 0.4f;



    void Start()
    {
        if (audioSource != null && musica != null)
        {
            audioSource.clip = musica;
            audioSource.volume = volumen;
            audioSource.loop = true;

            audioSource.Play();
        }
    }



    public void DetenerMusica()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}