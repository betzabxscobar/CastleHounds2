# Entrega V2 - Modelos, escala, colliders y daño

Esta carpeta queda limpia para tu parte. No incluye scripts de movimiento, camara, IA ni sistemas viejos.

Contenido:

- `PersonajePrincipal`: perro/Ares con modelo 3D, prefab, materiales, texturas, animaciones y script simple para recibir daño.
- `Lobo`: lobo con modelo 3D, prefab, materiales, texturas, animaciones y shader graph de pelaje.

El script `PersonajePrincipal/CharacterDamageReceiverV2.cs` se puede usar tanto en el perro como en el lobo. No mueve al personaje. Solo permite que el objeto reciba daño con `TakeDamage(float amount)`.

## Tu responsabilidad

CRISTOFER PRADO  
Encargado de modelos 3D, escalas y colliders.

Tarea:

- Revisar escala del perro.
- Revisar escala del lobo.
- Revisar escala del castillo nuevo de V2.
- Revisar puertas, paredes, casas, columnas y objetos.
- Poner colliders en paredes y objetos solidos.
- Evitar que el perro atraviese estructuras.
- Corregir objetos flotando o mal ubicados.

Entrega:

- Escenario con modelos en escala correcta y limites funcionales.
- Perro y lobo listos para que otro companero agregue movimiento.

## Archivos principales

Perro:

- `PersonajePrincipal/Ares_Visual.prefab`
- `PersonajePrincipal/Prefabs_HDRP/P_GermanShepherd.prefab`
- `PersonajePrincipal/Models/SK_GermanShepherd_01.fbx`
- `PersonajePrincipal/AresLocomotion.controller`
- `PersonajePrincipal/Ares_Idle.anim`
- `PersonajePrincipal/Ares_Run.anim`
- `PersonajePrincipal/Materials_URP`
- `PersonajePrincipal/Textures_HDRP`

Lobo:

- `Lobo/Prefab_URP/Wolf_URP.prefab`
- `Lobo/Models/Wolf.fbx`
- `Lobo/Animations`
- `Lobo/Materials_URP`
- `Lobo/Textures_URP`
- `Lobo/FurShader_Graph`

Script de daño:

- `PersonajePrincipal/CharacterDamageReceiverV2.cs`

## Si el lobo aparece rosado

El lobo usa materiales de pelaje que dependen del shader graph:

- `Lobo/FurShader_Graph/AnimalFurShader_URP_HDRP.shadergraph`

Si el lobo sale rosado en Unity, normalmente falta ese shader o el proyecto no lo ha importado bien.

Pasos:

1. Confirmar que tambien se copio/importo `Lobo/FurShader_Graph`.
2. Confirmar que se mantuvo el archivo `.meta` del shader graph.
3. Reimportar la carpeta `Lobo` en Unity.
4. Revisar los materiales:
   - `Lobo/Materials_URP/Wolf_URP.mat`
   - `Lobo/Materials_URP/FurWolf_URP.mat`
   - `Lobo/Materials_URP/HairWolf_URP.mat`
   - `Lobo/Materials_URP/EyeGlassWolf_URP.mat`
5. Si sigue rosado, asignar manualmente el shader `AnimalFurShader_URP_HDRP` a `FurWolf_URP` y `HairWolf_URP`.
6. Si el proyecto V2 usa URP, verificar que tenga instalados/activos Universal Render Pipeline y Shader Graph.

## Como dejar listo el perro en V2

1. Crea un GameObject raiz llamado `Player_Dog_Model`.
2. Mete el modelo/prefab del perro como hijo.
3. Ajusta la escala visual del perro desde el hijo, no desde todos los sistemas del jugador.
4. Agrega al GameObject raiz:
   - `CharacterDamageReceiverV2`
   - `CapsuleCollider` o `CharacterController`
5. Ajusta el collider:
   - Debe cubrir el cuerpo principal del perro.
   - No necesita cubrir toda la cola.
   - La base del collider debe tocar el suelo.
   - El centro debe quedar alineado con el cuerpo.
6. Marca el collider como solido. No activar `Is Trigger`.

Recomendacion inicial para el perro:

- Escala visual: dejarlo parecido a un perro mediano/grande frente a una puerta del castillo.
- Collider: usar forma tipo capsula, centrada en pecho/cuerpo.
- Vida inicial en `CharacterDamageReceiverV2`: 100.

## Como dejar listo el lobo en V2

1. Crea un GameObject raiz llamado `Enemy_Wolf_Model`.
2. Mete el modelo/prefab del lobo como hijo.
3. Ajusta la escala visual para que sea similar al perro.
4. Agrega al GameObject raiz:
   - `CharacterDamageReceiverV2`
   - `CapsuleCollider` o `CharacterController`
5. Ajusta el collider igual que el perro:
   - Cubrir cuerpo principal.
   - Base tocando el suelo.
   - Sin `Is Trigger`.

Recomendacion inicial para el lobo:

- Debe verse del mismo rango de tamano que el perro.
- Puede ser apenas mas grande si el diseno lo requiere, pero no gigante.
- Vida inicial sugerida: 100.

## Escala correcta

Usa el castillo nuevo de V2 como referencia:

- El perro debe poder verse coherente al lado de puertas, paredes y casas.
- El lobo debe tener escala cercana al perro.
- Las puertas deben verse suficientemente grandes para permitir paso si estan abiertas.
- Las casas, columnas y paredes no deben verse miniatura ni exageradas frente a los personajes.

Orden recomendado:

1. Ajustar escala del castillo o piezas principales.
2. Ajustar escala visual del perro.
3. Ajustar escala visual del lobo.
4. Ajustar colliders de perro y lobo.
5. Ajustar colliders de paredes, puertas, casas, columnas y objetos solidos.

## Colliders del escenario

Para evitar que el perro atraviese estructuras, el escenario necesita colliders solidos.

Usa:

- `BoxCollider`: paredes rectas, puertas, casas simples, limites invisibles.
- `CapsuleCollider`: columnas redondas o postes.
- `MeshCollider`: solo para formas complejas donde un collider simple no sirve.

Reglas:

- Paredes, casas, columnas y objetos solidos deben tener collider.
- Los colliders solidos no deben tener `Is Trigger`.
- El suelo debe tener collider.
- Las puertas cerradas deben bloquear.
- Las puertas abiertas deben permitir paso si el diseno lo requiere.

## Objetos flotando o mal ubicados

Revisar cada pieza visible:

1. Seleccionar objeto.
2. Confirmar que la base toca el suelo.
3. Si flota, bajarlo.
4. Si esta hundido, subirlo.
5. Revisar que el collider acompanhe al modelo.

## Limites funcionales

Si el jugador puede salirse del mapa:

1. Crear objetos vacios `WorldLimit_North`, `WorldLimit_South`, `WorldLimit_East`, `WorldLimit_West`.
2. Agregar `BoxCollider`.
3. Hacerlos altos y largos.
4. No activar `Is Trigger`.
5. Dejarlos invisibles, pero solidos.

## Pruebas antes de entregar

Probar en Play Mode cuando el companero conecte movimiento:

- El perro no atraviesa paredes.
- El perro no atraviesa columnas.
- El perro no atraviesa casas.
- El perro no atraviesa puertas cerradas.
- El perro no sale del escenario.
- El perro y el lobo tienen escala coherente entre si.
- Los dos personajes pueden recibir daño usando `TakeDamage`.
- No hay objetos flotando ni hundidos.

## Nota importante

Tu carpeta no contiene scripts de movimiento porque esa parte la hara otro companero. Tu entrega debe dejar modelos, escalas, colliders y daño listos para que el movimiento se conecte despues sin romper las colisiones.
