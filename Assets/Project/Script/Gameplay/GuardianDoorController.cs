using UnityEngine;

/// <summary>
/// Controla una puerta bloqueada por un guardián.
/// Soporta puerta simple o doble (izquierda + derecha).
/// La puerta inicia bloqueada y puede desbloquearse mediante UnlockDoor().
/// </summary>
public class GuardianDoorController : MonoBehaviour
{
    [Header("Colisiones")]
    [SerializeField] private Collider bloqueoCollider;
    [SerializeField] private Collider zonaDeteccion;

    [Header("Puerta Simple")]
    [SerializeField] private Transform objetoVisual;
    [SerializeField] private bool bloqueadoInicial = true;

    [Header("Puerta Doble (opcional)")]
    [SerializeField] private Transform puertaIzquierda;
    [SerializeField] private Transform puertaDerecha;
    [SerializeField] private float anguloIzquierda = -90f;
    [SerializeField] private float anguloDerecha = 90f;

    [Header("Apertura")]
    [SerializeField] private float anguloApertura = 90f;
    [SerializeField] private float velocidadApertura = 2f;

    private bool _estaBloqueada = true;
    private bool _abriendo;
    private Quaternion _rotacionInicial;
    private Quaternion _rotacionFinal;
    private Quaternion _rotIzqInicial;
    private Quaternion _rotIzqFinal;
    private Quaternion _rotDerInicial;
    private Quaternion _rotDerFinal;
    private bool _esPuertaDoble;

    private const string TagJugador = "Player";

    private void Awake()
    {
        if (bloqueoCollider == null)
        {
            bloqueoCollider = GetComponent<Collider>();
        }

        _esPuertaDoble = puertaIzquierda != null && puertaDerecha != null;

        if (_esPuertaDoble)
        {
            _rotIzqInicial = puertaIzquierda.rotation;
            _rotIzqFinal = _rotIzqInicial * Quaternion.Euler(0, anguloIzquierda, 0);
            _rotDerInicial = puertaDerecha.rotation;
            _rotDerFinal = _rotDerInicial * Quaternion.Euler(0, anguloDerecha, 0);
        }
        else
        {
            if (objetoVisual == null)
            {
                objetoVisual = transform;
            }

            _rotacionInicial = objetoVisual.rotation;
            _rotacionFinal = _rotacionInicial * Quaternion.Euler(0, anguloApertura, 0);
        }

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

        if (_esPuertaDoble)
        {
            puertaIzquierda.rotation = Quaternion.RotateTowards(
                puertaIzquierda.rotation, _rotIzqFinal,
                velocidadApertura * Time.deltaTime * 90f);

            puertaDerecha.rotation = Quaternion.RotateTowards(
                puertaDerecha.rotation, _rotDerFinal,
                velocidadApertura * Time.deltaTime * 90f);

            bool izqListo = Quaternion.Angle(puertaIzquierda.rotation, _rotIzqFinal) < 0.5f;
            bool derListo = Quaternion.Angle(puertaDerecha.rotation, _rotDerFinal) < 0.5f;

            if (izqListo && derListo)
            {
                puertaIzquierda.rotation = _rotIzqFinal;
                puertaDerecha.rotation = _rotDerFinal;
                _abriendo = false;
            }
        }
        else
        {
            objetoVisual.rotation = Quaternion.RotateTowards(
                objetoVisual.rotation, _rotacionFinal,
                velocidadApertura * Time.deltaTime * 90f);

            if (Quaternion.Angle(objetoVisual.rotation, _rotacionFinal) < 0.5f)
            {
                objetoVisual.rotation = _rotacionFinal;
                _abriendo = false;
            }
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
    /// Para puerta doble, ambas hojas se abren simultáneamente.
    /// Debe ser llamado por un sistema externo cuando las condiciones se cumplan.
    /// </summary>
    public void UnlockDoor()
    {
        _estaBloqueada = false;

        if (bloqueoCollider != null)
        {
            bloqueoCollider.enabled = false;
        }

        if (_esPuertaDoble)
        {
            _rotIzqInicial = puertaIzquierda.rotation;
            _rotIzqFinal = _rotIzqInicial * Quaternion.Euler(0, anguloIzquierda, 0);
            _rotDerInicial = puertaDerecha.rotation;
            _rotDerFinal = _rotDerInicial * Quaternion.Euler(0, anguloDerecha, 0);
        }
        else
        {
            _rotacionInicial = objetoVisual.rotation;
            _rotacionFinal = _rotacionInicial * Quaternion.Euler(0, anguloApertura, 0);
        }

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