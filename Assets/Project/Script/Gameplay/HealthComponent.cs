using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private UnityEvent<int> alRecibirDano;
    [SerializeField] private UnityEvent alMorir;

    private int _vidaActual;

    private void Awake()
    {
        _vidaActual = vidaMaxima;
    }

    public void RecibirDano(int cantidad)
    {
        if (_vidaActual <= 0) return;

        _vidaActual = Mathf.Max(0, _vidaActual - cantidad);
        alRecibirDano?.Invoke(_vidaActual);

        if (_vidaActual <= 0)
        {
            alMorir?.Invoke();
        }
    }

    public int VidaActual => _vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool Vida => _vidaActual > 0;
}