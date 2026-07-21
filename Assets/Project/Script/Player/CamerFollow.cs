using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Posición")]
    public Vector3 offset = new Vector3(0, 3.5f, -5f);

    [Header("Suavizado")]
    public float positionSmooth = 5f;
    public float rotationSmooth = 5f;

    [Header("Altura de mirada")]
    public float lookHeight = 0.8f;


    private void LateUpdate()
    {
        if (target == null) return;


        Vector3 desiredPosition =
            target.position + target.rotation * offset;


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
}