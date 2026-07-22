using UnityEngine;

public sealed class Challenge07Audio : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip hintSound;
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip victorySound;

    public AudioSource EffectsSource => effectsSource;
    public AudioClip PickupSound => pickupSound;
    public AudioClip CorrectSound => correctSound;
    public AudioClip WrongSound => wrongSound;
    public AudioClip HintSound => hintSound;
    public AudioClip ButtonSound => buttonSound;
    public AudioClip VictorySound => victorySound;

    public void Configure(AudioSource music, AudioSource effects, AudioClip ambient, AudioClip pickup, AudioClip correct, AudioClip wrong, AudioClip hint, AudioClip button, AudioClip victory)
    {
        musicSource = music;
        effectsSource = effects;
        ambientMusic = ambient;
        pickupSound = pickup;
        correctSound = correct;
        wrongSound = wrong;
        hintSound = hint;
        buttonSound = button;
        victorySound = victory;
    }

    private void Awake()
    {
        if (effectsSource == null) effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        pickupSound ??= CreateTone("PickupFallback", 520f, 0.09f, 0.12f);
        correctSound ??= CreateTone("CorrectFallback", 760f, 0.14f, 0.14f);
        wrongSound ??= CreateTone("WrongFallback", 190f, 0.16f, 0.12f);
        hintSound ??= CreateTone("HintFallback", 960f, 0.12f, 0.10f);
        buttonSound ??= CreateTone("ButtonFallback", 620f, 0.06f, 0.08f);
        victorySound ??= CreateTone("VictoryFallback", 880f, 0.35f, 0.15f);
    }

    public void PlayEffect(AudioClip clip)
    {
        if (effectsSource != null && clip != null) effectsSource.PlayOneShot(clip);
    }

    private static AudioClip CreateTone(string clipName, float frequency, float duration, float volume)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float fade = 1f - i / (float)sampleCount;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * volume * fade;
        }
        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
