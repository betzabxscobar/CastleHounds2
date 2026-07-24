# RecursosChallengue04

Esta carpeta es un paquete de traslado para reimplementar limpiamente el minijuego de pociones que antes vivia en `Challenge03`, pero ahora pensado para reconstruirse como `Challenge04` en el nuevo `main`.

No incluye recursos de personaje externos. Dentro de la carpeta original `Challenge03` no se encontraron assets del personaje viejo, perro, Ares, German Shepherd, Wolf o Lobo. El personaje/controlador debe venir del nuevo `main`.

## Contenido

### Challenge03_Source

Copia completa de:

`Assets/Project/Script/Challenges/Games/Challenge03`

Incluye la escena, scripts, input, UI, sonidos, fuentes, modelos, prefabs, materiales y texturas usados por el minijuego de pociones.

Piezas principales:

- `MiniJuegoPocion.unity`: escena original del minijuego.
- `Challenge03GameBridge.cs`: puente viejo con el sistema de retos.
- `PotionInput.inputactions`: asset de input del minijuego.
- `PotionInput.cs`: wrapper generado por Unity Input System.
- `Scripts/`: logica principal del minijuego.
- `Scripts/PotionS/`: datos y logica de poderes/recetas.
- `Scripts/Recipes/`: recetas como assets.
- `Sounds/`: audios del minijuego.
- `Fonts/`: fuentes TextMesh Pro usadas por la UI.
- `Recuadros/`: imagenes de interfaz.
- `LowPolyDungeonsLite/`: prefabs, modelos, materiales y texturas de escenario.
- `StylizedMagicPotion/`: botellas, materiales, texturas y prefabs de pociones.
- `Nature/`: pastos/modelos naturales.
- `Toby Fredson/Hand Painted Tiles - SleepingForest/`: hongos, texturas, materiales y postprocessing.
- `Models/`: prefabs/materiales extra usados por la escena.

### IntegrationReference

Estos scripts no son para copiarlos automaticamente al nuevo `main` si ya existen versiones nuevas. Estan aqui como referencia para entender como se conectaba antes el reto al sistema de siete casas.

- `Core/ChallengeGameController.cs`
- `Core/IChallengeGame.cs`
- `Core/ChallengeResult.cs`
- `Core/ChallengeProgressManager.cs`
- `HouseTriggers/HouseChallengeTrigger.cs`
- `Integration/SevenChallengesSceneBootstrap.cs`
- `Integration/PlayerControlLock.cs`

## Como se conectaba antes

El minijuego estaba registrado como el reto 03:

```csharp
public sealed class Challenge03GameBridge : ChallengeGameController
{
    protected override string FallbackChallengeId => "house_challenge_03";
}
```

En `SevenChallengesSceneBootstrap.cs`, el binding anterior era:

```csharp
("House (2)", ChallengeProgressManager.HouseChallenge03, typeof(Challenge03GameBridge))
```

Eso significa:

- Casa anterior: `House (2)`.
- Id anterior: `house_challenge_03`.
- Bridge anterior: `Challenge03GameBridge`.
- Sistema anterior: `ChallengeGameController` + `HouseChallengeTrigger` + `ChallengeProgressManager`.

Para el nuevo `main`, no se debe asumir que esos nombres de casa, personaje, input o player controller siguen existiendo. Hay que adaptar el minijuego al sistema actual.

## Recomendacion para reimplementarlo como Challenge04

Crear una nueva carpeta real del juego en el nuevo `main`, por ejemplo:

`Assets/Project/Script/Challenges/Games/Challenge04`

Luego migrar los recursos desde `Challenge03_Source`, pero renombrando/adaptando la integracion:

- Cambiar `Challenge03GameBridge` por `Challenge04GameBridge`.
- Cambiar el id viejo `house_challenge_03` por el id nuevo que use el nuevo main, probablemente `house_challenge_04`.
- Revisar `MiniJuegoPocion.unity` y reemplazar referencias al player/personaje viejo por el sistema de jugador nuevo.
- Mantener los scripts propios de pociones si compilan con el nuevo proyecto.
- No copiar controladores viejos de personaje.
- No forzar `SevenChallengesSceneBootstrap.cs` viejo si el nuevo main tiene otro sistema de retos.

## Prompt para otro agente

Usa este prompt para pedir la implementacion limpia:

```text
Necesito implementar limpiamente el minijuego de pociones como Challenge04 en el nuevo main de Unity.

Hay una carpeta de recursos en Assets/RecursosChallengue04. Dentro esta Challenge03_Source, que contiene el minijuego original completo de pociones, y IntegrationReference, que solo sirve para entender como se conectaba antes al sistema viejo.

Objetivo:
- Crear o actualizar el Challenge04 del nuevo main usando los recursos de Assets/RecursosChallengue04/Challenge03_Source.
- No copiar ni depender de personajes, controlador de jugador, perro, Ares, German Shepherd, Wolf/Lobo ni ningun sistema viejo de personaje.
- Usar el personaje y sistema de jugador actual del nuevo main.
- No reemplazar sistemas nuevos del main con scripts viejos de IntegrationReference. Esa carpeta es solo referencia.
- Adaptar la integracion al sistema actual de retos del nuevo main.
- El reto debe registrarse como Challenge04, no como Challenge03.
- El id del reto debe ser house_challenge_04 si el nuevo sistema conserva ese esquema.
- Renombrar/adaptar Challenge03GameBridge a Challenge04GameBridge si hace falta.
- Hacer que el minijuego pueda iniciarse desde la casa/reto correspondiente del nuevo main, bloquear/desbloquear control del jugador segun el sistema nuevo, y reportar victoria/derrota al progreso actual.
- Revisar y corregir referencias rotas de escena, prefabs, materiales, TextMesh Pro, input actions, audio y recetas.
- Mantener la logica propia de pociones: ingredientes, drag/input, caldero, recetas, UI, sonidos y escena.
- Al final, verificar que compile en Unity y documentar los archivos modificados.

Importante:
- No hacer merge de la rama vieja.
- No traer sistemas obsoletos completos si el nuevo main ya tiene reemplazos.
- Usar los recursos como materia prima, no como imposicion arquitectonica.
```
