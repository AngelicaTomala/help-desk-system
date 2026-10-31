# Casos de uso

| Campo   | Valor                  |
| ------- | ---------------------- |
| Versión | 1.0                    |
| Fecha   | 31/10/2026             |
| Estado  | Borrador para revisión |

Casos de uso (CU) de RF-01. Se derivan solo de las historias y criterios de aceptación de [01-requisitos-funcionales.md](functional/01-requisitos-funcionales.md), de las reglas de [02-reglas-negocio.md](functional/02-reglas-negocio.md) y del [contrato de la API](technical/03-contrato-api.md) (§2.1 y §2.4); no agregan comportamiento. Las "decisiones N" están en la [sección 9 de la auditoría](../docs/auditoria-documentacion.md#9-decisiones-tomadas).

- Los errores siguen el orden `401` → `404` → `403` → `400` → `409` (RNF-04, decisión 6). Los errores de lectura del cuerpo se responden antes que el `404` y el `403` (decisión 93).
- Una operación rechazada no modifica datos (RNF-04).
- El usuario se identifica con el encabezado `X-Usuario-Id` (S-02, RNF-01).

| CU | Nombre | Actor | Endpoint | HU | CA |
| -- | ------ | ----- | -------- | -- | -- |
| CU-01 | Crear ticket | Solicitante | `POST /api/v1/tickets` | HU-01 | CA-01.1 a CA-01.5, CA-01.10 |
| CU-02 | Reclasificar ticket | Técnico asignado o Supervisor | `POST /api/v1/tickets/{id}/reclasificar` | HU-09 | CA-01.6 a CA-01.9 |

---

## CU-01 · Crear ticket

| Campo | Valor |
| ----- | ----- |
| ID | CU-01 |
| Nombre | Crear ticket |
| Actor | Solicitante (RN-03) |
| Endpoint | `POST /api/v1/tickets` (contrato §2.1) |
| HU de origen | HU-01 |
| CA de origen | CA-01.1, CA-01.2, CA-01.3, CA-01.4, CA-01.5, CA-01.10 |
| RN | RN-01, RN-02, RN-03, RN-10 |

**Precondiciones:**

1. El usuario existe entre los datos semilla (S-01) y envía su Id en `X-Usuario-Id`.
2. La petición tiene `Content-Type: application/json` y un cuerpo JSON con `titulo`, `descripcion`, `categoria` y `prioridad`.

**Flujo principal:**

1. El solicitante envía el título, la descripción, la categoría y la prioridad. El solicitante del ticket se toma de `X-Usuario-Id`, nunca del cuerpo (decisión 54).
2. El sistema identifica al usuario por `X-Usuario-Id`.
3. El sistema comprueba que el usuario tiene el rol Solicitante (RN-03).
4. El sistema recorta los espacios al inicio y al final del título y la descripción, y los valida: título de 5 a 150 caracteres y descripción de 10 a 2000 (RN-01); categoría y prioridad entre los valores permitidos (RN-02).
5. El sistema genera un código único y creciente con formato `HD-` + 6 dígitos (RN-03).
6. El sistema crea el ticket en estado `Abierto`, sin técnico, con `fechaCreacion` y `fechaActualizacion` iguales a la hora actual (UTC) y `fechaVencimiento` igual a la creación más las horas de SLA de la prioridad: Crítica 2 h, Alta 4 h, Media 8 h, Baja 24 h (RN-03, RN-10, decisión 25).
7. El sistema responde `201 Created` con el ticket (detalle) y el encabezado `Location` como ruta relativa `/api/v1/tickets/{id}` (CA-01.1, decisión 64).

**Flujos alternativos:**

| ID | Condición | Resultado | Origen |
| -- | --------- | --------- | ------ |
| A1 | Categoría o prioridad con otras mayúsculas (p. ej., `"software"`) | Se aceptan; la respuesta usa la grafía canónica | RN-02, decisión 58 |
| A2 | El cuerpo trae campos no previstos (p. ej., `solicitanteId`) | Se ignoran; el solicitante sigue siendo el de `X-Usuario-Id` | Decisiones 54 y 66 |

**Flujos de error:**

| ID | Paso | Condición | Respuesta | Origen |
| -- | ---- | --------- | --------- | ------ |
| E1 | 2 | Falta `X-Usuario-Id`, no es un entero positivo de `int` con el formato válido o aparece más de una vez | `401` `USUARIO_NO_IDENTIFICADO`; no se crea el ticket | RNF-01, decisiones 30 y 95 |
| E2 | 2 | `X-Usuario-Id` válido que no corresponde a ningún usuario | `401` `USUARIO_INEXISTENTE`; no se crea el ticket | CA-01.5, RNF-01 |
| E3 | 1 | Cuerpo ausente, vacío, `null`, mal formado o sin `Content-Type`; o un campo con un tipo JSON incorrecto | `400` `VALIDACION_FALLIDA` (clave `cuerpo` o la del campo); no se crea el ticket | Decisión 93 |
| E4 | 1 | `Content-Type` presente que no es JSON | `415` sin `codigo` | Decisión 94 |
| E5 | 3 | El usuario es técnico o supervisor, aunque los datos sean inválidos | `403` `ROL_NO_AUTORIZADO`; no se crea el ticket | CA-01.10, RN-03, decisión 6 |
| E6 | 4 | Título o descripción fuera de rango, vacíos tras recortar o ausentes | `400` `VALIDACION_FALLIDA` con `errors` por campo; no se crea el ticket | CA-01.3, RN-01, decisión 31 |
| E7 | 4 | Categoría o prioridad inexistente, numérica o ausente | `400` `VALIDACION_FALLIDA` con `errors` por campo; no se crea el ticket | CA-01.4, RN-02, decisión 58 |
| E8 | 5 | El código superó `HD-999999` (SEQUENCE agotada) | `500` `ERROR_INTERNO`; no se crea el ticket | RN-03, decisión 90 |

**Postcondiciones:**

- **Éxito:** el ticket queda guardado en estado `Abierto`, sin técnico, con un código único y su vencimiento según RN-10. El estado del SLA se calcula al responder y no se guarda (RN-11).
- **Error:** no se crea ningún ticket (RNF-04).

---

## CU-02 · Reclasificar ticket

| Campo | Valor |
| ----- | ----- |
| ID | CU-02 |
| Nombre | Reclasificar ticket |
| Actor | Técnico asignado al ticket o Supervisor (RN-13) |
| Endpoint | `POST /api/v1/tickets/{id}/reclasificar` (contrato §2.4) |
| HU de origen | HU-09 |
| CA de origen | CA-01.6, CA-01.7, CA-01.8, CA-01.9 |
| RN | RN-02, RN-13 |

**Precondiciones:**

1. El usuario existe entre los datos semilla (S-01) y envía su Id en `X-Usuario-Id`.
2. El ticket `{id}` existe y no está `Resuelto` ni `Cerrado` (RN-13).
3. La petición tiene `Content-Type: application/json` y un cuerpo JSON con `categoria`.

**Flujo principal:**

1. El actor envía la nueva categoría del ticket `{id}`.
2. El sistema identifica al usuario por `X-Usuario-Id`.
3. El sistema busca el ticket `{id}`.
4. El sistema comprueba que el usuario es un supervisor o el técnico asignado al ticket (RN-13).
5. El sistema valida que la categoría sea uno de los valores permitidos (RN-02).
6. El sistema comprueba que el ticket no está `Resuelto` ni `Cerrado`, cambia la categoría y fija `fechaActualizacion` a la hora actual (RN-13, decisión 74). El estado del ticket no cambia.
7. El sistema responde `200 OK` con el ticket actualizado (detalle) (CA-01.6, decisión 23).

**Flujos alternativos:**

| ID | Condición | Resultado | Origen |
| -- | --------- | --------- | ------ |
| A1 | La categoría es igual a la actual y el ticket es modificable | `200 OK` sin cambios; `fechaActualizacion` no cambia | Decisión 67 |
| A2 | El ticket está `Abierto` (aún sin técnico asignado) | Solo el supervisor puede reclasificarlo; un técnico sigue el flujo E6 | Hallazgo 11 de [01-modelo-dominio.md §7](technical/01-modelo-dominio.md#7-contradicciones-y-vacíos-detectados) |
| A3 | Categoría con otras mayúsculas (p. ej., `"otros"`) | Se acepta; la respuesta usa la grafía canónica | RN-02, decisión 58 |

**Flujos de error:**

| ID | Paso | Condición | Respuesta | Origen |
| -- | ---- | --------- | --------- | ------ |
| E1 | 2 | Falta `X-Usuario-Id`, no es válido o aparece más de una vez | `401` `USUARIO_NO_IDENTIFICADO`; la categoría no cambia | RNF-01, decisiones 30 y 95 |
| E2 | 2 | `X-Usuario-Id` válido que no corresponde a ningún usuario | `401` `USUARIO_INEXISTENTE`; la categoría no cambia | RNF-01 |
| E3 | 1 | Cuerpo ilegible, `Content-Type` ausente o campo con tipo JSON incorrecto | `400` `VALIDACION_FALLIDA`, antes que el `404` y el `403` | Decisión 93 |
| E4 | 3 | `{id}` no es un entero positivo con el formato válido (p. ej., `012`) o el ticket no existe | `404` `TICKET_NO_ENCONTRADO`, aunque la categoría también sea inválida | Decisiones 6, 65 y 95 |
| E5 | 4 | El usuario es un solicitante | `403` `ROL_NO_AUTORIZADO`; la categoría no cambia | CA-01.8, RN-13 |
| E6 | 4 | El usuario es un técnico que no es el asignado, aunque el ticket esté `Cerrado` | `403` `TICKET_AJENO`; la categoría no cambia | CA-01.8, RN-13, decisión 6 |
| E7 | 5 | Categoría inexistente, numérica o ausente, aunque el ticket esté `Cerrado` | `400` `VALIDACION_FALLIDA`; la categoría no cambia | CA-01.9, RN-02, decisiones 6 y 58 |
| E8 | 6 | El ticket está `Resuelto` o `Cerrado`, aunque la categoría sea igual a la actual | `409` `TICKET_NO_MODIFICABLE`; la categoría no cambia | CA-01.7, RN-13, decisiones 40 y 70 |

**Postcondiciones:**

- **Éxito:** el ticket tiene la nueva categoría y `fechaActualizacion` igual a la hora de la operación; su estado, técnico, prioridad y vencimiento no cambian.
- **Error:** el ticket no cambia (RNF-04).
