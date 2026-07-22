using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BasicAttack))]
public sealed class EnemyAIController : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private DogHealth playerHealth;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BasicAttack attack;

    [Header("Comportamiento")]
    [SerializeField, Min(0f)] private float detectionRadius = 12f;
    [SerializeField, Min(0f)] private float attackDistance = 2.5f;
    [SerializeField, Min(0f)] private float chaseSpeed = 3.5f;
    [SerializeField] private bool requireCombatStarted = true;

    private EnemyState state = EnemyState.Idle;
    private bool missingReferencesReported;

    private void Awake()
    {
        agent ??= GetComponent<NavMeshAgent>();
        attack ??= GetComponent<BasicAttack>();
        enemyHealth ??= GetComponent<EnemyHealth>();

        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackDistance;
        }
    }

    private void Update()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        if (enemyHealth.IsDefeated || playerHealth.IsDead || !combatGameManager.IsCombatActive)
        {
            SetState(EnemyState.Dead);
            StopAgent();
            return;
        }

        if (requireCombatStarted && !combatGameManager.HasCombatStarted)
        {
            SetState(EnemyState.Idle);
            StopAgent();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRadius)
        {
            SetState(EnemyState.Idle);
            StopAgent();
            return;
        }

        if (distance > attackDistance)
        {
            SetState(EnemyState.Chase);
            ChasePlayer();
            return;
        }

        SetState(EnemyState.Attack);
        StopAgent();
        attack.AttackDog(playerHealth);
    }

    public void SetTarget(Transform target, DogHealth health)
    {
        player = target;
        playerHealth = health;
        missingReferencesReported = false;
    }

    private bool HasRequiredReferences()
    {
        if (player != null && playerHealth != null && enemyHealth != null && combatGameManager != null && attack != null)
        {
            return true;
        }

        if (!missingReferencesReported)
        {
            Debug.LogError("EnemyAIController necesita player, playerHealth, enemyHealth, combatGameManager y attack.", this);
            missingReferencesReported = true;
        }

        return false;
    }

    private void ChasePlayer()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackDistance;
        agent.SetDestination(player.position);
    }

    private void StopAgent()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void SetState(EnemyState newState)
    {
        state = newState;
    }

    private void OnValidate()
    {
        detectionRadius = Mathf.Max(0f, detectionRadius);
        attackDistance = Mathf.Max(0f, attackDistance);
        chaseSpeed = Mathf.Max(0f, chaseSpeed);
    }
}
