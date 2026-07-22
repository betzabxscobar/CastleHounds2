# Integracion de minijuegos de casas

Esta carpeta contiene un punto de integracion por cada reto futuro. Los minijuegos definitivos no estan implementados aqui.

## Flujo para crear un juego futuro

1. Abrir la carpeta correspondiente, por ejemplo `Challenge01`.
2. Implementar la logica visual y reglas propias del minijuego en scripts nuevos.
3. Reutilizar el bridge existente, por ejemplo `Challenge01GameBridge`.
4. Cuando el jugador gane, llamar `SimulateVictory()` o exponer un metodo propio que termine en `SubmitResult(ChallengeResult.Won)`.
5. Cuando el jugador pierda, terminar con `ChallengeResult.Lost`.
6. Si el jugador cierra o cancela, terminar con `ChallengeResult.Cancelled`.

## Juegos implementados

- `Challenge01`: Memoria de Runas, integrado con `House`.
- `Challenge02`: Cofre con combinacion, integrado con `House (1)`.

## Archivos que no deben modificarse para agregar un juego

- `HouseChallengeTrigger`
- `ChallengeProgressManager`
- `CastleUnlockController`
- `FinalCastleTrigger`
- `ChallengeProgressHUD`
- `InitialWolfVictoryTransition`

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

## Asociacion al trigger

`SevenChallengesSceneBootstrap` configura automaticamente los bridges al cargar `Demo`. Si mas adelante se decide configurar la escena manualmente, cada `HouseChallengeTrigger` debe apuntar al bridge que corresponda a su ID.
