using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class Challenge07ButtonAudio : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClick);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(PlayClick);
    }

    public void Configure(AudioSource source, AudioClip hover, AudioClip click)
    {
        audioSource = source;
        hoverClip = hover;
        clickClip = click;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && hoverClip != null) audioSource.PlayOneShot(hoverClip);
    }

    private void PlayClick()
    {
        if (audioSource != null && clickClip != null) audioSource.PlayOneShot(clickClip);
    }
}
