using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    public static event Action<string> OnBeforeSceneLoadRequested;

    public enum TipoTrigger
    {
        EntradaCastillo,
        InicioPelea,
        AbrirPuerta,
        MostrarMensaje,
        CambioDeZona,
        CargarEscena
    }

    [SerializeField] private TipoTrigger tipo;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool soloUnaVez = true;
    [SerializeField] private string mensaje;
    [SerializeField] private string doorId;
    [SerializeField] private string zoneId;
    [SerializeField] private UnityEvent onTriggerEnter;

    [Header("Solo si tipo = CargarEscena")]
    [Tooltip("Debe coincidir exactamente con el nombre del archivo de escena y estar agregada en Build Settings.")]
    [SerializeField] private string nombreEscena;

    private Collider _collider;
    private bool _yaActivado;
    private bool _cargandoEscena;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[ZoneTrigger] {gameObject.name}: ignorado, '{other.name}' tiene tag '{other.tag}' (se esperaba '{playerTag}').");
            return;
        }

        if (soloUnaVez && _yaActivado)
        {
            Debug.Log($"[ZoneTrigger] {gameObject.name}: ignorado, ya se activo antes (soloUnaVez).");
            return;
        }

        _yaActivado = true;

        switch (tipo)
        {
            case TipoTrigger.EntradaCastillo:
                GameEvents.RaiseCastleEnter();
                break;
            case TipoTrigger.InicioPelea:
                GameEvents.RaiseFightStart();
                break;
            case TipoTrigger.AbrirPuerta:
                GameEvents.RaiseDoorShouldOpen(doorId);
                break;
            case TipoTrigger.MostrarMensaje:
                GameEvents.RaiseMessageRequested(mensaje);
                break;
            case TipoTrigger.CambioDeZona:
                GameEvents.RaiseZoneChanged(zoneId);
                break;
            case TipoTrigger.CargarEscena:
                if (_cargandoEscena)
                {
                    return;
                }

                if (string.IsNullOrEmpty(nombreEscena))
                {
                    Debug.LogWarning($"[ZoneTrigger] {gameObject.name}: tipo CargarEscena pero nombreEscena esta vacio.");
                }
                else
                {
                    _cargandoEscena = true;
                    Time.timeScale = 1f;
                    OnBeforeSceneLoadRequested?.Invoke(nombreEscena);
                    SceneManager.LoadScene(nombreEscena);
                }
                break;
        }

        onTriggerEnter?.Invoke();

        if (soloUnaVez)
        {
            _collider.enabled = false;
        }
    }

    public void Resetear()
    {
        _yaActivado = false;
        _cargandoEscena = false;

        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }

        _collider.enabled = true;
    }

    public bool IsSceneLoadTarget(string sceneName)
    {
        return tipo == TipoTrigger.CargarEscena && nombreEscena == sceneName;
    }

    public void ConfigureSceneLoadTarget(string sceneName)
    {
        tipo = TipoTrigger.CargarEscena;
        nombreEscena = sceneName;
        _yaActivado = false;
        _cargandoEscena = false;

        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }

        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }
}
