using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactLayer;

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDistance, interactLayer))
        {
            var door = hit.collider.GetComponent<GuardianDoorController>();
            if (door != null)
            {
                if (door.EstaBloqueada)
                {
                    door.AbrirPuerta();
                    Debug.Log("Jugador abrió la puerta");
                }
                else
                {
                    door.BloquearPuerta();
                    Debug.Log("Jugador bloqueó la puerta");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * interactDistance);
    }
}