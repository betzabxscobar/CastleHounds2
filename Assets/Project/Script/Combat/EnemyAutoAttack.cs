using UnityEngine;

/// <summary>
/// IA basica: el enemigo ataca al perro cuando lo tiene dentro de rango.
/// </summary>
public sealed class EnemyAutoAttack : MonoBehaviour
{
    [SerializeField] private BasicAttack attack;
    [SerializeField] private DogHealth dogTarget;
    [SerializeField] private Transform dogTransform;
    [SerializeField, Min(0f)] private float attackRange = 2.5f;

    private void Update()
    {
        if (attack == null || dogTarget == null || dogTransform == null)
        {
            return;
        }

        if (!attack.CanAttack)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, dogTransform.position);
        if (distance <= attackRange)
        {
            attack.AttackDog(dogTarget);
        }
    }

    private void OnValidate()
    {
        attackRange = Mathf.Max(0f, attackRange);
    }
}
