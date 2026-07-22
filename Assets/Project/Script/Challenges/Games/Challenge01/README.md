# Memoria de Runas

## Objetivo

Memoria de Runas es el primer reto de las siete casas. El jugador observa una secuencia de cuatro runas posibles y debe repetirla en el mismo orden.

## Flujo del juego

El reto se abre cuando el perro entra al trigger de `House` configurado con `house_challenge_01`. `HouseChallengeTrigger` bloquea el control del jugador y llama a `Challenge01GameBridge.StartChallenge()`. El bridge hereda de `RuneMemoryGameController`.

Al abrirse, el panel muestra las runas, el boton `COMENZAR`, el boton `SALIR` y el texto inicial. Al pulsar comenzar, la secuencia se reproduce con botones bloqueados. Despues se habilita la entrada del jugador.

## Reglas

- Ronda 1: secuencia de 3 runas.
- Ronda 2: secuencia de 4 runas.
- Ronda 3: secuencia de 5 runas.
- Una respuesta correcta avanza al siguiente indice.
- Completar una ronda reproduce `correcta.mp3`.
- Fallar una runa reproduce `error.mp3`, muestra `REINTENTAR` y no completa el reto.
- Completar las tres rondas reproduce `victoria.mp3` y envia `ChallengeResult.Won`.
- Pulsar `SALIR` cancela el reto con `CancelChallenge()`.

## Scripts

- `Challenge01GameBridge.cs`: mantiene el tipo usado por `SevenChallengesSceneBootstrap` y hereda del controlador real.
- `Scripts/RuneMemoryGameController.cs`: controla estados, secuencias, audio, victoria, fallo, reintento y cancelacion.
- `Scripts/RuneMemoryButton.cs`: encapsula cada boton de runa, su indice, click e iluminacion.
- `Scripts/RuneMemoryPanel.cs`: muestra/oculta UI, actualiza textos y conecta botones.

## Recursos

- Runas: `Art/Runes/espada.png`, `escudo.png`, `lobo.png`, `corona.png`.
- UI: `Art/UI/marco.png`, `comenzar.png`, `reintentar.png`, `salir.png`.
- Audio: `Audio/ambiente.mp3`, `iluminar.mp3`, `correcta.mp3`, `error.mp3`, `victoria.mp3`.
- Prefab runtime: `Assets/Resources/Challenges/Challenge01/RuneMemoryPanel.prefab`.
- Prefab fuente anterior: `Prefabs/RuneMemoryPanel.prefab`.

## Configuracion de sprites

Los PNG deben importarse como `Sprite (2D and UI)`, `Single`, `Pixels Per Unit` 100, `Alpha Is Transparency` activo, `Full Rect`, filtro bilinear y compresion sin perdida o alta calidad. Las runas usan tamano maximo 512; marco y botones usan 1024.

## Configuracion de audio

El bootstrap configura dos `AudioSource` locales sobre `RuneMemoryPanel`: uno para efectos sin loop y otro para ambiente con loop. No se modifican `AudioManager` ni `MusicManager`.

## Integracion

`SevenChallengesSceneBootstrap` asegura un unico `ChallengesCanvas`, un `EventSystem` con `InputSystemUIInputModule`, instancia `RuneMemoryPanel.prefab` con `Resources.Load<GameObject>("Challenges/Challenge01/RuneMemoryPanel")` y llama a `RuneMemoryGameController.ConfigureRuntime(...)` para conectar panel, botones y clips.

El reto 1 ya no se agrega al `ChallengeTestPanel`; las casas 2 a 7 siguen usando el panel temporal.

## Resultado

- Victoria: `SubmitResult(ChallengeResult.Won)`.
- Fallo de secuencia: no envia `Lost`; deja el panel abierto para reintentar sin cerrar el trigger.
- Cancelacion: `CancelChallenge()`.

## Dificultad

La dificultad se ajusta en `RuneMemoryGameController` con `totalRounds`, `initialSequenceLength`, `highlightDuration`, `gapBetweenRunes`, `prepareDelay`, `roundTransitionDelay` y `victoryDelay`.

## Agregar mas runas

Para agregar mas runas se debe ampliar el arreglo `runeButtons`, crear botones adicionales en el prefab o en `RuneMemoryPanel.CreateDefault(...)`, y asegurar indices unicos.

## Pruebas realizadas

- Unity batch compile con codigo de salida 0.
- Busqueda de errores `error CS` sin resultados.
- Creacion del prefab runtime bajo `Assets/Resources/Challenges/Challenge01/`.
- Busqueda de `UnityEditor`, `AssetDatabase`, `PrefabUtility` y `LoadAssetAtPath` confirma que el bootstrap ya no depende de esas APIs en runtime.
- `git status --ignored=matching` confirma que `/Juego_1/` esta ignorado.

## Problemas conocidos

El prefab runtime debe mantener serializados los sprites, `AudioSource` y `AudioClip`. Si se regenera el panel, verificar esas referencias antes de probar en build.

## Commits

- `chore: ignore temporary game one resources`
- `feat: organize rune memory game assets`
- `feat: add rune memory game core logic`
- `feat: add rune memory game ui prefab`
- `feat: integrate rune memory game with first house`
- `fix: load rune memory prefab in runtime builds`
