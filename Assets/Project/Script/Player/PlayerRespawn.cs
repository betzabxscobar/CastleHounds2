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


        transform.position = spawnPosition;
        transform.eulerAngles = spawnRotation;


        controller.enabled = true;


        Debug.Log("Jugador reapareció en: " + spawnPosition);
    }
}