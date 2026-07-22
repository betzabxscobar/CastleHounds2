# Entrega Cristofer - Modelos, escala, colliders y limites

## Escala usada

- Perro: `Player_Dog_Model` como raiz, con `Ares_Visual` como hijo visual a escala `1.35`.
- Lobo: `Enemy_Wolf_Model` como raiz, con `Wolf_Visual` como hijo visual a escala `1`.
- Ambos quedan ubicados sobre el suelo del escenario del castillo para comparar tamano con puertas, paredes y casas.

## Colliders de personajes

- `Player_Dog_Model`: `CapsuleCollider` solido, sin `Is Trigger`, orientado en eje Z para cubrir el cuerpo principal.
- `Enemy_Wolf_Model`: `CapsuleCollider` solido, sin `Is Trigger`, orientado en eje Z para cubrir el cuerpo principal.
- La base de cada collider queda alineada para tocar el suelo.

## Dano recibido

- `Player_Dog_Model` tiene `CharacterDamageReceiverV2` con vida inicial `100`.
- `Enemy_Wolf_Model` tiene `CharacterDamageReceiverV2` con vida inicial `100`.
- Ambos quedan listos para recibir dano con `TakeDamage(float amount)`.

## Colliders del escenario

- Se revisaron los prefabs del castillo nuevo: paredes, casas, puertas, columnas, puente, castillo y suelo ya tienen colliders solidos.
- Se mantuvieron los colliders existentes para no duplicar fisicas ni tocar sistemas de otros companeros.

## Limites creados

- `WorldLimit_North`
- `WorldLimit_South`
- `WorldLimit_East`
- `WorldLimit_West`

Cada limite usa `BoxCollider` solido, sin `Is Trigger`, y no tiene renderer visible.

## Pruebas realizadas

- Confirmada la importacion de perro, lobo y `CharacterDamageReceiverV2` dentro de `Assets/Project/V2_Cristofer`.
- Confirmada la presencia de `Player_Dog_Model` y `Enemy_Wolf_Model` en la escena del castillo.
- Confirmado que los personajes tienen `CapsuleCollider` y `CharacterDamageReceiverV2`.
- Confirmado que los limites invisibles tienen `BoxCollider` solido.
- Confirmado que los prefabs principales del escenario ya contienen colliders para bloquear movimiento cuando se conecte despues.
