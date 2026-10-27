# 01 · Requisitos funcionales, historias y criterios de aceptación

Convención de IDs:

- **RF-xx**: requisito funcional
- **HU-xx**: historia de usuario
- **CA-xx.y**: criterio de aceptación _y_ del RF _xx_ (cada uno debe tener al menos una prueba)
- **RN-xx**: regla de negocio (ver `02-reglas-negocio.md`)

---

## RF-01 · Registrar ticket

**Prioridad:** Obligatorio ·

**Reglas:** RN-01, RN-02, RN-03, RN-10

**HU-01:** Como **solicitante**, quiero registrar un ticket describiendo mi problema, para que el área de TI lo atienda dentro del tiempo comprometido.

| ID      | Criterio de aceptación                                                                                                                                                                                   |
| ------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-01.1 | **Dado** un solicitante válido, **cuando** envía título, descripción, categoría y prioridad válidos, **entonces** se crea el ticket con estado `Abierto`, un código único (`HD-000001`) y respuesta `201 |
|         | Created`.                                                                                                                                                                                                |
| CA-01.2 | **Dado** un ticket recién creado, **entonces** su fecha de vencimiento es la fecha de creación más las horas de SLA de su prioridad (RN-10).                                                             |
| CA-01.3 | **Dado** un título con menos de 5 o más de 150 caracteres, **cuando** se envía, **entonces** se responde `400 Bad Request` con el detalle del error y no se crea el ticket.                              |
| CA-01.4 | **Dado** una categoría o prioridad inexistente, **cuando** se envía, **entonces** se responde `400 Bad Request`.                                                                                         |
| CA-01.5 | **Dado** un usuario que no existe en el encabezado `X-Usuario-Id`, **cuando** intenta crear un ticket, **entonces** se responde `401 Unauthorized`.                                                      |

---

## RF-02 · Consultar, filtrar y paginar tickets

**Prioridad:** Obligatorio ·

**Reglas:** RN-04, RN-05

**HU-02:** Como **supervisor**, quiero consultar los tickets filtrando por estado, prioridad, técnico o vencimiento, para tener visibilidad de la carga de trabajo.

**HU-03:** Como **solicitante**, quiero ver solo mis tickets y su estado, para saber cómo avanza mi solicitud.

| ID      | Criterio de aceptación                                                                                                                                                            |
| ------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-02.1 | **Dado** un ticket existente, **cuando** se consulta por su Id, **entonces** se devuelve con estado, prioridad, técnico asignado, fecha de vencimiento y estado del SLA.          |
| CA-02.2 | **Dado** un Id inexistente, **cuando** se consulta, **entonces** se responde `404 Not Found`.                                                                                     |
| CA-02.3 | **Dado** un listado, **cuando** se envían filtros (`estado`, `prioridad`, `tecnicoId`, `soloVencidos`), **entonces** solo se devuelven los tickets que cumplen todos los filtros. |
| CA-02.4 | **Dado** un listado, **entonces** se pagina con `page` y `pageSize` (máximo 50), ordenado por fecha de creación descendente, e incluye el total de registros.                     |
| CA-02.5 | **Dado** un solicitante, **cuando** lista tickets, **entonces** solo ve los suyos; un técnico solo ve los asignados a él; un supervisor ve todos (RN-04).                         |
| CA-02.6 | **Dado** un solicitante, **cuando** consulta por Id un ticket que no es suyo, **entonces** se responde `403 Forbidden`.                                                           |

---

## RF-03 · Asignar y reasignar técnico

**Prioridad:** Obligatorio ·

**Reglas:** RN-06, RN-07

**HU-04:** Como **supervisor**, quiero asignar un ticket a un técnico, para que haya un responsable claro de su atención.

| ID      | Criterio de aceptación                                                                                                                                              |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-03.1 | **Dado** un ticket `Abierto` y un técnico activo, **cuando** el supervisor lo asigna, **entonces** el ticket queda con ese técnico y pasa a estado `Asignado`.      |
| CA-03.2 | **Dado** un ticket `Asignado` o `En progreso`, **cuando** el supervisor lo reasigna a otro técnico, **entonces** cambia el técnico y el ticket queda en `Asignado`. |
| CA-03.3 | **Dado** un ticket `Resuelto` o `Cerrado`, **cuando** se intenta asignar, **entonces** se responde `409 Conflict`.                                                  |
| CA-03.4 | **Dado** un usuario que no es supervisor, **cuando** intenta asignar, **entonces** se responde `403 Forbidden`.                                                     |

---

## RF-04 · Cambiar estado del ticket

**Prioridad:** Obligatorio ·

**Reglas:** RN-08, RN-09

**HU-05:** Como **técnico**, quiero registrar el avance de un ticket (iniciar atención y resolverlo), para que el solicitante y el supervisor conozcan su estado.

**HU-06:** Como **solicitante**, quiero confirmar el cierre de mi ticket resuelto o reabrirlo si el problema persiste, para asegurar que mi problema quede realmente solucionado.

| ID      | Criterio de aceptación                                                                                                                                                                                  |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-04.1 | **Dado** un ticket `Asignado`, **cuando** su técnico asignado inicia la atención, **entonces** pasa a `En progreso`.                                                                                    |
| CA-04.2 | **Dado** un ticket `En progreso`, **cuando** su técnico lo marca como resuelto con un comentario de solución, **entonces** pasa a `Resuelto` y se registra la fecha de resolución.                      |
| CA-04.3 | **Dado** un intento de resolver sin comentario de solución, **entonces** se responde `400 Bad Request`.                                                                                                 |
| CA-04.4 | **Dado** un ticket `Resuelto`, **cuando** el solicitante (o el supervisor) confirma, **entonces** pasa a `Cerrado` y se registra la fecha de cierre.                                                    |
| CA-04.5 | **Dado** un ticket `Resuelto`, **cuando** el solicitante lo reabre con un motivo, **entonces** vuelve a `En progreso` con el mismo técnico y se borra la fecha de resolución.                           |
| CA-04.6 | **Dado** cualquier transición no permitida por la tabla de RN-08 (por ejemplo, `Abierto → Resuelto` o cualquier cambio desde `Cerrado`), **entonces** se responde `409 Conflict` y el estado no cambia. |
| CA-04.7 | **Dado** un técnico que no es el asignado al ticket, **cuando** intenta cambiar su estado, **entonces** se responde `403 Forbidden`.                                                                    |

---

## RF-05 · Calcular y controlar el SLA

**Prioridad:** Obligatorio ·

**Reglas:** RN-10, RN-11, RN-12

**HU-07:** Como **supervisor**, quiero que cada ticket tenga una fecha de vencimiento según su prioridad y saber si se cumple, para medir la calidad del servicio.

| ID      | Criterio de aceptación                                                                                                                                                                            |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-05.1 | **Dado** un ticket con prioridad Crítica / Alta / Media / Baja, **entonces** su vencimiento es creación + 2 / 4 / 8 / 24 horas respectivamente.                                                   |
| CA-05.2 | **Dado** un ticket no resuelto cuya fecha actual supera el vencimiento, **cuando** se consulta, **entonces** su estado de SLA es `Vencido`; si no la supera, es `EnTiempo`.                       |
| CA-05.3 | **Dado** un ticket resuelto, **entonces** su estado de SLA es `Cumplido` si se resolvió antes o en el vencimiento, e `Incumplido` si se resolvió después.                                         |
| CA-05.4 | **Dado** un ticket no resuelto ni cerrado, **cuando** el supervisor cambia su prioridad, **entonces** el vencimiento se recalcula desde la fecha de creación con las horas de la nueva prioridad. |
| CA-05.5 | **Dado** un ticket resuelto o cerrado, **cuando** se intenta cambiar la prioridad, **entonces** se responde `409 Conflict`.                                                                       |

---

## RF-06 · Reporte de tickets vencidos _(deseable)_

**Prioridad:** Deseable ·

**Reglas:** RN-11

**HU-08:** Como **supervisor**, quiero un reporte de tickets vencidos agrupado por técnico y prioridad, para tomar acciones correctivas.

| ID      | Criterio de aceptación                                                                                                                                                      |
| ------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA-06.1 | **Dado** tickets con SLA `Vencido`, **cuando** el supervisor solicita el reporte, **entonces** recibe la cantidad agrupada por técnico y por prioridad, y el total general. |
| CA-06.2 | **Dado** tickets sin técnico asignado y vencidos, **entonces** aparecen agrupados como "Sin asignar".                                                                       |
| CA-06.3 | **Dado** un usuario que no es supervisor, **cuando** solicita el reporte, **entonces** se responde `403 Forbidden`.                                                         |

---

## Resumen

| RF        | Historias    | Criterios | Prioridad   |
| --------- | ------------ | --------- | ----------- |
| RF-01     | HU-01        | 5         | Obligatorio |
| RF-02     | HU-02, HU-03 | 6         | Obligatorio |
| RF-03     | HU-04        | 4         | Obligatorio |
| RF-04     | HU-05, HU-06 | 7         | Obligatorio |
| RF-05     | HU-07        | 5         | Obligatorio |
| RF-06     | HU-08        | 3         | Deseable    |
| **Total** | **8**        | **30**    |             |
