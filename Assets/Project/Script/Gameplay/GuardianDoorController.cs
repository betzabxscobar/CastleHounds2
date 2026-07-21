using UnityEngine;

public class GuardianDoorController : MonoBehaviour
{
    [SerializeField] private Collider bloqueoCollider;

    private bool _estaBloqueada = true;

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
}