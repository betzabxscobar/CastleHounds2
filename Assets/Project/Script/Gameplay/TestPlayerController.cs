using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var col in colliders)
        {
            var door = col.GetComponent<GuardianDoorController>();
            if (door != null)
            {
                if (door.EstaBloqueada)
                {
                    door.AbrirPuerta();
                    Debug.Log("Puerta abierta!");
                }
                else
                {
                    door.BloquearPuerta();
                    Debug.Log("Puerta bloqueada!");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}