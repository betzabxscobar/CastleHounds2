using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BasicAttack), typeof(EnemyHealth), typeof(NavMeshAgent))]
public sealed class EnemyAIController : MonoBehaviour
{
    private enum EnemyState { Idle, Chase, Attack, Hit, Dead }

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerPositionReference;
    [SerializeField] private DogHealth playerHealth;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BasicAttack attack;
    [SerializeField] private CombatAnimation combatAnimation;

    [Header("Comportamiento")]
    [SerializeField, Min(0f)] private float detectionRadius = 20f;
    [SerializeField, Min(0f)] private float attackDistance = 1.8f;
    [SerializeField, Min(0f)] private float stoppingDistance = 1.4f;
    [SerializeField, Min(0f)] private float chaseSpeed = 3.5f;
    [SerializeField, Min(0f)] private float rotationSpeed = 10f;
    [SerializeField, Min(0f)] private float hitLockDuration = 0.25f;
    [SerializeField, Min(0f)] private float attackExitHysteresis = 0.5f;
    [SerializeField, Min(0.1f)] private float stuckRecoveryDelay = 0.45f;
    [SerializeField] private bool forceReliableDirectPursuit = true;
    [SerializeField] private bool requireCombatStarted;

    private EnemyState state = EnemyState.Idle;
    private Coroutine actionRoutine;
    private bool missingReferencesReported;
    private bool navMeshWarningReported;
    private bool recoveryWarningReported;
    private bool hasValidNavMeshPosition;
    private bool damageAppliedThisAttack;
    private bool attackAnimationFinished;
    private Vector3 lastValidNavMeshPosition;
    private Vector3 lastMovementCheckPosition;
    private float stuckTimer;

    private void Awake()
    {
        agent ??= GetComponent<NavMeshAgent>();
        attack ??= GetComponent<BasicAttack>();
        enemyHealth ??= GetComponent<EnemyHealth>();
        combatAnimation ??= GetComponent<CombatAnimation>();
        ConfigureAgent();
        lastMovementCheckPosition = transform.position;
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Damaged += HandleDamaged;
            enemyHealth.Defeated += HandleDefeated;
        }
        if (combatAnimation != null)
        {
            combatAnimation.AttackImpact += HandleAttackImpact;
            combatAnimation.AttackFinished += HandleAttackFinished;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Damaged -= HandleDamaged;
            enemyHealth.Defeated -= HandleDefeated;
        }
        if (combatAnimation != null)
        {
            combatAnimation.AttackImpact -= HandleAttackImpact;
            combatAnimation.AttackFinished -= HandleAttackFinished;
        }

        StopActionRoutine();
        StopAgent();
    }

    private void Update()
    {
        if (!HasRequiredReferences() || state == EnemyState.Dead || state == EnemyState.Hit)
            return;

        MaintainNavMeshPosition();

        if (enemyHealth.IsDefeated || playerHealth.IsDead || !combatGameManager.IsCombatActive)
        {
            EnterDeadState();
            return;
        }

        if (requireCombatStarted && !combatGameManager.HasCombatStarted)
        {
            EnterIdleState();
            return;
        }

        float distance = HorizontalDistanceToPlayer();
        if (state == EnemyState.Attack)
        {
            if (distance > attackDistance + attackExitHysteresis)
                EnterChaseState();
            else
                EnterAttackState();
            return;
        }

        if (distance > detectionRadius)
        {
            EnterIdleState();
        }
        else if (distance > attackDistance)
        {
            EnterChaseState();
        }
        else
        {
            EnterAttackState();
        }
    }

    public void SetTarget(Transform target, DogHealth health)
    {
        SetTarget(target, health, target);
    }

    public void SetTarget(Transform target, DogHealth health, Transform positionReference)
    {
        player = target;
        playerHealth = health;
        playerPositionReference = positionReference != null ? positionReference : target;
        missingReferencesReported = false;
    }

    public void ConfigureCombat(CombatGameManager manager)
    {
        combatGameManager = manager;
        missingReferencesReported = false;
    }

    public void ConfigureBehavior(float detection, float attackRange, float movementSpeed,
        float configuredRotationSpeed, float configuredStoppingDistance)
    {
        detectionRadius = Mathf.Max(0f, detection);
        attackDistance = Mathf.Max(0f, attackRange);
        chaseSpeed = Mathf.Max(0f, movementSpeed);
        rotationSpeed = Mathf.Max(0f, configuredRotationSpeed);
        stoppingDistance = Mathf.Clamp(configuredStoppingDistance, 0f, attackDistance);
        ConfigureAgent();
    }

    private void EnterIdleState()
    {
        if (state == EnemyState.Attack) StopActionRoutine();
        StopAgent();
        combatAnimation?.SetMoving(false);
        ChangeState(EnemyState.Idle);
    }

    private void EnterChaseState()
    {
        if (state == EnemyState.Attack) StopActionRoutine();
        combatAnimation?.SetMoving(true);
        RotateTowardsPlayer();

        // La arena tiene geometria que puede producir un NavMesh valido pero una ruta inmovil.
        // Este movimiento usa colisiones y rodeo lateral, por lo que garantiza la persecucion.
        if (forceReliableDirectPursuit)
        {
            StopAgent();
            MoveDirectlyTowardsPlayer();
            ChangeState(EnemyState.Chase);
            return;
        }

        if (!CanUseAgent())
        {
            MoveDirectlyTowardsPlayer();
            ChangeState(EnemyState.Chase);
            return;
        }
        if (!agent.updatePosition)
        {
            agent.nextPosition = transform.position;
            agent.updatePosition = true;
        }
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;
        bool destinationAccepted = false;
        if (NavMesh.SamplePosition(PlayerPosition, out NavMeshHit targetHit, 3f, NavMesh.AllAreas))
        {
            destinationAccepted = agent.SetDestination(targetHit.position);
            if (!destinationAccepted)
                Debug.LogWarning($"{name}: NavMeshAgent rechazó el destino del jugador {targetHit.position}.", this);
        }

        bool moved = (transform.position - lastMovementCheckPosition).sqrMagnitude > 0.0025f;
        stuckTimer = moved ? 0f : stuckTimer + Time.deltaTime;
        lastMovementCheckPosition = transform.position;

        // Respaldo para escenas cuyo NavMesh existe pero no produce una ruta útil.
        if (!destinationAccepted || agent.pathStatus == NavMeshPathStatus.PathInvalid || stuckTimer >= stuckRecoveryDelay)
        {
            agent.isStopped = true;
            agent.ResetPath();
            MoveDirectlyTowardsPlayer();
        }
        ChangeState(EnemyState.Chase);
    }

    private void MoveDirectlyTowardsPlayer()
    {
        Vector3 direction = PlayerPosition - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;
        if (distance <= attackDistance || distance < 0.001f) return;

        direction /= distance;
        float step = Mathf.Min(chaseSpeed * Time.deltaTime, distance - attackDistance);
        Vector3 movement = FindClearMovement(direction, step);
        if (movement.sqrMagnitude < 0.000001f) return;

        Rigidbody body = GetComponent<Rigidbody>();
        Vector3 destination = transform.position + movement;
        if (body != null && body.isKinematic)
            body.MovePosition(destination);
        else
            transform.position = destination;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.nextPosition = destination;
    }

    private Vector3 FindClearMovement(Vector3 direction, float distance)
    {
        if (CanMoveInDirection(direction, distance)) return direction * distance;

        Vector3 left = Quaternion.Euler(0f, -55f, 0f) * direction;
        if (CanMoveInDirection(left, distance)) return left * distance;

        Vector3 right = Quaternion.Euler(0f, 55f, 0f) * direction;
        return CanMoveInDirection(right, distance) ? right * distance : Vector3.zero;
    }

    private bool CanMoveInDirection(Vector3 direction, float distance)
    {
        Vector3 origin = transform.position + Vector3.up * 0.45f;
        RaycastHit[] hits = Physics.SphereCastAll(
            origin, 0.32f, direction, distance + 0.08f, ~0, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform)) continue;
            if (player != null && (hitTransform == player || hitTransform.IsChildOf(player))) continue;
            return false;
        }
        return true;
    }

    private void EnterAttackState()
    {
        StopAgent();
        if (agent != null && agent.enabled)
        {
            // El agente sigue sincronizado con el Transform, pero queda detenido.
            // Desactivar updatePosition podía dejar al modelo visual separado o trabado.
            agent.updatePosition = true;
            agent.nextPosition = transform.position;
        }
        combatAnimation?.SetMoving(false);
        RotateTowardsPlayer();

        if (state == EnemyState.Attack) return;
        ChangeState(EnemyState.Attack);
        actionRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        while (state == EnemyState.Attack && !enemyHealth.IsDefeated && !playerHealth.IsDead)
        {
            while (!attack.CanAttack && state == EnemyState.Attack)
            {
                RotateTowardsPlayer();
                yield return null;
            }

            if (state != EnemyState.Attack) break;

            damageAppliedThisAttack = false;
            attackAnimationFinished = false;
            combatAnimation?.PlayAttack(playerPositionReference != null ? playerPositionReference : player);
            while (!attackAnimationFinished && state == EnemyState.Attack)
            {
                RotateTowardsPlayer();
                yield return null;
            }

            if (state != EnemyState.Attack) break;
            yield return null;
        }

        actionRoutine = null;
    }

    // Puede llamarse desde un Animation Event colocado en el fotograma de la mordida.
    public void ApplyAttackDamage()
    {
        if (damageAppliedThisAttack || state != EnemyState.Attack || player == null ||
            playerHealth == null || playerHealth.IsDead || enemyHealth == null || enemyHealth.IsDefeated)
            return;

        if (HorizontalDistanceToPlayer() > attackDistance + 0.3f) return;
        damageAppliedThisAttack = true;
        attack.AttackDog(playerHealth, false);
    }

    private void HandleAttackImpact()
    {
        ApplyAttackDamage();
    }

    private void HandleAttackFinished()
    {
        attackAnimationFinished = true;
        FinishAttack();
    }

    public void FinishAttack()
    {
        if (state != EnemyState.Attack || player == null) return;
        if (HorizontalDistanceToPlayer() > attackDistance + attackExitHysteresis)
            EnterChaseState();
    }

    private void HandleDamaged(float amount)
    {
        if (state == EnemyState.Dead || enemyHealth.IsDefeated) return;
        StopActionRoutine();
        actionRoutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        StopAgent();
        combatAnimation?.SetMoving(false);
        ChangeState(EnemyState.Hit);
        yield return new WaitForSeconds(hitLockDuration);
        actionRoutine = null;
        if (!enemyHealth.IsDefeated) ChangeState(EnemyState.Idle);
    }

    private void HandleDefeated()
    {
        EnterDeadState();
    }

    private void EnterDeadState()
    {
        if (state == EnemyState.Dead) return;
        ChangeState(EnemyState.Dead);
        StopActionRoutine();
        StopAgent();
        combatAnimation?.SetMoving(false);
        if (agent != null) agent.enabled = false;
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;
        Vector3 direction = PlayerPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private float HorizontalDistanceToPlayer()
    {
        Vector3 offset = PlayerPosition - transform.position;
        offset.y = 0f;
        return offset.magnitude;
    }

    private Vector3 PlayerPosition => playerPositionReference != null
        ? playerPositionReference.position
        : player != null ? player.position : transform.position;

    private bool HasRequiredReferences()
    {
        bool valid = player != null && playerHealth != null && enemyHealth != null &&
                     combatGameManager != null && attack != null && combatAnimation != null;
        if (!valid && !missingReferencesReported)
        {
            Debug.LogError("EnemyAIController necesita Player, Player Health, Enemy Health, Combat Game Manager, Basic Attack y Combat Animation.", this);
            missingReferencesReported = true;
        }
        return valid;
    }

    private bool CanUseAgent()
    {
        bool valid = agent != null && agent.enabled && agent.isOnNavMesh;
        if (!valid && !navMeshWarningReported)
        {
            Debug.LogWarning("EnemyAIController: el enemigo no está sobre un NavMesh horneado; no puede perseguir hasta configurarlo.", this);
            navMeshWarningReported = true;
        }
        return valid;
    }

    private void MaintainNavMeshPosition()
    {
        if (agent == null || !agent.enabled) return;

        if (agent.isOnNavMesh)
        {
            lastValidNavMeshPosition = transform.position;
            hasValidNavMeshPosition = true;
            return;
        }

        Vector3 recoveryOrigin = hasValidNavMeshPosition ? lastValidNavMeshPosition : transform.position;
        if (NavMesh.SamplePosition(recoveryOrigin, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            if (!recoveryWarningReported)
            {
                Debug.LogWarning($"{name}: recuperado dentro del NavMesh en {hit.position}.", this);
                recoveryWarningReported = true;
            }
        }
    }

    private void ConfigureAgent()
    {
        if (agent == null) return;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = 0f;
        agent.updateRotation = false;
    }

    private void StopAgent()
    {
        stuckTimer = 0f;
        lastMovementCheckPosition = transform.position;
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (state == newState) return;
        state = newState;
        string controllerName = "NINGUNO";
        Animator animator = GetComponentInChildren<Animator>(true);
        if (animator != null && animator.runtimeAnimatorController != null)
            controllerName = animator.runtimeAnimatorController.name;

        Debug.Log(
            $"IA ESTADO={state} | distanciaXZ={(player != null ? HorizontalDistanceToPlayer() : -1f):F2} | " +
            $"isAttacking={state == EnemyState.Attack} | agentStopped={agent == null || agent.isStopped} | " +
            $"hasPath={agent != null && agent.hasPath} | objetivo={(player != null ? player.name : "NULL")} | " +
            $"Animator={controllerName}", this);
    }

    private void StopActionRoutine()
    {
        if (actionRoutine == null) return;
        StopCoroutine(actionRoutine);
        actionRoutine = null;
    }

    private void OnValidate()
    {
        detectionRadius = Mathf.Max(0f, detectionRadius);
        attackDistance = Mathf.Max(0f, attackDistance);
        stoppingDistance = Mathf.Clamp(stoppingDistance, 0f, attackDistance);
        chaseSpeed = Mathf.Max(0f, chaseSpeed);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        hitLockDuration = Mathf.Max(0f, hitLockDuration);
        attackExitHysteresis = Mathf.Max(0f, attackExitHysteresis);
        stuckRecoveryDelay = Mathf.Max(0.1f, stuckRecoveryDelay);
        ConfigureAgent();
    }
}
