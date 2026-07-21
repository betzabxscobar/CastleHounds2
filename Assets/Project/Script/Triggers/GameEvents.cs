using System;

public static class GameEvents
{
    public static event Action OnCastleEnter;
    public static event Action OnFightStart;
    public static event Action OnEnemyDefeated;
    public static event Action<string> OnDoorShouldOpen;
    public static event Action<string> OnMessageRequested;
    public static event Action<string> OnZoneChanged;

    public static void RaiseCastleEnter() => OnCastleEnter?.Invoke();
    public static void RaiseFightStart() => OnFightStart?.Invoke();
    public static void RaiseEnemyDefeated() => OnEnemyDefeated?.Invoke();
    public static void RaiseDoorShouldOpen(string doorId) => OnDoorShouldOpen?.Invoke(doorId);
    public static void RaiseMessageRequested(string mensaje) => OnMessageRequested?.Invoke(mensaje);
    public static void RaiseZoneChanged(string zoneId) => OnZoneChanged?.Invoke(zoneId);
}
