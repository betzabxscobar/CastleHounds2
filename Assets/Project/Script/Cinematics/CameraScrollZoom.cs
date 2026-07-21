using Unity.Cinemachine;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CinemachineThirdPersonFollow))]
public class CameraScrollZoom : MonoBehaviour
{
    [SerializeField] private float zoomStep = 1f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 6f;
    [Tooltip("Tiempo de suavizado del zoom, en segundos.")]
    [SerializeField] private float smoothTime = 0.15f;

    private CinemachineThirdPersonFollow thirdPersonFollow;
    private float targetDistance;
    private float zoomVelocity;

    private void Awake()
    {
        thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
        targetDistance = thirdPersonFollow.CameraDistance;
    }

    private void Update()
    {
        float scroll = ReadScrollInput();
        if (!Mathf.Approximately(scroll, 0f))
        {
            targetDistance = Mathf.Clamp(targetDistance - scroll * zoomStep, minDistance, maxDistance);
        }

        thirdPersonFollow.CameraDistance = Mathf.SmoothDamp(
            thirdPersonFollow.CameraDistance,
            targetDistance,
            ref zoomVelocity,
            smoothTime
        );
    }

    private float ReadScrollInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.scroll.ReadValue().y * 0.01f;
        }
        return 0f;
#else
        return Input.GetAxis("Mouse ScrollWheel");
#endif
    }
}
