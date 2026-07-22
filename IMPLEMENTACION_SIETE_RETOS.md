# Plan de implementacion de siete retos

## Rama y base

- Rama actual: `CrisP`.
- No se trabaja sobre `main`.
- No se hara push ni merge.

## Arquitectura existente que se reutilizara

- `GameEvents` para mensajes desacoplados mediante `OnMessageRequested`.
- `PlayerController` como controlador principal del perro.
- `DogHealth`, `EnemyHealth`, `BasicAttack`, `CombatGameManager` y `VictoriaCargarEscena` como base del combate existente.
- `ZoneTrigger` solo se mantiene para triggers ya existentes; los retos usaran un trigger especifico para no mezclar responsabilidades.
- `EditorBuildSettings.asset` confirma que la escena de pelea disponible es `_DemoScene`.

## Componentes nuevos

- Contratos base de retos:
  - `IChallengeGame`
  - `ChallengeResult`
  - `ChallengeGameController`
- Progreso:
  - `ChallengeProgressManager`
- Integracion por casas:
  - `HouseChallengeTrigger`
  - `Challenge01GameBridge` hasta `Challenge07GameBridge`
- Bloqueo de control:
  - `PlayerControlLock`
- Pruebas temporales:
  - `ChallengeTestPanel`
- HUD:
  - `ChallengeProgressHUD`
- Castillo:
  - `CastleUnlockController`
  - `FinalCastleTrigger`
- Enemigos y transicion:
  - `EnemyRole`
  - `EnemyRoleMarker`
  - `InitialWolfVictoryTransition`

## Componentes existentes que se modificaran

- `EnemyHealth`: para publicar una muerte con rol sin cargar siempre la victoria final.
- `VictoriaCargarEscena`: para cargar `Ganaste` solo cuando el enemigo derrotado sea `FinalBoss`.
- `GameEvents`: para agregar un evento de enemigo derrotado con rol, manteniendo el evento antiguo por compatibilidad.

## Escena afectada

- `Assets/Project/Scenes/Castillo/Scenes/Demo.unity`
  - Contiene `Player_Dog_Model`.
  - Contiene `Enemy_Wolf_Model`, que se desactiva en runtime para que no aparezca durante la exploracion.
  - Contiene las casas `House`, `House (1)`, `House (2)`, `House (3)`, `House (4)`, `House (5)`, `House (6)`.
  - Contiene `Castle`.

## Persistencia

- Se usara un singleton seguro con `DontDestroyOnLoad` en `ChallengeProgressManager`.
- El progreso se guardara adicionalmente en `PlayerPrefs` con claves especificas del sistema de retos.
- No se borraran preferencias ajenas.

## Identificacion de casas

| Objeto | ID |
|---|---|
| House | house_challenge_01 |
| House (1) | house_challenge_02 |
| House (2) | house_challenge_03 |
| House (3) | house_challenge_04 |
| House (4) | house_challenge_05 |
| House (5) | house_challenge_06 |
| House (6) | house_challenge_07 |

## Desbloqueo de Castle

- `CastleUnlockController` consultara `ChallengeProgressManager`.
- Antes de 7/7 mostrara mensajes con retos faltantes.
- Al completar 7/7 emitira el mensaje de desbloqueo una sola vez.
- No se desactivara ni movera `Castle`.

## Diferencia entre lobo de batalla y jefe final

- Se agregara `EnemyRoleMarker` con valores:
  - `InitialWolf`
  - `RegularEnemy`
  - `FinalBoss`
- El lobo de `Demo` permanecera desactivado durante la exploracion.
- El lobo aparecera cuando el trigger final cargue `_DemoScene`.
- `FinalBoss` sera el unico rol que puede cargar `Ganaste`.

## Transforms registrados antes de tocar triggers

| Objeto | Position | Rotation |
|---|---|---|
| House | `(-5.42, 0.8406752, 13.54)` | `(0, 0, 0, 1)` |
| House (1) | `(-11.09364, 0.8406752, 10.765364)` | `(0, 0.40440816, 0, 0.9145787)` |
| House (2) | `(-11.518974, 0.8406752, 4.9260836)` | `(0, 0.3826302, 0, 0.9239016)` |
| House (3) | `(-6.7854514, 1.041, 1.7815752)` | `(0, 0.7070692, 0, 0.7071444)` |
| House (4) | `(-0.298077, 0.8406752, -0.5425432)` | `(0, -0.38276446, 0, 0.923846)` |
| House (5) | `(3.5020633, 0.8406752, 4.660003)` | `(0, -0.3535587, 0, 0.93541235)` |
| House (6) | `(2.33, 0.8406752, 12.610001)` | `(0, 0, 0, 1)` |
| Castle | `(-2.6394413, 3.310215, 8.242853)` | `(0, 0, 0, 1)` |

Las escalas no aparecen como overrides en esas instancias, por lo que se consideran las escalas originales de sus prefabs.
