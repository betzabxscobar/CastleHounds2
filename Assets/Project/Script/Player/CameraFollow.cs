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

        // Deja la camara ya ubicada en su posicion final desde el primer frame,
        // en vez de arrancar desde donde haya quedado en el editor y hacer un
        // barrido visible mientras el Lerp/Slerp la alcanza.
        if (target != null)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleZoomInput();

        Vector3 desiredPosition = ComputeDesiredPosition();

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

    private Vector3 ComputeDesiredPosition()
    {
        // Offset en espacio del mundo (no rotado por target.rotation): la camara
        // no gira con el jugador, solo lo sigue en posicion. Con offset.z
        // negativo la camara queda mas cerca del Portal que el jugador, es
        // decir "adelante" de el, mirando hacia su frente.
        Vector3 adjustedOffset = new Vector3(offset.x, offset.y, Mathf.Sign(offset.z) * currentDistance);
        return target.position + adjustedOffset;
    }

    private void SnapToTarget()
    {
        transform.position = ComputeDesiredPosition();
        Vector3 lookPoint = target.position + Vector3.up * lookHeight;
        transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);
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