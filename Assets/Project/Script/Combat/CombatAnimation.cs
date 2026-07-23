using System.Collections;
using System;
using UnityEngine;

/// <summary>
/// Combina clips reales del Animator con desplazamientos suaves de combate.
/// </summary>
public sealed class CombatAnimation : MonoBehaviour
{
    public event Action AttackImpact;
    public event Action AttackFinished;

    [Header("Referencias")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private Animator animator;

    [Header("Estados del Animator")]
    [SerializeField] private string attackStateName = "Run";
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string movementStateName = "Run";
    [SerializeField] private string hitStateName;
    [SerializeField] private string defeatStateName;

    [Header("Embestida")]
    [SerializeField, Min(0.05f)] private float anticipationDuration = 0.1f;
    [SerializeField, Min(0.05f)] private float lungeDuration = 0.18f;
    [SerializeField, Min(0.05f)] private float recoveryDuration = 0.18f;
    [SerializeField, Min(0f)] private float attackDistance = 0.45f;
    [SerializeField, Min(0f)] private float anticipationDistance = 0.08f;
    [SerializeField, Min(0f)] private float movementBounce = 0.09f;

    [Header("Reaccion sin clip")]
    [SerializeField, Min(0.05f)] private float hitDuration = 0.16f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Coroutine currentAnimation;
    private bool isDefeated;
    private Transform jawBone;
    private Transform leftShoulderBone;
    private Transform rightShoulderBone;
    private Transform leftFrontLegBone;
    private Transform rightFrontLegBone;
    private float biteWeight;
    private float pawWeight;
    private float attackPhase;
    private bool isMoving;
    private float movementPhase;

    private void Awake()
    {
        if (visualTransform == null)
        {
            visualTransform = transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        initialLocalPosition = visualTransform.localPosition;
        initialLocalRotation = visualTransform.localRotation;

        // Huesos reales del pastor aleman incluidos en el FBX.
        jawBone = FindChildRecursive(visualTransform, "DEF-jaw");
        leftShoulderBone = FindChildRecursive(visualTransform, "DEF-shoulder.L");
        rightShoulderBone = FindChildRecursive(visualTransform, "DEF-shoulder.R");
        leftFrontLegBone = FindChildRecursive(visualTransform, "DEF-front_thigh.L");
        rightFrontLegBone = FindChildRecursive(visualTransform, "DEF-front_thigh.R");
    }

    private void LateUpdate()
    {
        if (isMoving && currentAnimation == null && !isDefeated)
        {
            movementPhase += Time.deltaTime * 9f;
            float step = Mathf.Sin(movementPhase);
            visualTransform.localPosition = initialLocalPosition + Vector3.up * (Mathf.Abs(step) * movementBounce);
            visualTransform.localRotation = initialLocalRotation * Quaternion.Euler(step * 3f, 0f, step * 2.5f);
        }

        if (biteWeight <= 0f && pawWeight <= 0f)
        {
            return;
        }

        // Dos cierres rapidos de mandibula durante el impacto.
        float biteMotion = Mathf.Abs(Mathf.Sin(attackPhase * Mathf.PI * 2f)) * biteWeight;
        ApplyBoneRotation(jawBone, new Vector3(28f * biteMotion, 0f, 0f));

        // Las patas delanteras se elevan y golpean de forma alternada.
        float leftPaw = (0.65f + 0.35f * Mathf.Sin(attackPhase * Mathf.PI * 2f)) * pawWeight;
        float rightPaw = (0.65f - 0.35f * Mathf.Sin(attackPhase * Mathf.PI * 2f)) * pawWeight;
        ApplyBoneRotation(leftShoulderBone, new Vector3(-34f * leftPaw, 0f, 0f));
        ApplyBoneRotation(rightShoulderBone, new Vector3(-34f * rightPaw, 0f, 0f));
        ApplyBoneRotation(leftFrontLegBone, new Vector3(22f * leftPaw, 0f, 0f));
        ApplyBoneRotation(rightFrontLegBone, new Vector3(22f * rightPaw, 0f, 0f));
    }

    public void PlayAttack(Transform target)
    {
        if (!CanPlayAnimation())
        {
            return;
        }

        if (isDefeated)
        {
            return;
        }

        Vector3 direction = GetLocalDirection(target);
        StartRoutine(AttackRoutine(direction));
    }

    public void SetMoving(bool moving)
    {
        if (isMoving == moving || currentAnimation != null) return;
        isMoving = moving;
        bool statePlayed = TryPlayState(moving ? movementStateName : idleStateName, 0.12f);
        if (!moving || !statePlayed)
        {
            if (!moving) movementPhase = 0f;
            RestoreVisualTransform();
        }
    }

    public void ConfigureStateNames(string idle, string movement, string attack, string hit, string defeat)
    {
        idleStateName = idle;
        movementStateName = movement;
        attackStateName = attack;
        hitStateName = hit;
        defeatStateName = defeat;
    }

    public void ConfigurePresentation(float lungeDistance, float bounceAmount)
    {
        attackDistance = Mathf.Max(0.1f, lungeDistance);
        movementBounce = Mathf.Max(0.02f, bounceAmount);
    }

    public void PlayHit()
    {
        if (!CanPlayAnimation())
        {
            return;
        }

        if (isDefeated)
        {
            return;
        }

        if (TryPlayState(hitStateName, 0.05f))
        {
            return;
        }

        StartRoutine(HitRoutine());
    }

    public void PlayDefeat()
    {
        if (!CanPlayAnimation())
        {
            return;
        }

        if (isDefeated)
        {
            return;
        }

        isDefeated = true;
        StopCurrentRoutine();
        RestoreVisualTransform();

        if (!TryPlayState(defeatStateName, 0.08f))
        {
            currentAnimation = StartCoroutine(FallbackDefeatRoutine());
        }
    }

    private IEnumerator AttackRoutine(Vector3 direction)
    {
        TryPlayState(attackStateName, 0.05f);
        isMoving = false;

        Vector3 recoil = initialLocalPosition - direction * anticipationDistance;
        yield return MoveVisual(initialLocalPosition, recoil, anticipationDuration, 0f, 0.25f);

        Vector3 impact = initialLocalPosition + direction * attackDistance;
        yield return MoveVisual(recoil, impact, lungeDuration, 0.25f, 1f);
        AttackImpact?.Invoke();

        // Pausa breve en el impacto para que el golpe se perciba con claridad.
        float impactElapsed = 0f;
        while (impactElapsed < 0.16f)
        {
            impactElapsed += Time.deltaTime;
            attackPhase += Time.deltaTime * 7f;
            biteWeight = 1f;
            pawWeight = 1f;
            yield return null;
        }

        yield return MoveVisual(impact, initialLocalPosition, recoveryDuration, 1f, 0f);

        biteWeight = 0f;
        pawWeight = 0f;
        RestoreVisualTransform();
        TryPlayState(idleStateName, 0.1f);
        currentAnimation = null;
        AttackFinished?.Invoke();
    }

    private IEnumerator MoveVisual(Vector3 start, Vector3 end, float duration, float startAction, float endAction)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = progress * progress * (3f - 2f * progress);
            visualTransform.localPosition = Vector3.LerpUnclamped(start, end, eased);
            biteWeight = Mathf.Lerp(startAction, endAction, eased);
            pawWeight = Mathf.Lerp(startAction, endAction, eased);
            attackPhase += Time.deltaTime * 5f;
            yield return null;
        }

        visualTransform.localPosition = end;
    }

    private IEnumerator HitRoutine()
    {
        float elapsed = 0f;

        while (elapsed < hitDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / hitDuration);
            float angle = Mathf.Sin(progress * Mathf.PI * 2f) * 4f;
            visualTransform.localRotation = initialLocalRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        RestoreVisualTransform();
        currentAnimation = null;
    }

    private IEnumerator FallbackDefeatRoutine()
    {
        float elapsed = 0f;
        const float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            visualTransform.localRotation = initialLocalRotation * Quaternion.Euler(0f, 0f, 80f * progress);
            yield return null;
        }

        currentAnimation = null;
    }

    private bool TryPlayState(string stateName, float transitionDuration)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return false;
        }

        int fullPathHash = Animator.StringToHash($"Base Layer.{stateName}");
        if (!animator.HasState(0, fullPathHash))
        {
            return false;
        }

        animator.CrossFade(fullPathHash, transitionDuration, 0, 0f);
        return true;
    }

    private Vector3 GetLocalDirection(Transform target)
    {
        if (target == null)
        {
            return Vector3.forward;
        }

        Vector3 worldDirection = target.position - transform.position;
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.001f)
        {
            return Vector3.forward;
        }

        Transform parent = visualTransform.parent;
        Vector3 localDirection = parent != null
            ? parent.InverseTransformDirection(worldDirection.normalized)
            : worldDirection.normalized;
        localDirection.y = 0f;
        return localDirection.normalized;
    }

    private void StartRoutine(IEnumerator routine)
    {
        if (!CanPlayAnimation())
        {
            return;
        }

        StopCurrentRoutine();
        RestoreVisualTransform();
        currentAnimation = StartCoroutine(routine);
    }

    private bool CanPlayAnimation()
    {
        return isActiveAndEnabled && gameObject.activeInHierarchy;
    }

    private void StopCurrentRoutine()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        biteWeight = 0f;
        pawWeight = 0f;
    }

    private void RestoreVisualTransform()
    {
        visualTransform.localPosition = initialLocalPosition;
        visualTransform.localRotation = initialLocalRotation;
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void ApplyBoneRotation(Transform bone, Vector3 eulerOffset)
    {
        if (bone != null)
        {
            bone.localRotation *= Quaternion.Euler(eulerOffset);
        }
    }

    private void OnValidate()
    {
        anticipationDuration = Mathf.Max(0.05f, anticipationDuration);
        lungeDuration = Mathf.Max(0.05f, lungeDuration);
        recoveryDuration = Mathf.Max(0.05f, recoveryDuration);
        attackDistance = Mathf.Max(0f, attackDistance);
        anticipationDistance = Mathf.Max(0f, anticipationDistance);
        hitDuration = Mathf.Max(0.05f, hitDuration);
    }
}
