using UnityEngine;

public class PuertaBloqueada : MonoBehaviour
{
    [SerializeField] private string doorId;
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerAnimacionAbrir = "Abrir";
    [SerializeField] private Collider colliderBloqueo;

    private bool abierta;

    private void OnEnable()
    {
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
        GameEvents.OnDoorShouldOpen += HandleDoorShouldOpen;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
        GameEvents.OnDoorShouldOpen -= HandleDoorShouldOpen;
    }

    private void HandleEnemyDefeated()
    {
        Abrir();
    }

    private void HandleDoorShouldOpen(string id)
    {
        if (id != doorId) return;
        Abrir();
    }

    private void Abrir()
    {
        if (abierta) return;
        abierta = true;

        if (animator != null)
        {
            animator.SetTrigger(triggerAnimacionAbrir);
        }

        if (colliderBloqueo != null)
        {
            colliderBloqueo.enabled = false;
        }

        GameEvents.RaiseMessageRequested("La puerta se ha abierto.");
    }
}
