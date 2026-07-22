using UnityEngine;

public sealed class EnemyRoleMarker : MonoBehaviour
{
    [SerializeField] private EnemyRole role = EnemyRole.FinalBoss;

    public EnemyRole Role => role;

    public void Configure(EnemyRole configuredRole)
    {
        role = configuredRole;
    }
}
