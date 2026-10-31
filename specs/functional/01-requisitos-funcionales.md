# 01 · Requisitos funcionales, historias y criterios de aceptación

| Campo   | Valor                  |
| ------- | ---------------------- |
| Versión | 1.2                    |
| Fecha   | 29/10/2026             |
| Estado  | Borrador para revisión |

Convención de IDs:

- **RF-xx**: requisito funcional
- **HU-xx**: historia de usuario
- **CA-xx.y**: criterio de aceptación _y_ del RF _xx_ (cada uno debe tener al menos una prueba)
- **RN-xx**: regla de negocio (ver [02-reglas-negocio.md](02-reglas-negocio.md))

Convenciones de respuesta de la API (decisiones de la auditoría, [auditoria-documentacion.md §9](../../docs/auditoria-documentacion.md#9-decisiones-tomadas)):

- Cada acción sobre un ticket (asignar, reasignar, iniciar atención, resolver, cerrar, reabrir, reclasificar, cambiar prioridad) es una operación propia, no una operación genérica de cambio de estado.
- Crear responde `201 Created` con el ticket (detalle) y el encabezado `Location`. Asignar, reasignar, las transiciones, reclasificar y cambiar la prioridad responden `200 OK` con el ticket actualizado (detalle). Las consultas responden `200 OK`.
- El solicitante y el técnico se devuelven como objeto `{ id, nombre }`.
- Orden de evaluación de errores: `401` → `404` → `403` → `400` → `409` (RNF-04, [03-requisitos-no-funcionales.md](03-requisitos-no-funcionales.md)).
- Permisos por rol: [matriz de permisos](02-reglas-negocio.md#matriz-de-permisos).

---

## RF-01 · Registrar y reclasificar el ticket

**Prioridad:** Obligatorio ·

**Reglas:** RN-01, RN-02, RN-03, RN-10, RN-13

**HU-01:** Como **solicitante**, quiero registrar un ticket describiendo mi problema, para que el área de TI lo atienda dentro del tiempo comprometido.

**HU-09:** Como **técnico asignado** o **supervisor**, quiero reclasificar un ticket mal clasificado, para que los reportes y la atención reflejen el tipo real de problema.

| ID       | Criterio de aceptación                                                                                                                                                                                             |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| CA-01.1  | **Dado** un solicitante válido, **cuando** envía título, descripción, categoría y prioridad válidos, **entonces** se crea el ticket con estado `Abierto` y un código único (`HD-000001`), y se responde `201 Created` con el ticket (detalle, ver CA-02.1) y el encabezado `Location`. |
| CA-01.2  | **Dado** un ticket recién creado, **entonces** su fecha de vencimiento es la fecha de creación más las horas de SLA de su prioridad (RN-10).                                                                       |
| CA-01.3  | **Dado** un título con menos de 5 o más de 150 caracteres, o una descripción con menos de 10 o más de 2000, **cuando** se envía, **entonces** se responde `400 Bad Request` con el detalle del error y no se crea el ticket. |
| CA-01.4  | **Dado** una categoría o prioridad inexistente, **cuando** se envía, **entonces** se responde `400 Bad Request`.                                                                                                   |
| CA-01.5  | **Dado** un usuario que no existe en el encabezado `X-Usuario-Id`, **cuando** intenta crear un ticket, **entonces** se responde `401 Unauthorized`.                                                                |
| CA-01.6  | **Dado** un ticket que no está `Resuelto` ni `Cerrado`, **cuando** su técnico asignado o un supervisor lo reclasifica con una categoría válida, **entonces** la categoría se actualiza y se responde `200 OK` con el ticket actualizado (detalle). |
| CA-01.7  | **Dado** un ticket `Resuelto` o `Cerrado`, **cuando** se intenta reclasificar, **entonces** se responde `409 Conflict` y la categoría no cambia.                                                                   |
| CA-01.8  | **Dado** un solicitante o un técnico que no es el asignado, **cuando** intenta reclasificar, **entonces** se responde `403 Forbidden`.                                                                             |
| CA-01.9  | **Dado** una categoría inexistente, **cuando** el técnico asignado o un supervisor intenta reclasificar, **entonces** se responde `400 Bad Request` y la categoría no cambia.                                       |
| CA-01.10 | **Dado** un técnico o un supervisor, **cuando** intenta crear un ticket, **entonces** se responde `403 Forbidden` y no se crea el ticket (RN-03).                                                                  |

---

## RF-02 · Consultar, filtrar y paginar tickets

**Prioridad:** Obligatorio ·

**Reglas:** RN-04, RN-05

**HU-02:** Como **supervisor**, quiero consultar los tickets filtrando por estado, prioridad, técnico o vencimiento, para tener visibilidad de la carga de trabajo.

**HU-03:** Como **solicitante**, quiero ver solo mis tickets y su estado, para saber cómo avanza mi ticket.

**HU-13:** Como **técnico**, quiero consultar los tickets asignados a mí, para saber qué tickets debo atender y en qué estado están.

| ID      | Criterio de aceptación                                                                                                                                                                                                            |
| ------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-02.1 | **Dado** un ticket existente, **cuando** se consulta por su Id, **entonces** se responde `200 OK` con el detalle completo: los atributos del ticket del [modelo de dominio](../technical/01-modelo-dominio.md#1-entidades), el estado del SLA, el solicitante y el técnico asignado como `{ id, nombre }`, y sus comentarios. Cada comentario tiene `id`, `tipo` (`Solucion` o `Reapertura`), `texto`, `autor` (`{ id, nombre }`) y `fechaCreacion`. |
| CA-02.2 | **Dado** un Id inexistente, **cuando** se consulta, **entonces** se responde `404 Not Found`.                                                                                                                                     |
| CA-02.3 | **Dado** un listado, **cuando** se envían filtros (`estado`, `prioridad`, `tecnicoId`, `soloVencidos`), **entonces** solo se devuelven los tickets que cumplen todos los filtros. `soloVencidos` devuelve solo tickets con estado del SLA `Vencido` (no incluye `Incumplido`). |
| CA-02.4 | **Dado** un listado, **entonces** se responde `200 OK` con `items`, `page`, `pageSize` y `totalItems`, ordenado por fecha de creación descendente. `page` vale 1 por defecto y `pageSize` 20 por defecto, con máximo 100. Cada elemento de `items` es un resumen: id, código, título, estado, prioridad, técnico asignado (`{ id, nombre }`), fecha de vencimiento y estado del SLA. Si `page` es menor a 1, o `pageSize` es mayor a 100 o menor a 1, se responde `400 Bad Request`. |
| CA-02.5 | **Dado** un solicitante, **cuando** lista tickets, **entonces** solo ve los suyos; un técnico solo ve los asignados a él; un supervisor ve todos (RN-04).                                                                         |
| CA-02.6 | **Dado** un solicitante que consulta por Id un ticket que no es suyo, o un técnico que consulta uno que no tiene asignado, **entonces** se responde `403 Forbidden`.                                                              |
| CA-02.7 | **Dado** un técnico, **cuando** lista tickets filtrando por el `tecnicoId` de otro técnico, **entonces** se responde `403 Forbidden`.                                                                                             |
| CA-02.8 | **Dado** un listado, **cuando** se envía un valor de filtro inválido, **entonces** se responde `400 Bad Request`.                                                                                                                 |

---

## RF-03 · Asignar y reasignar técnico

**Prioridad:** Obligatorio ·

**Reglas:** RN-06, RN-07

**HU-04:** Como **supervisor**, quiero asignar un ticket a un técnico, para que haya un responsable claro de su atención.

**HU-11:** Como **supervisor**, quiero reasignar un ticket a otro técnico activo, para que siga habiendo un responsable claro de su atención.

**HU-14:** Como **supervisor**, quiero consultar los técnicos activos, para saber a quién puedo asignar un ticket.

| ID      | Criterio de aceptación                                                                                                                                              |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-03.1 | **Dado** un ticket `Abierto` y un técnico activo, **cuando** el supervisor lo asigna, **entonces** el ticket queda con ese técnico, pasa a estado `Asignado` y se responde `200 OK` con el ticket actualizado (detalle). |
| CA-03.2 | **Dado** un ticket `Asignado` o `En progreso`, **cuando** el supervisor lo reasigna a otro técnico, **entonces** cambia el técnico, el ticket queda en `Asignado` y se responde `200 OK` con el ticket actualizado (detalle). |
| CA-03.3 | **Dado** un usuario que no existe, no tiene rol Técnico o no está activo, **cuando** se intenta asignar, **entonces** se responde `400 Bad Request`.                 |
| CA-03.4 | **Dado** un ticket que no está `Abierto` al asignar, o que no está `Asignado` ni `En progreso` al reasignar, **cuando** el supervisor intenta la operación, **entonces** se responde `409 Conflict` y el ticket no cambia. |
| CA-03.5 | **Dado** un usuario que no es supervisor, **cuando** intenta asignar, **entonces** se responde `403 Forbidden`.                                                     |
| CA-03.6 | **Dado** un supervisor, **cuando** consulta los técnicos activos, **entonces** se responde `200 OK` con la lista completa (sin paginar) de técnicos activos, cada uno como `{ id, nombre }`. |
| CA-03.7 | **Dado** un usuario que no es supervisor, **cuando** consulta los técnicos activos, **entonces** se responde `403 Forbidden`.                                       |
| CA-03.8 | **Dado** un ticket con técnico asignado, **cuando** el supervisor lo reasigna al mismo técnico, **entonces** se responde `400 Bad Request` y el ticket no cambia.  |

---

## RF-04 · Cambiar estado del ticket

**Prioridad:** Obligatorio ·

**Reglas:** RN-08, RN-09

**HU-05:** Como **técnico**, quiero registrar el avance de un ticket (iniciar atención y resolverlo), para que el solicitante y el supervisor conozcan su estado.

**HU-06:** Como **solicitante**, quiero cerrar mi ticket resuelto o reabrirlo si el problema persiste, para asegurar que mi problema quede realmente solucionado.

**HU-12:** Como **supervisor**, quiero cerrar un ticket resuelto, para dar por terminada su atención.

| ID      | Criterio de aceptación                                                                                                                                                                                  |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-04.1 | **Dado** un ticket `Asignado`, **cuando** su técnico asignado inicia la atención, **entonces** pasa a `En progreso` y se responde `200 OK` con el ticket actualizado (detalle).                        |
| CA-04.2 | **Dado** un ticket `En progreso`, **cuando** su técnico asignado lo resuelve con un comentario de solución, **entonces** pasa a `Resuelto`, se registra la fecha de resolución y se responde `200 OK` con el ticket actualizado (detalle). |
| CA-04.3 | **Dado** un intento de resolver sin comentario de solución, **entonces** se responde `400 Bad Request`.                                                                                                 |
| CA-04.4 | **Dado** un ticket `Resuelto`, **cuando** el solicitante dueño o el supervisor lo cierra, **entonces** pasa a `Cerrado`, se registra la fecha de cierre y se responde `200 OK` con el ticket actualizado (detalle). |
| CA-04.5 | **Dado** un ticket `Resuelto`, **cuando** el solicitante dueño lo reabre con un motivo, **entonces** vuelve a `En progreso` con el mismo técnico, se borra la fecha de resolución y se responde `200 OK` con el ticket actualizado (detalle). |
| CA-04.6 | **Dado** cualquier transición no permitida por la tabla de RN-08 (por ejemplo, `Abierto → Resuelto` o cualquier cambio desde `Cerrado`), **entonces** se responde `409 Conflict` y el estado no cambia. |
| CA-04.7 | **Dado** un usuario que no figura en RN-08 como autorizado para la acción (por ejemplo, un técnico que no es el asignado, el técnico asignado que intenta cerrar o reabrir, o un supervisor que intenta iniciar atención o resolver), **cuando** intenta ejecutarla, **entonces** se responde `403 Forbidden`. |
| CA-04.8 | **Dado** un intento de reabrir sin motivo, **entonces** se responde `400 Bad Request`.                                                                                                                 |
| CA-04.9 | **Dado** un comentario de solución o un motivo de reapertura de más de 2000 caracteres, **cuando** se envía, **entonces** se responde `400 Bad Request` y el estado no cambia.                          |

---

## RF-05 · Calcular y controlar el SLA

**Prioridad:** Obligatorio ·

**Reglas:** RN-10, RN-11, RN-12

**HU-07:** Como **supervisor**, quiero que cada ticket tenga una fecha de vencimiento según su prioridad y saber si se cumple, para medir la calidad del servicio.

**HU-10:** Como **supervisor**, quiero cambiar la prioridad de un ticket que no está resuelto ni cerrado, para que su vencimiento se recalcule según la nueva prioridad (RN-12).

| ID      | Criterio de aceptación                                                                                                                                                                            |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-05.1 | **Dado** un ticket con prioridad Crítica / Alta / Media / Baja, **entonces** su vencimiento es creación + 2 / 4 / 8 / 24 horas respectivamente.                                                   |
| CA-05.2 | **Dado** un ticket no resuelto cuya fecha actual supera el vencimiento, **cuando** se consulta, **entonces** su estado de SLA es `Vencido`; si no la supera, es `EnTiempo`.                       |
| CA-05.3 | **Dado** un ticket resuelto, **entonces** su estado de SLA es `Cumplido` si se resolvió antes o en el vencimiento, e `Incumplido` si se resolvió después.                                         |
| CA-05.4 | **Dado** un ticket no resuelto ni cerrado, **cuando** el supervisor cambia su prioridad, **entonces** el vencimiento se recalcula desde la fecha de creación con las horas de la nueva prioridad y se responde `200 OK` con el ticket actualizado (detalle). |
| CA-05.5 | **Dado** un ticket resuelto o cerrado, **cuando** se intenta cambiar la prioridad, **entonces** se responde `409 Conflict`.                                                                       |
| CA-05.6 | **Dado** un usuario que no es supervisor, **cuando** intenta cambiar la prioridad, **entonces** se responde `403 Forbidden` y la prioridad no cambia.                                              |
| CA-05.7 | **Dado** una prioridad inexistente, **cuando** el supervisor intenta cambiar la prioridad, **entonces** se responde `400 Bad Request` y la prioridad no cambia.                                    |

---

## RF-06 · Reporte de tickets vencidos _(deseable)_

**Prioridad:** Deseable ·

**Reglas:** RN-11

**HU-08:** Como **supervisor**, quiero un reporte de tickets vencidos agrupado por técnico y prioridad, para tomar acciones correctivas.

| ID      | Criterio de aceptación                                                                                                                                                      |
| ------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-06.1 | **Dado** tickets con SLA `Vencido`, **cuando** el supervisor solicita el reporte, **entonces** se responde `200 OK` con dos agrupaciones separadas, la cantidad por técnico y la cantidad por prioridad, y el total general. |
| CA-06.2 | **Dado** tickets sin técnico asignado y vencidos, **entonces** aparecen agrupados como "Sin asignar".                                                                       |
| CA-06.3 | **Dado** un usuario que no es supervisor, **cuando** solicita el reporte, **entonces** se responde `403 Forbidden`.                                                         |

---

## Resumen

| RF        | Historias           | Criterios | Prioridad   |
| --------- | ------------------- | --------- | ----------- |
| RF-01     | HU-01, HU-09        | 10        | Obligatorio |
| RF-02     | HU-02, HU-03, HU-13 | 8         | Obligatorio |
| RF-03     | HU-04, HU-11, HU-14 | 8         | Obligatorio |
| RF-04     | HU-05, HU-06, HU-12 | 9         | Obligatorio |
| RF-05     | HU-07, HU-10        | 7         | Obligatorio |
| RF-06     | HU-08               | 3         | Deseable    |
| **Total** | **14**              | **45**    |             |
