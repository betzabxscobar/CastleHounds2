using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Distancias")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2.5f;

    [Header("Referencias")]
    [SerializeField] private BasicAttack basicAttack;
    [SerializeField] private DogHealth dogHealth;
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        Debug.Log("¿Está sobre el NavMesh?: " + agent.isOnNavMesh);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Debug.Log("EnemyController funcionando");

        if (target == null)
        {
            Debug.LogError("Target es NULL");
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        // Si el jugador está muy lejos
        if (distance > detectionRange)
        {
            agent.isStopped = true;
            return;
        }

        // Perseguir al jugador
        if (distance > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
        // Atacar
        else
        {
            agent.isStopped = true;

            if (basicAttack != null && basicAttack.CanAttack)
            {
                basicAttack.AttackDog(dogHealth);
            }
        }
    }
}