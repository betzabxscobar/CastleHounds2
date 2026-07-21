using UnityEngine;

public sealed class CastleOrbitCameraRig : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 2.5f, 0f);

    [Header("Movimiento")]
    [SerializeField] private bool rotateOnEnable;
    [SerializeField] private float degreesPerSecond = 12f;
    [SerializeField] private float lookSmoothTime = 0.18f;

    private Vector3 angularVelocity;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public bool IsRotating
    {
        get => rotateOnEnable;
        set => rotateOnEnable = value;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (rotateOnEnable && !Mathf.Approximately(degreesPerSecond, 0f))
        {
            transform.RotateAround(target.position, Vector3.up, degreesPerSecond * Time.deltaTime);
        }

        Vector3 lookPoint = target.position + lookOffset;
        Vector3 direction = lookPoint - transform.position;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        if (lookSmoothTime <= 0f)
        {
            transform.rotation = targetRotation;
            return;
        }

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = targetRotation.eulerAngles;
        Vector3 smoothedEuler = new Vector3(
            Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref angularVelocity.x, lookSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref angularVelocity.y, lookSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref angularVelocity.z, lookSmoothTime));

        transform.rotation = Quaternion.Euler(smoothedEuler);
    }
}
