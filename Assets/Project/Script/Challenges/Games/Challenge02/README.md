# Cofre con combinacion

## Objetivo

Cofre con combinacion es el segundo reto de las siete casas. El jugador debe leer una pista medieval e introducir una combinacion de tres numeros para abrir un cofre.

## Integracion

- Casa: `House (1)`.
- ID: `house_challenge_02`.
- Constante: `ChallengeProgressManager.HouseChallenge02`.
- Bridge: `Challenge02GameBridge`.
- Escena: `Assets/Project/Scenes/Castillo/Scenes/Demo.unity`.
- Prefab runtime: `Assets/Resources/Challenges/Challenge02/ChestCombinationPanel.prefab`.
- Carga runtime: `Resources.Load<GameObject>("Challenges/Challenge02/ChestCombinationPanel")`.

`SevenChallengesSceneBootstrap` crea o reutiliza `ChallengesCanvas`, asegura un `EventSystem` con `InputSystemUIInputModule`, instancia el prefab del panel y configura `Challenge02GameBridge`.

## Mecanica

La combinacion por defecto es:

```text
2 - 3 - 1
```

La pista por defecto es:

```text
Dos caballeros custodian tres antorchas y una corona.
```

La combinacion y la pista son configurables desde el Inspector en `ChestCombinationGameController`.

## Flujo

- Al abrir: muestra cofre cerrado, pista, tres casillas vacias, botones 0-9, `CONFIRMAR`, `BORRAR` y `SALIR`.
- Numero: agrega el valor en la primera casilla disponible y reproduce `number_press.mp3`.
- Borrar: elimina el ultimo numero si existe.
- Confirmar incompleto: muestra `Introduce los tres numeros.` y no evalua.
- Combinacion incorrecta: reproduce `incorrect.mp3`, muestra error y habilita `REINTENTAR`; no envia `Lost`.
- Reintentar: limpia casillas y reactiva el teclado sin cerrar el reto.
- Combinacion correcta: reproduce audio correcto, abre el cofre, muestra llave y moneda, reproduce victoria y envia `ChallengeResult.Won`.
- Salir: cancela con `CancelChallenge()`.

## Scripts

- `Challenge02GameBridge.cs`: mantiene el tipo usado por el binding de `SevenChallengesSceneBootstrap` y hereda del controlador real.
- `Scripts/ChestCombinationGameController.cs`: controla estados, combinacion, entradas, reintento, victoria, cancelacion y audio.
- `Scripts/ChestCombinationPanel.cs`: muestra/oculta UI, construye el panel, conecta botones, actualiza textos, cofre y recompensas.
- `Scripts/ChestNumberButton.cs`: representa un boton numerico de 0 a 9.
- `Scripts/CombinationSlot.cs`: representa cada casilla de combinacion.

## Recursos

- Cofre: `Art/Chest/chest_closed.png`, `chest_open.png`, `chest_lock.png`.
- UI: `Art/UI/main_frame.png`, `clue_scroll.png`, `number_slot.png`, `confirm_button.png`, `clear_button.png`, `retry_button.png`, `exit_button.png`.
- Recompensas: `Art/Rewards/golden_key.png`, `gold_coin.png`.
- Audio: `Audio/ambience.mp3`, `number_press.mp3`, `correct.mp3`, `incorrect.mp3`, `chest_open.mp3`, `victory.mp3`.

No existe un sprite independiente para boton numerico; se reutiliza `number_slot.png` para los 10 botones y se muestra el numero con TextMeshPro.

## Estados

- `Inactive`
- `Ready`
- `EnteringCombination`
- `Checking`
- `Incorrect`
- `OpeningChest`
- `Completed`
- `Cancelling`

Estos estados impiden doble inicio, doble resultado, entradas durante verificacion, reintentos invalidos y cancelacion despues de victoria.

## Resultado

El minijuego no llama directamente a `ChallengeProgressManager.CompleteChallenge`. Al ganar llama `SubmitResult(ChallengeResult.Won)` y `HouseChallengeTrigger` registra `house_challenge_02`.

## Cambiar combinacion o pista

En el componente que hereda de `ChestCombinationGameController`:

- `correctCombination` debe tener exactamente tres numeros.
- Cada numero debe estar entre `0` y `9`.
- `clueText` acepta texto multilinea.

## Pruebas

- Confirmar que `Juego_2/` esta ignorada y no staged.
- Confirmar que el prefab runtime existe en `Assets/Resources/Challenges/Challenge02/`.
- Entrar a `House (1)` en `Demo`.
- Probar numero, borrar, confirmar incompleto, error, reintentar, salir y combinacion correcta.
- Volver a entrar despues de ganar para confirmar que no suma dos veces.
- Entrar a `House` para confirmar que Memoria de Runas sigue aislado.

## Riesgos

- No duplicar `ChallengesCanvas`.
- No duplicar `EventSystem`.
- No usar `AssetDatabase`, `PrefabUtility` ni `UnityEditor` en runtime.
- No modificar `Demo.unity` para esta integracion.
- No incluir `Juego_2/` en commits.

## Commits creados

- `chore: ignorar recursos temporales del juego dos`
- `feat: organizar recursos del cofre con combinacion`
- `feat: agregar logica del cofre con combinacion`
- `feat: agregar prefab ui del cofre con combinacion`
- `feat: integrar cofre con combinacion en la segunda casa`
