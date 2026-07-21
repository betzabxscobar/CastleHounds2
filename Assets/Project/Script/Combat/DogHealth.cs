using UnityEngine;

/// <summary>
/// Administra la vida del perro y comunica su derrota al combate.
/// </summary>
public sealed class DogHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private CombatGameManager combatGameManager;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Debug.Log($"{name} recibio {amount} de dano. Vida: {CurrentHealth}/{maxHealth}.", this);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        Debug.Log($"{name} recupero {amount} de vida. Vida: {CurrentHealth}/{maxHealth}.", this);
    }

    /// <summary>Restaura la vida; se usa para pruebas o al iniciar otro combate.</summary>
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        Debug.Log($"Vida de {name} reiniciada a {CurrentHealth}.", this);
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log($"{name} ha muerto.", this);

        if (combatGameManager != null)
        {
            combatGameManager.Defeat();
        }
        else
        {
            Debug.LogError("DogHealth no tiene una referencia a CombatGameManager.", this);
        }
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
    }
}
