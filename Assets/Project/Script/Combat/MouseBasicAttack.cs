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
    private bool invalidTargetReported;

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

        if (!CanUseTarget(enemyTarget))
        {
            enemyTarget = null;
            return;
        }

        invalidTargetReported = false;
        dogAttack.AttackEnemy(enemyTarget);
    }

    public void ClearEnemyTargetIfMatches(EnemyHealth target)
    {
        if (target != null && enemyTarget == target)
        {
            enemyTarget = null;
            invalidTargetReported = false;
        }
    }

    private bool CanUseTarget(EnemyHealth target)
    {
        if (target == null || target.IsDefeated)
        {
            return false;
        }

        if (!target.gameObject.activeInHierarchy || !target.enabled)
        {
            if (!invalidTargetReported)
            {
                Debug.LogWarning(
                    "MouseBasicAttack ignora el ataque porque el enemigo objetivo esta inactivo.",
                    this);
                invalidTargetReported = true;
            }

            return false;
        }

        return true;
    }
}
