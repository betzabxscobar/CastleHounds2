using UnityEngine;
using UnityEngine.InputSystem;

// SCRIPT TEMPORAL - borrar cuando el sistema de ataque real esté conectado
public class DebugCombateTemporal : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;

    private void Awake()
    {
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<CombatManager>();
        }
    }

    private void Update()
    {
        if (combatManager == null || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            combatManager.DanoAlEnemigo(20);
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            combatManager.DanoAlJugador(20);
        }
    }
}
