using UnityEngine;

/// <summary>
/// Aplica dano directo a un perro o enemigo mediante metodos publicos.
/// </summary>
public sealed class BasicAttack : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damage = 10f;
    [SerializeField, Min(0f)] private float attackCooldown = 1f;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField] private CombatAnimation combatAnimation;

    private float nextAttackTime;

    public float Damage => damage;
    public bool CanAttack => combatGameManager != null &&
                             combatGameManager.IsCombatActive &&
                             Time.time >= nextAttackTime;

    public void AttackDog(DogHealth target)
    {
        if (!CanStartAttack(target, "perro"))
        {
            return;
        }

        combatGameManager.BeginCombat();
        target.TakeDamage(damage);
        PlayCombatAnimations(target.GetComponent<CombatAnimation>(), target.IsDead);
        RegisterAttack(target.name);
    }

    public void AttackEnemy(EnemyHealth target)
    {
        if (!CanStartAttack(target, "enemigo"))
        {
            return;
        }

        combatGameManager.BeginCombat();
        target.TakeDamage(damage);
        PlayCombatAnimations(target.GetComponent<CombatAnimation>(), target.IsDefeated);
        RegisterAttack(target.name);
    }

    /// <summary>Permite iniciar una nueva prueba sin esperar el cooldown anterior.</summary>
    public void ResetCooldown()
    {
        nextAttackTime = 0f;
    }

    private bool CanStartAttack(Object target, string targetType)
    {
        if (target == null)
        {
            Debug.LogWarning($"{name} no puede atacar: el {targetType} es nulo.", this);
            return false;
        }

        if (combatGameManager == null)
        {
            Debug.LogError("BasicAttack no tiene una referencia a CombatGameManager.", this);
            return false;
        }

        if (!combatGameManager.IsCombatActive)
        {
            Debug.LogWarning($"{name} no puede atacar porque el combate termino.", this);
            return false;
        }

        if (Time.time < nextAttackTime)
        {
            Debug.Log($"{name} aun esta esperando para volver a atacar.", this);
            return false;
        }

        return true;
    }

    private void RegisterAttack(string targetName)
    {
        nextAttackTime = Time.time + attackCooldown;
        Debug.Log($"{name} ataco e hizo {damage} de dano a {targetName}.", this);
    }

    private void PlayCombatAnimations(CombatAnimation targetAnimation, bool targetDefeated)
    {
        if (combatAnimation != null)
        {
            combatAnimation.PlayAttack(targetAnimation != null ? targetAnimation.transform : null);
        }

        if (targetAnimation == null)
        {
            return;
        }

        if (targetDefeated)
        {
            targetAnimation.PlayDefeat();
        }
        else
        {
            targetAnimation.PlayHit();
        }
    }

    private void OnValidate()
    {
        damage = Mathf.Max(0f, damage);
        attackCooldown = Mathf.Max(0f, attackCooldown);
    }
}
