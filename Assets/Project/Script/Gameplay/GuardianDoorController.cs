using UnityEngine;

/// <summary>
/// Controla una puerta bloqueada por un guardián.
/// La puerta inicia bloqueada y puede desbloquearse mediante UnlockDoor().
/// </summary>
public class GuardianDoorController : MonoBehaviour
{
    [Header("Colisiones")]
    [SerializeField] private Collider bloqueoCollider;
    [SerializeField] private Collider zonaDeteccion;

    [Header("Puerta")]
    [SerializeField] private Transform objetoVisual;
    [SerializeField] private bool bloqueadoInicial = true;

    [Header("Apertura")]
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

        if (objetoVisual == null)
        {
            objetoVisual = transform;
        }

        _rotacionInicial = objetoVisual.rotation;
        _rotacionFinal = _rotacionInicial * Quaternion.Euler(0, anguloApertura, 0);

        if (bloqueadoInicial)
        {
            BloquearPuerta();
        }
        else
        {
            AbrirPuerta();
        }
    }

    private void Update()
    {
        if (!_abriendo) return;

        objetoVisual.rotation = Quaternion.RotateTowards(
            objetoVisual.rotation,
            _rotacionFinal,
            velocidadApertura * Time.deltaTime * 90f);

        if (Quaternion.Angle(objetoVisual.rotation, _rotacionFinal) < 0.5f)
        {
            objetoVisual.rotation = _rotacionFinal;
            _abriendo = false;
        }
    }

    // ──────────────────────────────────────────────
    //  Métodos públicos
    // ──────────────────────────────────────────────

    /// <summary>
    /// Desactiva el collider de bloqueo sin animación.
    /// </summary>
    public void AbrirPuerta()
    {
        _estaBloqueada = false;

        if (bloqueoCollider != null)
        {
            bloqueoCollider.enabled = false;
        }
    }

    /// <summary>
    /// Activa el collider de bloqueo.
    /// </summary>
    public void BloquearPuerta()
    {
        _estaBloqueada = true;

        if (bloqueoCollider != null)
        {
            bloqueoCollider.enabled = true;
        }
    }

    /// <summary>
    /// Estado actual de la puerta. true = bloqueada.
    /// </summary>
    public bool EstaBloqueada => _estaBloqueada;

    /// <summary>
    /// Desbloquea la puerta y ejecuta la animación de apertura.
    /// Debe ser llamado por un sistema externo cuando la pelea contra el guardián termine.
    /// </summary>
    public void UnlockDoor()
    {
        _estaBloqueada = false;

        if (bloqueoCollider != null)
        {
            bloqueoCollider.enabled = false;
        }

        _rotacionInicial = objetoVisual.rotation;
        _rotacionFinal = _rotacionInicial * Quaternion.Euler(0, anguloApertura, 0);
        _abriendo = true;
    }

    // ──────────────────────────────────────────────
    //  Detección del jugador
    // ──────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagJugador)) return;

        if (_estaBloqueada)
        {
            Debug.Log("La puerta está sellada por el guardián.");
        }
    }
}