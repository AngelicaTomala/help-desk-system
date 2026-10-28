# Matriz de trazabilidad

| Campo   | Valor                  |
| ------- | ---------------------- |
| Versión | 1.0                    |
| Fecha   | 28/10/2026             |
| Estado  | Borrador para revisión |

Une cada criterio de aceptación de [01-requisitos-funcionales.md](functional/01-requisitos-funcionales.md) con su historia, las reglas de [02-reglas-negocio.md](functional/02-reglas-negocio.md) y los endpoints de [03-contrato-api.md](technical/03-contrato-api.md).

- Las columnas **Test** y **Commit** se llenan durante la implementación (RNF-10: `[Trait("CA", …)]` y `[Trait("RN", …)]`).
- **Estado inicial:** `Especificado`.
- Las rutas omiten el prefijo `/api/v1`, común a todas (decisión 43).
- "—" en "RN relacionadas" indica que el CA no deriva de una RN; ver [Observaciones](#observaciones).

## RF-01 · Registrar y reclasificar el ticket

| RF | HU | CA | RN relacionadas | Endpoint | Test | Commit | Estado |
| -- | -- | -- | --------------- | -------- | ---- | ------ | ------ |
| RF-01 | HU-01 | CA-01.1 | RN-01, RN-02, RN-03 | `POST /tickets` | | | Especificado |
| RF-01 | HU-01 | CA-01.2 | RN-10 | `POST /tickets` | | | Especificado |
| RF-01 | HU-01 | CA-01.3 | RN-01 | `POST /tickets` | | | Especificado |
| RF-01 | HU-01 | CA-01.4 | RN-02 | `POST /tickets` | | | Especificado |
| RF-01 | HU-01 | CA-01.5 | — | `POST /tickets` | | | Especificado |
| RF-01 | HU-09 | CA-01.6 | RN-02, RN-13 | `POST /tickets/{id}/reclasificar` | | | Especificado |
| RF-01 | HU-09 | CA-01.7 | RN-13 | `POST /tickets/{id}/reclasificar` | | | Especificado |
| RF-01 | HU-09 | CA-01.8 | RN-13 | `POST /tickets/{id}/reclasificar` | | | Especificado |
| RF-01 | HU-09 | CA-01.9 | RN-02, RN-13 | `POST /tickets/{id}/reclasificar` | | | Especificado |
| RF-01 | HU-01 | CA-01.10 | RN-03 | `POST /tickets` | | | Especificado |

## RF-02 · Consultar, filtrar y paginar tickets

| RF | HU | CA | RN relacionadas | Endpoint | Test | Commit | Estado |
| -- | -- | -- | --------------- | -------- | ---- | ------ | ------ |
| RF-02 | HU-02, HU-03, HU-13 | CA-02.1 | RN-04, RN-11 | `GET /tickets/{id}` | | | Especificado |
| RF-02 | HU-02, HU-03, HU-13 | CA-02.2 | — | `GET /tickets/{id}` | | | Especificado |
| RF-02 | HU-02 | CA-02.3 | RN-04, RN-11 | `GET /tickets` | | | Especificado |
| RF-02 | HU-02, HU-03, HU-13 | CA-02.4 | RN-05 | `GET /tickets` | | | Especificado |
| RF-02 | HU-02, HU-03, HU-13 | CA-02.5 | RN-04 | `GET /tickets` | | | Especificado |
| RF-02 | HU-03, HU-13 | CA-02.6 | RN-04 | `GET /tickets/{id}` | | | Especificado |
| RF-02 | HU-13 | CA-02.7 | RN-04 | `GET /tickets` | | | Especificado |
| RF-02 | HU-02 | CA-02.8 | — | `GET /tickets` | | | Especificado |

## RF-03 · Asignar y reasignar técnico

| RF | HU | CA | RN relacionadas | Endpoint | Test | Commit | Estado |
| -- | -- | -- | --------------- | -------- | ---- | ------ | ------ |
| RF-03 | HU-04 | CA-03.1 | RN-06, RN-07, RN-08 | `POST /tickets/{id}/asignar` | | | Especificado |
| RF-03 | HU-11 | CA-03.2 | RN-06, RN-07, RN-08 | `POST /tickets/{id}/reasignar` | | | Especificado |
| RF-03 | HU-04, HU-11 | CA-03.3 | RN-06 | `POST /tickets/{id}/asignar`, `POST /tickets/{id}/reasignar` | | | Especificado |
| RF-03 | HU-04, HU-11 | CA-03.4 | RN-07, RN-08 | `POST /tickets/{id}/asignar`, `POST /tickets/{id}/reasignar` | | | Especificado |
| RF-03 | HU-04, HU-11 | CA-03.5 | RN-06 | `POST /tickets/{id}/asignar`, `POST /tickets/{id}/reasignar` | | | Especificado |
| RF-03 | HU-14 | CA-03.6 | RN-06 | `GET /tecnicos` | | | Especificado |
| RF-03 | HU-14 | CA-03.7 | — | `GET /tecnicos` | | | Especificado |
| RF-03 | HU-11 | CA-03.8 | RN-06 | `POST /tickets/{id}/reasignar` | | | Especificado |

## RF-04 · Cambiar estado del ticket

| RF | HU | CA | RN relacionadas | Endpoint | Test | Commit | Estado |
| -- | -- | -- | --------------- | -------- | ---- | ------ | ------ |
| RF-04 | HU-05 | CA-04.1 | RN-08 | `POST /tickets/{id}/iniciar-atencion` | | | Especificado |
| RF-04 | HU-05 | CA-04.2 | RN-08, RN-09 | `POST /tickets/{id}/resolver` | | | Especificado |
| RF-04 | HU-05 | CA-04.3 | RN-09 | `POST /tickets/{id}/resolver` | | | Especificado |
| RF-04 | HU-06, HU-12 | CA-04.4 | RN-08 | `POST /tickets/{id}/cerrar` | | | Especificado |
| RF-04 | HU-06 | CA-04.5 | RN-08, RN-09 | `POST /tickets/{id}/reabrir` | | | Especificado |
| RF-04 | HU-05, HU-06, HU-12 | CA-04.6 | RN-08 | `POST /tickets/{id}/iniciar-atencion`, `POST /tickets/{id}/resolver`, `POST /tickets/{id}/cerrar`, `POST /tickets/{id}/reabrir` | | | Especificado |
| RF-04 | HU-05, HU-06, HU-12 | CA-04.7 | RN-08 | `POST /tickets/{id}/iniciar-atencion`, `POST /tickets/{id}/resolver`, `POST /tickets/{id}/cerrar`, `POST /tickets/{id}/reabrir` | | | Especificado |
| RF-04 | HU-06 | CA-04.8 | RN-09 | `POST /tickets/{id}/reabrir` | | | Especificado |
| RF-04 | HU-05, HU-06 | CA-04.9 | RN-09 | `POST /tickets/{id}/resolver`, `POST /tickets/{id}/reabrir` | | | Especificado |

## RF-05 · Calcular y controlar el SLA

| RF | HU | CA | RN relacionadas | Endpoint | Test | Commit | Estado |
| -- | -- | -- | --------------- | -------- | ---- | ------ | ------ |
| RF-05 | HU-07 | CA-05.1 | RN-10 | `POST /tickets`, `POST /tickets/{id}/cambiar-prioridad` | | | Especificado |
| RF-05 | HU-07 | CA-05.2 | RN-11 | `GET /tickets/{id}`, `GET /tickets` | | | Especificado |
| RF-05 | HU-07 | CA-05.3 | RN-11 | `GET /tickets/{id}`, `GET /tickets`, `POST /tickets/{id}/resolver`, `POST /tickets/{id}/reabrir` | | | Especificado |
| RF-05 | HU-10 | CA-05.4 | RN-10, RN-12 | `POST /tickets/{id}/cambiar-prioridad` | | | Especificado |
| RF-05 | HU-10 | CA-05.5 | RN-12 | `POST /tickets/{id}/cambiar-prioridad` | | | Especificado |
| RF-05 | HU-10 | CA-05.6 | RN-12 | `POST /tickets/{id}/cambiar-prioridad` | | | Especificado |
| RF-05 | HU-10 | CA-05.7 | RN-02, RN-12 | `POST /tickets/{id}/cambiar-prioridad` | | | Especificado |

## RF-06 · Reporte de tickets vencidos

| RF | HU | CA | RN relacionadas | Endpoint | Test | Commit | Estado |
| -- | -- | -- | --------------- | -------- | ---- | ------ | ------ |
| RF-06 | HU-08 | CA-06.1 | RN-11 | `GET /reportes/tickets-vencidos` | | | Especificado |
| RF-06 | HU-08 | CA-06.2 | RN-11 | `GET /reportes/tickets-vencidos` | | | Especificado |
| RF-06 | HU-08 | CA-06.3 | — | `GET /reportes/tickets-vencidos` | | | Especificado |

## Observaciones

- **Ningún CA tiene endpoint "N/A".** Los 45 CA se verifican a través de un endpoint del contrato, incluidos los de validación (CA-01.3, CA-01.4, CA-02.8, CA-04.3, CA-04.8, CA-04.9, CA-05.7), que se observan en sus respuestas `400`.
- **CA sin RN relacionada ("—"):**
  - **CA-01.5:** se basa en el supuesto S-02 y en RNF-01 (identidad por `X-Usuario-Id`), no en una RN. El `401` aplica a todos los endpoints; aquí se traza al que nombra el CA.
  - **CA-02.2:** el `404` de un ticket inexistente sale de RNF-04 y de la decisión 65, no de una RN.
  - **CA-02.8:** el `400` por filtro inválido sale de las decisiones 10 y 53 y de RNF-05.
  - **CA-03.7 y CA-06.3:** el `403` sale de la matriz de permisos de [02-reglas-negocio.md](functional/02-reglas-negocio.md#matriz-de-permisos) (decisión 12 para técnicos activos), no de una RN numerada.
- **CA-05.1 a CA-05.3:** no tienen una operación propia. Se verifican por los valores de `fechaVencimiento` y `estadoSla` que devuelven los endpoints indicados ([03-contrato-api.md §5.1](technical/03-contrato-api.md#51-ca--endpoints)).
- **HU de RF-02 y CA-04.4:** cuando un CA cubre varios actores, se listan todas sus HU. Por ejemplo, CA-04.4 (cierre) tiene HU-06 para el solicitante y HU-12 para el supervisor.

## Verificación

- **CA:** los 45 CA de `01-requisitos-funcionales.md` (CA-01.1 a CA-06.3) aparecen una vez cada uno en esta matriz.
- **Endpoints:** las 13 operaciones de `03-contrato-api.md` tienen al menos un CA:

| Endpoint | CA |
| -------- | -- |
| `POST /tickets` | CA-01.1 a CA-01.5, CA-01.10, CA-05.1 |
| `GET /tickets/{id}` | CA-02.1, CA-02.2, CA-02.6, CA-05.2, CA-05.3 |
| `GET /tickets` | CA-02.3, CA-02.4, CA-02.5, CA-02.7, CA-02.8, CA-05.2, CA-05.3 |
| `POST /tickets/{id}/reclasificar` | CA-01.6 a CA-01.9 |
| `POST /tickets/{id}/cambiar-prioridad` | CA-05.1, CA-05.4 a CA-05.7 |
| `GET /reportes/tickets-vencidos` | CA-06.1 a CA-06.3 |
| `POST /tickets/{id}/asignar` | CA-03.1, CA-03.3, CA-03.4, CA-03.5 |
| `POST /tickets/{id}/reasignar` | CA-03.2, CA-03.3, CA-03.4, CA-03.5, CA-03.8 |
| `GET /tecnicos` | CA-03.6, CA-03.7 |
| `POST /tickets/{id}/iniciar-atencion` | CA-04.1, CA-04.6, CA-04.7 |
| `POST /tickets/{id}/resolver` | CA-04.2, CA-04.3, CA-04.6, CA-04.7, CA-04.9, CA-05.3 |
| `POST /tickets/{id}/cerrar` | CA-04.4, CA-04.6, CA-04.7 |
| `POST /tickets/{id}/reabrir` | CA-04.5 a CA-04.9, CA-05.3 |
