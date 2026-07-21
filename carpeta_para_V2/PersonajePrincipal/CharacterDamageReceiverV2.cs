using UnityEngine;

public sealed class CharacterDamageReceiverV2 : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool disableWhenDefeated;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDefeated => CurrentHealth <= 0f;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDefeated)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (IsDefeated && disableWhenDefeated)
        {
            gameObject.SetActive(false);
        }
    }

    public void RestoreHealth()
    {
        CurrentHealth = maxHealth;
        gameObject.SetActive(true);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
    }
}
