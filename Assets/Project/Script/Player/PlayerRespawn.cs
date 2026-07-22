using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Posición de Spawn")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 2, 0);
    [SerializeField] private Vector3 spawnRotation = Vector3.zero;


    private CharacterController controller;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }


    private void Start()
    {
        Respawn();
    }


    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Respawn();
        }
    }


    public void Respawn()
    {
        controller.enabled = false;


        transform.position = FindGroundedPosition(spawnPosition);
        transform.eulerAngles = spawnRotation;


        controller.enabled = true;


        Debug.Log("Jugador reapareció en: " + transform.position);
    }


    private static Vector3 FindGroundedPosition(Vector3 desiredPosition)
    {
        Vector3 rayOrigin = desiredPosition + Vector3.up * 50f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 200f))
        {
            return hit.point + Vector3.up * 0.05f;
        }

        return desiredPosition;
    }
}