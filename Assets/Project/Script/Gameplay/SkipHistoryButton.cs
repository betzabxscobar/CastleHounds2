using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class SkipHistoryButton : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Demo";
    [SerializeField] private IntroManager introManager;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField, Min(0f)] private float transitionDelay;
    [SerializeField] private CanvasGroup fadePanel;

    private bool isSkipping;

    public void SkipHistory()
    {
        if (isSkipping)
        {
            return;
        }

        isSkipping = true;
        Time.timeScale = 1f;

        if (playableDirector != null)
        {
            playableDirector.Stop();
        }

        if (introManager != null)
        {
            introManager.CancelIntro();
        }

        StopAllCoroutines();
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            fadePanel.interactable = false;
        }

        if (transitionDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(transitionDelay);
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("SkipHistoryButton no tiene configurada la escena siguiente.", this);
            isSkipping = false;
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
