using UnityEngine;

[RequireComponent(typeof(Transform))]
public sealed class DogCinematicAutoMover : MonoBehaviour
{
    [Header("Ruta")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float stopDistance = 0.12f;

    [Header("Animacion opcional")]
    [SerializeField] private Animator animator;
    [SerializeField] private string movingBoolParameter = "IsMoving";
    [SerializeField] private string speedFloatParameter = "Speed";
    [SerializeField] private float animationSpeedValue = 1f;

    private bool isMoving;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public bool HasArrived => target == null || Vector3.Distance(transform.position, target.position) <= stopDistance;

    public void BeginMove()
    {
        isMoving = target != null;
        SetAnimatorMoving(isMoving);
    }

    public void StopMove()
    {
        isMoving = false;
        SetAnimatorMoving(false);
    }

    private void Update()
    {
        if (!isMoving || target == null)
        {
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= stopDistance)
        {
            StopMove();
            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        SetAnimatorMoving(false);
    }

    private void SetAnimatorMoving(bool moving)
    {
        if (animator == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(movingBoolParameter))
        {
            animator.SetBool(movingBoolParameter, moving);
        }

        if (!string.IsNullOrWhiteSpace(speedFloatParameter))
        {
            animator.SetFloat(speedFloatParameter, moving ? animationSpeedValue : 0f);
        }
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        turnSpeed = Mathf.Max(0f, turnSpeed);
        stopDistance = Mathf.Max(0.01f, stopDistance);
    }
}
