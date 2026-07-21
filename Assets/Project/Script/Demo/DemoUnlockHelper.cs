using UnityEngine;

public class DemoUnlockHelper : MonoBehaviour
{
    [SerializeField] private GuardianDoorController puerta;
    [SerializeField] private KeyCode teclaDesbloquear = KeyCode.U;

    private void Update()
    {
        if (Input.GetKeyDown(teclaDesbloquear) && puerta != null && puerta.EstaBloqueada)
        {
            puerta.UnlockDoor();
        }
    }
}