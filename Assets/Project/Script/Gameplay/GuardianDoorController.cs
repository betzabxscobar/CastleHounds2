using UnityEngine;

public class GuardianDoorController : MonoBehaviour
{
    [SerializeField] private Collider bloqueoCollider;
    [SerializeField] private Collider zonaDeteccion;
    [SerializeField] private float anguloApertura = 90f;
    [SerializeField] private float velocidadApertura = 2f;

    private bool _estaBloqueada = true;
    private bool _abriendo;
    private Quaternion _rotacionInicial;
    private Quaternion _rotacionFinal;

    private const string TagJugador = "Player";

    private void Awake()
    {
        if (bloqueoCollider == null)
        {
            bloqueoCollider = GetComponent<Collider>();
        }

        _rotacionInicial = transform.rotation;
        _rotacionFinal = _rotacionInicial * Quaternion.Euler(0, anguloApertura, 0);

        BloquearPuerta();
    }

    private void Update()
    {
        if (!_abriendo) return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            _rotacionFinal,
            velocidadApertura * Time.deltaTime * 90f);

        if (Quaternion.Angle(transform.rotation, _rotacionFinal) < 0.5f)
        {
            transform.rotation = _rotacionFinal;
            _abriendo = false;
        }
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

    public void UnlockDoor()
    {
        _estaBloqueada = false;

        if (bloqueoCollider != null)
        {
            bloqueoCollider.enabled = false;
        }

        _abriendo = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagJugador)) return;

        if (_estaBloqueada)
        {
            Debug.Log("La puerta está sellada por el guardián.");
        }
    }
}