using UnityEngine;

/// <summary>
/// Administra la vida del enemigo y comunica su derrota al combate.
/// </summary>
public sealed class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField] private bool destroyWhenDefeated;
    [SerializeField, Min(0f)] private float destroyDelay;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDefeated { get; private set; }

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDefeated)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Debug.Log($"{name} recibio {amount} de dano. Vida: {CurrentHealth}/{maxHealth}.", this);

        if (CurrentHealth <= 0f)
        {
            DefeatEnemy();
        }
    }

    /// <summary>Restaura la vida; se usa para pruebas o al iniciar otro combate.</summary>
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDefeated = false;
        Debug.Log($"Vida de {name} reiniciada a {CurrentHealth}.", this);
    }

    private void DefeatEnemy()
    {
        // Esta bandera impide procesar la derrota mas de una vez.
        IsDefeated = true;
        Debug.Log($"{name} ha sido derrotado.", this);

        if (combatGameManager != null)
        {
            combatGameManager.Victory();
        }
        else
        {
            Debug.LogError("EnemyHealth no tiene una referencia a CombatGameManager.", this);
        }

        if (destroyWhenDefeated)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        destroyDelay = Mathf.Max(0f, destroyDelay);
    }
}
