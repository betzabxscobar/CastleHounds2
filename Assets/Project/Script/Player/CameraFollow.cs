using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Posición")]
    public Vector3 offset = new Vector3(0, 1.5f, -5f);

    [Header("Suavizado")]
    public float positionSmooth = 3f;
    public float rotationSmooth = 3f;

    [Header("Altura de mirada")]
    public float lookHeight = 0.8f;

    [Header("Zoom (scroll del mouse)")]
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 10f;

    private float currentDistance;

    private void Awake()
    {
        currentDistance = Mathf.Clamp(Mathf.Abs(offset.z), minDistance, maxDistance);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleZoomInput();

        Vector3 adjustedOffset = new Vector3(offset.x, offset.y, Mathf.Sign(offset.z) * currentDistance);
        Vector3 desiredPosition =
            target.position + target.rotation * adjustedOffset;


        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmooth * Time.deltaTime
        );


        Vector3 lookPoint =
            target.position + Vector3.up * lookHeight;


        Quaternion targetRotation =
            Quaternion.LookRotation(
                lookPoint - transform.position
            );


        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    private void HandleZoomInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed, minDistance, maxDistance);
    }
}