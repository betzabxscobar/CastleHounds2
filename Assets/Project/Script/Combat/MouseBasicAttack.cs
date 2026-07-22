using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ejecuta el ataque basico del perro con el boton izquierdo del mouse.
/// </summary>
public sealed class MouseBasicAttack : MonoBehaviour
{
    [SerializeField] private BasicAttack dogAttack;
    [SerializeField] private EnemyHealth enemyTarget;
    [SerializeField, Min(0f)] private float attackRange = 2f;

    private bool missingReferencesReported;

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Attack();
        }
    }

    /// <summary>Tambien permite ordenar el ataque desde otro script.</summary>
    public void Attack()
    {
        if (dogAttack == null || enemyTarget == null)
        {
            if (!missingReferencesReported)
            {
                Debug.LogWarning(
                    "MouseBasicAttack necesita las referencias Dog Attack y Enemy Target.",
                    this);
                missingReferencesReported = true;
            }

            return;
        }

        float distance = Vector3.Distance(transform.position, enemyTarget.transform.position);
        if (distance > attackRange)
        {
            Debug.Log($"{name}: {enemyTarget.name} esta demasiado lejos para atacar (distancia {distance:F1}, rango {attackRange}).", this);
            return;
        }

        dogAttack.AttackEnemy(enemyTarget);
    }
}
