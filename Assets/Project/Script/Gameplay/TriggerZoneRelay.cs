using UnityEngine;

/// <summary>
/// Reenvía los eventos de trigger al GuardianDoorController padre.
/// Colocar en el GameObject hijo que tiene el collider de detección.
/// </summary>
public class TriggerZoneRelay : MonoBehaviour
{
    [SerializeField] private GuardianDoorController puerta;

    private void Awake()
    {
        if (puerta == null)
        {
            puerta = GetComponentInParent<GuardianDoorController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (puerta != null)
        {
            puerta.OnJugadorEntro(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (puerta != null)
        {
            puerta.OnJugadorSalio(other);
        }
    }
}
