using UnityEngine;

public class GuardianDoorController : MonoBehaviour
{
    [SerializeField] private Collider bloqueoCollider;
    [SerializeField] private Collider zonaDeteccion;

    private bool _estaBloqueada = true;

    private const string TagJugador = "Player";

    private void Awake()
    {
        if (bloqueoCollider == null)
        {
            bloqueoCollider = GetComponent<Collider>();
        }

        BloquearPuerta();
    }

    public void AbrirPuerta()
    {
        _estaBloqueada = false;
        
        if (bloqueoCollider != null)
        {
            bloqueoCollider.enabled = false;
        }
    }

    public void BloquearPuerta()
    {
        _estaBloqueada = true;
        
        if (bloqueoCollider != null)
        {
            bloqueoCollider.enabled = true;
        }
    }

    public bool EstaBloqueada => _estaBloqueada;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagJugador)) return;

        if (_estaBloqueada)
        {
            Debug.Log("La puerta está sellada por el guardián.");
        }
    }
}