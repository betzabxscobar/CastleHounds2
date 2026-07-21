using UnityEngine;

public class DebugListenerTemporal : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnCastleEnter += HandleCastleEnter;
        GameEvents.OnFightStart += HandleFightStart;
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
        GameEvents.OnDoorShouldOpen += HandleDoorShouldOpen;
        GameEvents.OnMessageRequested += HandleMessageRequested;
        GameEvents.OnZoneChanged += HandleZoneChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnCastleEnter -= HandleCastleEnter;
        GameEvents.OnFightStart -= HandleFightStart;
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
        GameEvents.OnDoorShouldOpen -= HandleDoorShouldOpen;
        GameEvents.OnMessageRequested -= HandleMessageRequested;
        GameEvents.OnZoneChanged -= HandleZoneChanged;
    }

    private void HandleCastleEnter() => Debug.Log("[DebugListenerTemporal] OnCastleEnter");
    private void HandleFightStart() => Debug.Log("[DebugListenerTemporal] OnFightStart");
    private void HandleEnemyDefeated() => Debug.Log("[DebugListenerTemporal] OnEnemyDefeated");
    private void HandleDoorShouldOpen(string doorId) => Debug.Log($"[DebugListenerTemporal] OnDoorShouldOpen: {doorId}");
    private void HandleMessageRequested(string mensaje) => Debug.Log($"[DebugListenerTemporal] OnMessageRequested: {mensaje}");
    private void HandleZoneChanged(string zoneId) => Debug.Log($"[DebugListenerTemporal] OnZoneChanged: {zoneId}");
}
