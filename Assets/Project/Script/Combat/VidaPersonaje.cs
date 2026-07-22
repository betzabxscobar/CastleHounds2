using UnityEngine;
using UnityEngine.Events;

public class VidaPersonaje : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private int vidaActual;
    [SerializeField] private bool esEnemigo;

    public UnityEvent<int> OnVidaCambiada;
    public UnityEvent OnMuerte;

    private bool muerteInvocada;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool EsEnemigo => esEnemigo;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDano(int cantidad)
    {
        if (cantidad <= 0 || !EstaVivo())
        {
            return;
        }

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0, vidaMaxima);
        OnVidaCambiada?.Invoke(vidaActual);

        if (vidaActual == 0 && !muerteInvocada)
        {
            muerteInvocada = true;
            OnMuerte?.Invoke();
        }
    }

    public bool EstaVivo()
    {
        return vidaActual > 0;
    }
}
