using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ejecuta el ataque basico del perro con el boton izquierdo del mouse.
/// </summary>
public sealed class MouseBasicAttack : MonoBehaviour
{
    [SerializeField] private BasicAttack dogAttack;
    [SerializeField] private EnemyHealth enemyTarget;
    [SerializeField, Min(0f)] private float attackRange = 3.5f;

    private bool missingReferencesReported;
    private bool invalidTargetReported;
    private bool tooFarReported;
    private Renderer attackerRenderer;
    private Renderer targetRenderer;

    private void Awake()
    {
        // Se usa la malla visible como referencia de posicion porque el root
        // (donde vive este script y el collider) puede estar desplazado del
        // modelo que se ve en pantalla.
        attackerRenderer = GetComponentInChildren<Renderer>();

        if (enemyTarget != null)
        {
            targetRenderer = enemyTarget.GetComponentInChildren<Renderer>();
        }
    }

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

        if (!IsTargetInRange(enemyTarget))
        {
            return;
        }

        tooFarReported = false;
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

    private bool IsTargetInRange(EnemyHealth target)
    {
        if (targetRenderer == null || targetRenderer.gameObject != target.gameObject)
        {
            targetRenderer = target.GetComponentInChildren<Renderer>();
        }

        // Mide entre los modelos VISIBLES (no entre los roots, que pueden estar
        // separados de la malla), asi el rango coincide con lo que se ve.
        Vector3 origin = attackerRenderer != null ? attackerRenderer.bounds.center : transform.position;
        Vector3 targetPoint = targetRenderer != null ? targetRenderer.bounds.center : target.transform.position;

        float distance = Vector3.Distance(origin, targetPoint);
        if (distance > attackRange)
        {
            if (!tooFarReported)
            {
                Debug.Log($"{name}: {target.name} esta demasiado lejos para atacar (distancia {distance:F1}, rango {attackRange}).", this);
                tooFarReported = true;
            }

            return false;
        }

        return true;
    }
}
