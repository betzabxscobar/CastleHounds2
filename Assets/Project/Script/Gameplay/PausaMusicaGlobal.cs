using UnityEngine;

public class PausaMusicaGlobal : MonoBehaviour
{
    private void OnEnable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PausarMusica();
        }
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReanudarMusica();
        }
    }
}
