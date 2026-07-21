using UnityEngine;

/// <summary>
/// Mantiene el estado general de una sesion de combate.
/// </summary>
public sealed class CombatGameManager : MonoBehaviour
{
    public bool IsCombatActive { get; private set; } = true;
    public bool HasCombatStarted { get; private set; }
    public bool IsVictory { get; private set; }
    public bool IsDefeat { get; private set; }

    public void Victory()
    {
        if (!IsCombatActive)
        {
            return;
        }

        IsCombatActive = false;
        IsVictory = true;
        IsDefeat = false;
        Debug.Log("VICTORIA: El enemigo ha sido derrotado.", this);
    }

    public void BeginCombat()
    {
        if (IsCombatActive)
        {
            HasCombatStarted = true;
        }
    }

    public void Defeat()
    {
        if (!IsCombatActive)
        {
            return;
        }

        IsCombatActive = false;
        IsVictory = false;
        IsDefeat = true;
        Debug.Log("DERROTA: El perro ha sido derrotado.", this);
    }

    /// <summary>Restablece solo el estado general para comenzar otra prueba.</summary>
    public void ResetCombat()
    {
        IsCombatActive = true;
        HasCombatStarted = false;
        IsVictory = false;
        IsDefeat = false;
        Debug.Log("El estado del combate ha sido reiniciado.", this);
    }
}
