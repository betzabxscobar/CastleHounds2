using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private VidaPersonaje vidaJugador;
    [SerializeField] private VidaPersonaje vidaEnemigo;

    private void OnEnable()
    {
        GameEvents.OnFightStart += ManejarInicioPelea;

        if (vidaJugador != null)
        {
            vidaJugador.OnMuerte.AddListener(ManejarMuerteJugador);
        }

        if (vidaEnemigo != null)
        {
            vidaEnemigo.OnMuerte.AddListener(ManejarMuerteEnemigo);
        }
    }

    private void OnDisable()
    {
        GameEvents.OnFightStart -= ManejarInicioPelea;

        if (vidaJugador != null)
        {
            vidaJugador.OnMuerte.RemoveListener(ManejarMuerteJugador);
        }

        if (vidaEnemigo != null)
        {
            vidaEnemigo.OnMuerte.RemoveListener(ManejarMuerteEnemigo);
        }
    }

    private void ManejarInicioPelea()
    {
        Debug.Log("Pelea iniciada");
    }

    private void ManejarMuerteEnemigo()
    {
        GameEvents.RaiseEnemyDefeated();
        Debug.Log("Victoria: enemigo derrotado");
    }

    private void ManejarMuerteJugador()
    {
        Debug.Log("Derrota: el jugador murió");
        // TODO: conectar pantalla de derrota cuando exista.
    }

    public void DanoAlEnemigo(int cantidad)
    {
        if (vidaEnemigo != null)
        {
            vidaEnemigo.RecibirDano(cantidad);
        }
    }

    public void DanoAlJugador(int cantidad)
    {
        if (vidaJugador != null)
        {
            vidaJugador.RecibirDano(cantidad);
        }
    }
}
