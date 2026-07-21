using UnityEngine;

public class TestGuardianDoorController : MonoBehaviour
{
    [SerializeField] private GuardianDoorController doorController;
    [SerializeField] private KeyCode testKey = KeyCode.E;

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            if (doorController != null)
            {
                if (doorController.EstaBloqueada)
                {
                    doorController.AbrirPuerta();
                    Debug.Log("Puerta abierta desde test");
                }
                else
                {
                    doorController.BloquearPuerta();
                    Debug.Log("Puerta bloqueada desde test");
                }
            }
        }
    }
}