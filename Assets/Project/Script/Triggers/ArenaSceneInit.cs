using UnityEngine;

/// <summary>
/// Este script va en la escena "_Demo Scene" (la arena de batalla).
/// Dispara el inicio de pelea automaticamente al cargar esa escena.
/// </summary>
public class ArenaSceneInit : MonoBehaviour
{
    [SerializeField] private float retrasoSegundos = 0f;

    private void Start()
    {
        if (retrasoSegundos <= 0f)
        {
            GameEvents.RaiseFightStart();
        }
        else
        {
            Invoke(nameof(DispararInicioPelea), retrasoSegundos);
        }
    }

    private void DispararInicioPelea()
    {
        GameEvents.RaiseFightStart();
    }
}
