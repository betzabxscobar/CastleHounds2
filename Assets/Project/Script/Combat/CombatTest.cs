using UnityEngine;

/// <summary>
/// Expone acciones en el menu contextual para probar el combate temporalmente.
/// </summary>
public sealed class CombatTest : MonoBehaviour
{
    [SerializeField] private DogHealth dogHealth;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private BasicAttack dogAttack;
    [SerializeField] private BasicAttack enemyAttack;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField, Min(0f)] private float healAmount = 20f;

    [ContextMenu("Combat Test/Damage Dog")]
    public void DamageDog()
    {
        if (enemyAttack == null || dogHealth == null)
        {
            Debug.LogWarning("CombatTest necesita Enemy Attack y Dog Health.", this);
            return;
        }

        enemyAttack.AttackDog(dogHealth);
    }

    [ContextMenu("Combat Test/Damage Enemy")]
    public void DamageEnemy()
    {
        if (dogAttack == null || enemyHealth == null)
        {
            Debug.LogWarning("CombatTest necesita Dog Attack y Enemy Health.", this);
            return;
        }

        dogAttack.AttackEnemy(enemyHealth);
    }

    [ContextMenu("Combat Test/Heal Dog")]
    public void HealDog()
    {
        if (dogHealth == null)
        {
            Debug.LogWarning("CombatTest necesita Dog Health.", this);
            return;
        }

        dogHealth.Heal(healAmount);
    }

    [ContextMenu("Combat Test/Reset Combat")]
    public void ResetCombat()
    {
        if (combatGameManager == null || dogHealth == null || enemyHealth == null)
        {
            Debug.LogWarning("CombatTest necesita Game Manager, Dog Health y Enemy Health.", this);
            return;
        }

        combatGameManager.ResetCombat();
        dogHealth.ResetHealth();
        enemyHealth.ResetHealth();

        if (dogAttack != null)
        {
            dogAttack.ResetCooldown();
        }

        if (enemyAttack != null)
        {
            enemyAttack.ResetCooldown();
        }

        Debug.Log("CombatTest reinicio el combate y las vidas.", this);
    }

    private void OnValidate()
    {
        healAmount = Mathf.Max(0f, healAmount);
    }
}
