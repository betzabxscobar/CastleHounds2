using UnityEngine;

public sealed class EnemyRoleMarker : MonoBehaviour
{
    [SerializeField] private EnemyRole role = EnemyRole.FinalBoss;

    public EnemyRole Role => role;
}
