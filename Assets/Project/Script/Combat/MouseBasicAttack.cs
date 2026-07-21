using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ejecuta el ataque basico del perro con el boton izquierdo del mouse.
/// </summary>
public sealed class MouseBasicAttack : MonoBehaviour
{
    [SerializeField] private BasicAttack dogAttack;
    [SerializeField] private EnemyHealth enemyTarget;

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

        dogAttack.AttackEnemy(enemyTarget);
    }
}
