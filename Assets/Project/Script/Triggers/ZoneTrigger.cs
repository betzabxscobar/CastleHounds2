using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    public enum TipoTrigger
    {
        EntradaCastillo,
        InicioPelea,
        AbrirPuerta,
        MostrarMensaje,
        CambioDeZona
    }

    [SerializeField] private TipoTrigger tipo;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool soloUnaVez = true;
    [SerializeField] private string mensaje;
    [SerializeField] private string doorId;
    [SerializeField] private string zoneId;
    [SerializeField] private UnityEvent onTriggerEnter;

    private Collider _collider;
    private bool _yaActivado;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (soloUnaVez && _yaActivado) return;

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

        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }

        _collider.enabled = true;
    }
}
