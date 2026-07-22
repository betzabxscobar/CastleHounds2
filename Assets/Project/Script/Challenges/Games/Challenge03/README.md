# Challenge03 - Acertijo

Minijuego de preguntas y respuestas creado por Adrian, integrado como el
tercer reto de las casas.

- Casa: `House (2)`
- Challenge ID: `house_challenge_03`
- Bridge: `Challenge03GameBridge`

## Estructura

- `Art/` — sprites originales (fondo, recuadro, boton, imagen vacia).
- `Audio/` — sonidos de respuesta correcta e incorrecta.
- `Scripts/AcertijoManager.cs` — mecanica original del acertijo (preguntas,
  respuestas, progreso, sonidos). Ya no destruye objetos del sistema de
  retos al terminar: expone `Show()`, `Hide()`, `OnWon` y `OnExitRequested`
  para que el bridge controle su ciclo de vida.
- `Scripts/Pregunta.cs` — modelo de pregunta (sin cambios).
- `Challenge03GameBridge.cs` — conecta `AcertijoManager` con
  `ChallengeGameController` (`FallbackChallengeId = "house_challenge_03"`).

## Prefab runtime

`Assets/Resources/Challenges/Challenge03/AcertijoPanel.prefab`, cargado por
`SevenChallengesSceneBootstrap` via:

```csharp
Resources.Load<GameObject>("Challenges/Challenge03/AcertijoPanel")
```

Se instancia una sola vez dentro de `ChallengesCanvas`, igual que
`RuneMemoryPanel` (Challenge01) y `ChestCombinationPanel` (Challenge02).

## Escena fuente

`Assets/Project/Scenes/Minijuegos/Acertijo.unity` se conserva unicamente
como escena de origen/prueba del diseño visual de Adrian. No se agrega a
Build Settings ni se carga en runtime: el minijuego se juega como panel
dentro de `Demo`.
