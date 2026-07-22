# Implementacion de siete retos

## Flujo final

`MenuPrincipal -> SeleccionAvatar -> Historia -> Cinematica -> exploracion de siete casas -> Castle desbloqueado -> _DemoScene -> combate contra lobo -> volver a Demo`.

## Arquitectura

- `ChallengeProgressManager` mantiene el progreso 0/7 a 7/7 con `HashSet<string>`.
- El progreso persiste en `PlayerPrefs` con claves `CastleHounds2.SevenChallenges.<id>`.
- `HouseChallengeTrigger` abre un reto, bloquea control, espera resultado y solo completa con `Won`.
- `ChallengeGameController` es la base reusable para minijuegos futuros.
- `ChallengeTestPanel` es temporal y permite simular `Won`, `Lost` y `Cancelled`.
- `CastleUnlockController` consulta el progreso y desbloquea el acceso cuando hay 7/7.
- `FinalCastleTrigger` carga `_DemoScene` solo si todos los retos estan completos.
- `EnemyRole` separa `InitialWolf`, `RegularEnemy` y `FinalBoss`.
- `SevenChallengesSceneBootstrap` apaga el lobo de `Demo`; el lobo aparece al cargar `_DemoScene` desde la puerta del castillo.
- `ArenaBattleReturnController` evita que `_DemoScene` cargue `Ganaste` y devuelve al jugador a `Demo` en `(-0.105, 0, 0.7049999)`.

## Scripts creados

- `Assets/Project/Script/Challenges/Core/ChallengeResult.cs`
- `Assets/Project/Script/Challenges/Core/IChallengeGame.cs`
- `Assets/Project/Script/Challenges/Core/ChallengeGameController.cs`
- `Assets/Project/Script/Challenges/Core/ChallengeProgressManager.cs`
- `Assets/Project/Script/Challenges/Core/CastleUnlockController.cs`
- `Assets/Project/Script/Challenges/HouseTriggers/HouseChallengeTrigger.cs`
- `Assets/Project/Script/Challenges/HouseTriggers/FinalCastleTrigger.cs`
- `Assets/Project/Script/Challenges/Integration/PlayerControlLock.cs`
- `Assets/Project/Script/Challenges/Integration/InitialWolfVictoryTransition.cs`
- `Assets/Project/Script/Challenges/Integration/ArenaBattleReturnController.cs`
- `Assets/Project/Script/Challenges/Integration/SevenChallengesSceneBootstrap.cs`
- `Assets/Project/Script/Challenges/Testing/ChallengeTestPanel.cs`
- `Assets/Project/Script/Challenges/UI/ChallengeProgressHUD.cs`
- `Assets/Project/Script/Combat/EnemyRole.cs`
- `Assets/Project/Script/Combat/EnemyRoleMarker.cs`
- `Challenge01GameBridge.cs` hasta `Challenge07GameBridge.cs`

## Scripts modificados

- `GameEvents`: agrega `OnEnemyDefeatedWithRole`.
- `EnemyHealth`: emite muerte con rol, permite configurar rol por codigo y solo dispara victoria final para `FinalBoss`.
- `ChallengeGameController`: emite evento de inicio.
- `IChallengeGame`: expone evento de inicio.

## Relacion casa-ID

| Objeto | ID |
|---|---|
| House | house_challenge_01 |
| House (1) | house_challenge_02 |
| House (2) | house_challenge_03 |
| House (3) | house_challenge_04 |
| House (4) | house_challenge_05 |
| House (5) | house_challenge_06 |
| House (6) | house_challenge_07 |

## Como probar cada trigger

1. Cargar `Demo`.
2. Entrar al trigger runtime hijo `ChallengeTrigger` de una casa.
3. Confirmar que aparece el panel temporal.
4. Pulsar `Simular victoria`, `Simular derrota` o `Cancelar`.
5. Verificar que el HUD cambie solo con victoria.

## Como reiniciar progreso

Llamar:

```csharp
ChallengeProgressManager.Instance.ResetProgress();
```

El progreso se puede reiniciar manualmente con la llamada anterior. El flujo actual ya no reinicia progreso desde el lobo inicial de `Demo`.

## Configuracion de Castle

`SevenChallengesSceneBootstrap` agrega `CastleUnlockController` al objeto `Castle` en runtime y crea un hijo `FinalBattleTrigger` con `FinalCastleTrigger`.

## Configuracion del lobo inicial

`SevenChallengesSceneBootstrap` desactiva `Enemy_Wolf_Model` en `Demo`. El lobo debe estar activo en `_DemoScene`, que es la escena cargada por el trigger final del castillo.

Al cargar `_DemoScene`, `ArenaBattleReturnController` marca ese lobo como `RegularEnemy`, desactiva los cargadores directos a `Ganaste` y configura `Portal_Trigger`, hijo de `Portal`, como salida hacia `Demo`.

## Configuracion del jefe final

El jefe final debe tener `EnemyHealth` con rol `FinalBoss` o un `EnemyRoleMarker` configurado como `FinalBoss`. Solo ese rol dispara `Ganaste`.

## Retorno desde la batalla

El trigger `Portal/Portal_Trigger` de `_DemoScene` se habilita al derrotar al lobo, vuelve a `Demo`, salta la cinematica inicial de retorno y teletransporta al jugador a:

```text
x = -0.105
y = 0
z = 0.7049999
```

## Referencias necesarias en Inspector

La implementacion runtime reduce configuracion manual. Para el flujo actual no hace falta crear `CityEntryPoint` en `Demo`.

## Problemas pendientes

- Hay cambios sin commitear previos en `Assets/Project/Scenes/Castillo/Scenes/Demo.unity` y `Assets/TextMesh Pro/Fonts/Cinzel-Regular SDF.asset`; no se incluyeron en estos commits.
- No se ejecuto Play Mode desde este entorno.
- Validar en Play Mode que `Enemy_Wolf_Model` no aparezca en `Demo`, si aparezca al cargar `_DemoScene` desde la puerta del castillo y que el trigger de salida vuelva a `Demo` en `(-0.105, 0, 0.7049999)`.

## Commits creados

- `docs: plan seven challenge progression system`
- `feat: add challenge core contracts and folder structure`
- `feat: add seven challenge integration bridges`
- `feat: add persistent challenge progress manager`
- `feat: add reusable house challenge trigger`
- `feat: add temporary challenge test panel`
- `feat: add challenge progress HUD`
- `feat: add central castle unlock and final trigger`
- `feat: classify initial wolf and final boss outcomes`
- `feat: teleport player after initial wolf victory`
- `feat: configure seven house challenge triggers`
- `fix: avoid obsolete bootstrap object lookup`
- `docs: add challenge game integration guide`
- `docs: update challenge implementation notes`
