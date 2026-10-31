# 03 · Contrato de la API

| Campo   | Valor                  |
| ------- | ---------------------- |
| Versión | 0.2                    |
| Fecha   | 30/10/2026             |
| Estado  | Borrador en redacción  |

Contrato REST de las 13 operaciones del MVP. Resuelve el hallazgo H-03 de la [auditoría](../../docs/auditoria-documentacion.md). Fuentes: decisiones 1 a 74, 93 a 95, 114, 116 y 117 de la [auditoría §9](../../docs/auditoria-documentacion.md#9-decisiones-tomadas), [requisitos funcionales](../functional/01-requisitos-funcionales.md), [reglas de negocio](../functional/02-reglas-negocio.md), [RNF](../functional/03-requisitos-no-funcionales.md), [modelo de dominio](01-modelo-dominio.md) y [ADR-004](adr/ADR-004-problemdetails-errores.md).

## Índice

1. [Convenciones comunes](#1-convenciones-comunes)
2. [Endpoints – Grupo A](#2-endpoints--grupo-a)
3. [Endpoints – Grupo B](#3-endpoints--grupo-b)
4. [Ejemplos de ProblemDetails](#4-ejemplos-de-problemdetails)
5. [Verificación](#5-verificación)

---

## 1. Convenciones comunes

### 1.1 Base y estilo de rutas

- Todas las rutas empiezan por `/api/v1` (decisión 43).
- Recursos en plural y una operación por acción, no una operación genérica de cambio de estado (decisiones 8 y 42).
- Las rutas de varias palabras usan kebab-case, sin tildes, por ejemplo `iniciar-atencion` (decisión 44).
- Un `{id}` de ruta que no sea un entero positivo responde `404` `TICKET_NO_ENCONTRADO` con Problem Details completo (decisión 65). Solo es entero positivo con el formato de §1.2 (decisión 95): por ejemplo, `012` y `+12` responden `404`.

> **Nota de implementación.** Una restricción de ruta como `{id:int:min(1)}` produce un `404` sin Problem Details, por lo que no cumple la decisión 65. Además, la validación de `X-Usuario-Id` debe ejecutarse en un middleware, antes de resolver el recurso, para respetar el orden `401` → `404` de §1.4.

### 1.2 Encabezado `X-Usuario-Id`

- Identifica al usuario en cada petición; no hay JWT en el MVP (S-02, RNF-01).
- **Formato:** entero positivo dentro del rango de `int` (RNF-01, decisión 30), formado solo por dígitos ASCII, sin signo, sin espacios y sin ceros a la izquierda (decisión 95). Por ejemplo, `+1`, `01` y `1,2` no son válidos.
- HTTP quita los espacios al inicio y al final del valor del encabezado (RFC 9110) antes de que la Api lo lea, y el formato se comprueba sobre el valor ya sin ellos: ` 1` equivale a `1` (decisión 117).
- **Si falta, no es válido** (no numérico, `0`, negativo, fuera de rango o con otro formato que el anterior) **o el encabezado aparece más de una vez** (decisión 95): `401` con `codigo` `USUARIO_NO_IDENTIFICADO`. **Si es válido pero no corresponde a ningún usuario:** `401` con `USUARIO_INEXISTENTE` ([ADR-004](adr/ADR-004-problemdetails-errores.md#enmienda-3--28102026)).
- La Api obtiene el usuario a través de Application (decisión 28, ADR-001 Enmienda 2).
- **Exentos:** Swagger UI y el documento OpenAPI no exigen el encabezado (RNF-01, decisión 52). Tampoco lo exigen las peticiones que no llegan a una operación: una ruta inexistente responde `404` y un método no admitido `405` aunque falte el encabezado (decisión 116).
- El solicitante de un ticket nuevo se toma de este encabezado; nunca viene en el cuerpo (decisión 54).

### 1.3 Formato de error (Problem Details)

- Todas las respuestas de error usan Problem Details (RFC 9457) con `Content-Type: application/problem+json` (RNF-03, [ADR-004](adr/ADR-004-problemdetails-errores.md)).
- `type` y `title` toman los valores por defecto de ASP.NET Core. Se agrega el campo de extensión `codigo` (decisión 29).
- `codigo` solo toma valores del [catálogo de ADR-004](adr/ADR-004-problemdetails-errores.md#enmienda-3--28102026), aprobado en la decisión 39. Las respuestas del framework que no son casos de negocio también usan Problem Details (RNF-03), con `traceId` y sin `codigo`: ruta inexistente (`404`), método no admitido (`405`) y tipo de contenido no admitido (`415`). El `415` se responde cuando el `Content-Type` está presente y no es JSON; si falta, aplica el punto del cuerpo de esta sección (decisión 94).
- En los `409`, `codigo` es el `Codigo` de `TransicionInvalidaException` (`TRANSICION_INVALIDA` o `TICKET_NO_MODIFICABLE`), que la Api copia (decisión 40).
- En los `400` de validación, el detalle por campo va en `errors` (ValidationProblemDetails, RNF-05, ADR-003). Las claves de `errors` son los nombres de campo en camelCase, salvo la clave `cuerpo` del punto siguiente. Los mensajes van en español y su redacción queda a la implementación; el contrato fija solo las claves y el `codigo` (decisión 61).
- Un cuerpo ausente, vacío, `null`, que no es un objeto JSON o con JSON mal formado responde `400` `VALIDACION_FALLIDA` con la clave `cuerpo` en `errors`. Una petición sin `Content-Type` se trata como un cuerpo ausente o ilegible. Un campo con un tipo JSON incorrecto (por ejemplo, `"titulo": 123` o `"categoria": 3`) responde `400` `VALIDACION_FALLIDA` con la clave de ese campo, y solo se informa ese error, porque la lectura del JSON se detiene en el primer error de tipo (decisión 93).
- Todos los Problem Details incluyen `traceId`. `detail` e `instance` no forman parte del contrato, y `detail` nunca expone información interna (decisión 62).
- En los `500`, `codigo` es siempre `ERROR_INTERNO`, sin detalles internos; el detalle de la excepción se registra en el log (decisión 41, RNF-03).
- El `404` aplica solo al recurso de la ruta. Un dato inválido del cuerpo, como un técnico inexistente, es `400` (decisión 4, RNF-04).

| `codigo` | HTTP |
| -------- | ---- |
| `VALIDACION_FALLIDA`, `TECNICO_NO_VALIDO`, `MISMO_TECNICO` | `400` |
| `USUARIO_NO_IDENTIFICADO`, `USUARIO_INEXISTENTE` | `401` |
| `ROL_NO_AUTORIZADO`, `TICKET_AJENO`, `FILTRO_NO_AUTORIZADO` | `403` |
| `TICKET_NO_ENCONTRADO` | `404` |
| `TRANSICION_INVALIDA`, `TICKET_NO_MODIFICABLE` | `409` |
| `ERROR_INTERNO` | `500` |

Sin `codigo`, con `traceId`: `404` de ruta inexistente, `405` y `415` (decisión 94).

El caso de cada código está en el catálogo de ADR-004.

### 1.4 Orden de evaluación de errores

Cuando aplican varios errores se responde el primero de esta secuencia: `401` → `404` → `403` → `400` → `409` (RNF-04, decisión 6). Una operación rechazada no modifica datos (RNF-04).

**Excepción:** todo error de lectura del cuerpo (§1.3, decisión 93), incluido un campo con tipo JSON incorrecto, ocurre antes del handler, así que su `400` se responde antes que el `404` y el `403`. El `401` sigue siendo el primero.

### 1.5 Respuesta paginada

Los listados paginados responden con este envoltorio (decisión 9, CA-02.4):

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 0
}
```

- `page`: por defecto 1; mínimo 1 (RN-05, RNF-05).
- `pageSize`: por defecto 20; entre 1 y 100 (RN-05, RNF-05, decisión 9).
- Fuera de rango: `400` `VALIDACION_FALLIDA` (CA-02.4, RNF-05).
- **Orden del listado de tickets:** `fechaCreacion` descendente y, en caso de empate, `id` descendente (RN-05, CA-02.4, decisión 59).

### 1.6 Fechas y textos

- Todas las fechas están en UTC (S-04, RNF-08) y se serializan en ISO 8601 con sufijo `Z`, por ejemplo `2026-10-28T14:30:00Z`. Los segundos fraccionarios son opcionales (decisión 60).
- `fechaActualizacion` es igual a `fechaCreacion` al crear (decisión 25). Después cambia en toda operación que modifica el ticket, incluidas las que agregan comentarios, salvo la operación sin cambios de la decisión 67 (decisión 74).

> **Nota de implementación.** EF Core lee las columnas `datetime2` como `DateTimeKind.Unspecified`, y así se serializarían sin `Z`. Para cumplir la decisión 60 hará falta un `ValueConverter` que marque las fechas como UTC o usar `DateTimeOffset`.
- Los textos se recortan (espacios al inicio y al final) antes de validar; si quedan vacíos responden `400` `VALIDACION_FALLIDA` (RN-01, RN-09, decisión 31).

### 1.7 Nombres de campos

Los campos JSON, del cuerpo y de la respuesta, van en camelCase (decisión 47). Los campos no previstos en el cuerpo se ignoran; por ejemplo, un `solicitanteId` enviado al crear un ticket (decisiones 54 y 66).

### 1.8 Enums

- Se serializan como texto, con el nombre del valor en el modelo de dominio (decisión 48, RNF-09).
- En la entrada (cuerpo y query) no distinguen mayúsculas y minúsculas; las respuestas usan siempre la grafía canónica (decisión 58).
- Los valores numéricos se rechazan con `400` `VALIDACION_FALLIDA` (decisión 58).
- Los valores de enum no se recortan: un valor con espacios al inicio o al final (por ejemplo, `" Red "`) responde `400` `VALIDACION_FALLIDA`. El recorte de §1.6 se aplica solo a los textos libres (decisión 114).

| Enum             | Valores                                                  | Fuente |
| ---------------- | -------------------------------------------------------- | ------ |
| `EstadoTicket`   | `Abierto`, `Asignado`, `EnProgreso`, `Resuelto`, `Cerrado` | RN-08 |
| `Prioridad`      | `Critica`, `Alta`, `Media`, `Baja`                       | RN-02  |
| `Categoria`      | `Hardware`, `Software`, `Red`, `Accesos`, `Otros`        | RN-02  |
| `EstadoSla`      | `EnTiempo`, `Vencido`, `Cumplido`, `Incumplido`          | RN-11  |
| `TipoComentario` | `Solucion`, `Reapertura`                                 | RN-09  |

Los valores vienen de [01-modelo-dominio.md §2](01-modelo-dominio.md#2-enumeraciones). `Rol` no aparece en ninguna respuesta: las personas se devuelven como `{ id, nombre }` (decisión 1).

### 1.9 Valores `null`

- `tecnico` es `null` cuando el ticket no tiene técnico asignado (decisión 49).
- Las fechas que aún no aplican (`fechaResolucion`, `fechaCierre`) son `null` (decisión 49).
- En el reporte de vencidos, el grupo "Sin asignar" se representa con `tecnico: null` (decisión 50).

### 1.10 Ids de los ejemplos

Los Id de los ejemplos son ilustrativos: los reales los fija la migración ([01-modelo-dominio.md §8](01-modelo-dominio.md#8-datos-semilla)). Los usuarios usan como `id` su número de orden en la tabla de semilla, de forma consistente en todo el documento. Por ejemplo, `1` = Ana Solicitante, `6` = Fabián Técnico, `9` = Irene Técnico (inactiva) y `10` = Jorge Supervisor.

**Elenco de tickets ilustrativos** (decisión 63). Cada ejemplo parte del estado de su ticket en esta tabla, no del resultado de otro ejemplo: los ejemplos son independientes entre sí. El instante de referencia de los ejemplos es `2026-10-28T14:30:00Z`.

| id | codigo | titulo | estado | solicitante | técnico | prioridad | categoria | fechaCreacion | fechaActualizacion | fechaVencimiento | fechaResolucion | fechaCierre | estadoSla |
| -- | ------ | ------ | ------ | ----------- | ------- | --------- | --------- | ------------- | ------------------ | ---------------- | --------------- | ----------- | --------- |
| 11 | `HD-000011` | No abre el sistema de nómina | `Abierto` | 1 · Ana | — (`null`) | `Critica` | `Software` | `2026-10-28T08:00:00Z` | `2026-10-28T08:00:00Z` | `2026-10-28T10:00:00Z` | `null` | `null` | `Vencido` |
| 12 | `HD-000012` | Monitor sin imagen | `Asignado` | 2 · Bruno | 6 · Fabián | `Media` | `Hardware` | `2026-10-28T09:00:00Z` | `2026-10-28T09:30:00Z` | `2026-10-28T17:00:00Z` | `null` | `null` | `EnTiempo` |
| 13 | `HD-000013` | Sin acceso a la red Wi-Fi | `EnProgreso` | 3 · Carla | 7 · Gabriela | `Alta` | `Red` | `2026-10-28T08:00:00Z` | `2026-10-28T10:00:00Z` | `2026-10-28T12:00:00Z` | `null` | `null` | `Vencido` |
| 14 | `HD-000014` | Restablecer contraseña de correo | `Resuelto` | 1 · Ana | 6 · Fabián | `Media` | `Accesos` | `2026-10-27T10:00:00Z` | `2026-10-27T16:00:00Z` | `2026-10-27T18:00:00Z` | `2026-10-27T16:00:00Z` | `null` | `Cumplido` |
| 15 | `HD-000015` | Error al imprimir facturas | `Cerrado` | 4 · Diego | 6 · Fabián | `Baja` | `Software` | `2026-10-26T09:00:00Z` | `2026-10-27T15:00:00Z` | `2026-10-27T09:00:00Z` | `2026-10-27T11:00:00Z` | `2026-10-27T15:00:00Z` | `Incumplido` |
| 16 | `HD-000016` | No puedo instalar el antivirus | `Abierto` | 1 · Ana | — (`null`) | `Alta` | `Software` | `2026-10-28T14:30:00Z` | `2026-10-28T14:30:00Z` | `2026-10-28T18:30:00Z` | `null` | `null` | `EnTiempo` |
- **Ticket 16:** no existe antes del ejemplo de crear ticket; es el que ese ejemplo crea.
- **Vencimientos:** cumplen RN-10 (Crítica 2 h, Alta 4 h, Media 8 h, Baja 24 h).
- **Estado del SLA:** cumple RN-11 respecto del instante de referencia.

---

## 2. Endpoints – Grupo A

Registro, consulta y datos del ticket. Los ejemplos usan el elenco de §1.10 y el instante de referencia `2026-10-28T14:30:00Z`.

**Comunes a todas las operaciones del grupo:**

- **Encabezados de la petición:** `X-Usuario-Id` (obligatorio, §1.2) y `Content-Type: application/json` cuando hay cuerpo.
- **Respuestas exitosas:** `Content-Type: application/json`.
- **Errores:** `application/problem+json` (§1.3). Además de los de cada tabla, cualquier operación puede responder `500` con `ERROR_INTERNO` (decisión 41).
- **Tablas de errores:** siguen el orden de evaluación de §1.4.
- **Detalle:** las operaciones que devuelven el ticket usan el objeto detalle de §2.2.

### 2.1 Crear ticket

`POST /api/v1/tickets`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Solicitante (RN-03, decisión 3, matriz de permisos) |
| Origen | RF-01, RF-05 · HU-01, HU-07 · CA-01.1 a CA-01.5, CA-01.10, CA-05.1 |
| Parámetros | — |

**Cuerpo** (CA-01.1, decisiones 47 y 54). El solicitante se toma de `X-Usuario-Id`.

| Campo | Tipo | Reglas |
| ----- | ---- | ------ |
| `titulo` | string | Obligatorio; 5–150 caracteres tras recortar (RN-01, decisión 31) |
| `descripcion` | string | Obligatorio; 10–2000 caracteres tras recortar (RN-01, decisión 31) |
| `categoria` | `Categoria` | Obligatorio; valor de RN-02 (decisión 58) |
| `prioridad` | `Prioridad` | Obligatorio; valor de RN-02 (decisión 58) |

```http
POST /api/v1/tickets
X-Usuario-Id: 1
Content-Type: application/json

{
  "titulo": "No puedo instalar el antivirus",
  "descripcion": "El instalador muestra un error de permisos al iniciar.",
  "categoria": "Software",
  "prioridad": "Alta"
}
```

**Respuesta:** `201 Created` con el ticket (detalle) y el encabezado `Location`, que es una ruta relativa `/api/v1/tickets/{id}` (CA-01.1, decisiones 2 y 64). El ticket nace `Abierto`, sin técnico (RN-03). El vencimiento es la creación más 4 h, por ser `Alta` (RN-10, CA-01.2). `fechaActualizacion` es igual a `fechaCreacion` (decisión 25). Estado del SLA `EnTiempo`: no resuelto y el instante de referencia ≤ vencimiento (RN-11).

```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/v1/tickets/16

{
  "id": 16,
  "codigo": "HD-000016",
  "titulo": "No puedo instalar el antivirus",
  "descripcion": "El instalador muestra un error de permisos al iniciar.",
  "categoria": "Software",
  "prioridad": "Alta",
  "estado": "Abierto",
  "solicitante": { "id": 1, "nombre": "Ana Solicitante" },
  "tecnico": null,
  "fechaCreacion": "2026-10-28T14:30:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-28T18:30:00Z",
  "fechaResolucion": null,
  "fechaCierre": null,
  "estadoSla": "EnTiempo",
  "comentarios": []
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es un entero positivo de `int` | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | CA-01.5, RNF-01 |
| `403` | `ROL_NO_AUTORIZADO` | Quien crea es técnico o supervisor; no se crea el ticket | CA-01.10, RN-03 |
| `400` | `VALIDACION_FALLIDA` | Título o descripción fuera de rango o vacíos tras recortar; categoría o prioridad inexistente o numérica; campo obligatorio ausente | CA-01.3, CA-01.4, RN-01, RN-02, decisiones 31 y 58 |

### 2.2 Consultar ticket por Id

`GET /api/v1/tickets/{id}`

| Aspecto | Valor |
| ------- | ----- |
| Roles autorizados | Solicitante (solo los suyos), Técnico (solo los asignados a él), Supervisor (RN-04, matriz de permisos) |
| Origen | RF-02, RF-05 · HU-02, HU-03, HU-07, HU-13 · CA-02.1, CA-02.2, CA-02.6, CA-05.2, CA-05.3 |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

```http
GET /api/v1/tickets/14
X-Usuario-Id: 1
```

**Respuesta:** `200 OK` con el detalle completo (CA-02.1; decisiones 1, 22, 27 y 49). Los comentarios se ordenan por `fechaCreacion` ascendente y luego por `id` ascendente (decisión 68). Estado del SLA `Cumplido`: resuelto a las 16:00 con vencimiento a las 18:00 (RN-10: `Media` = 8 h; RN-11).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 14,
  "codigo": "HD-000014",
  "titulo": "Restablecer contraseña de correo",
  "descripcion": "No recuerdo la contraseña de mi cuenta de correo.",
  "categoria": "Accesos",
  "prioridad": "Media",
  "estado": "Resuelto",
  "solicitante": { "id": 1, "nombre": "Ana Solicitante" },
  "tecnico": { "id": 6, "nombre": "Fabián Técnico" },
  "fechaCreacion": "2026-10-27T10:00:00Z",
  "fechaActualizacion": "2026-10-27T16:00:00Z",
  "fechaVencimiento": "2026-10-27T18:00:00Z",
  "fechaResolucion": "2026-10-27T16:00:00Z",
  "fechaCierre": null,
  "estadoSla": "Cumplido",
  "comentarios": [
    {
      "id": 41,
      "tipo": "Solucion",
      "texto": "Se restableció la contraseña y se envió el enlace de acceso.",
      "autor": { "id": 6, "nombre": "Fabián Técnico" },
      "fechaCreacion": "2026-10-27T16:00:00Z"
    }
  ]
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | CA-02.2, decisión 65 |
| `403` | `TICKET_AJENO` | El solicitante no es el dueño, o el técnico no es el asignado | CA-02.6, RN-04 |

### 2.3 Listar tickets

`GET /api/v1/tickets`

| Aspecto | Valor |
| ------- | ----- |
| Roles autorizados | Solicitante (solo los suyos), Técnico (solo los asignados a él), Supervisor (todos) (RN-04, CA-02.5) |
| Origen | RF-02, RF-05 · HU-02, HU-03, HU-07, HU-13 · CA-02.3, CA-02.4, CA-02.5, CA-02.7, CA-02.8, CA-05.2, CA-05.3 |

| Parámetro | Ubicación | Tipo | Por defecto | Valores válidos |
| --------- | --------- | ---- | ----------- | --------------- |
| `estado` | Query | `EstadoTicket` | Sin filtro | Valores de §1.8 (decisión 58) |
| `prioridad` | Query | `Prioridad` | Sin filtro | Valores de §1.8 (decisión 58) |
| `tecnicoId` | Query | entero positivo | Sin filtro | Id de un usuario (decisión 53). Un técnico solo puede usar su propio Id (CA-02.7); el solicitante puede usarlo dentro de su visibilidad (decisión 51). Si no corresponde a un técnico existente, `200` con lista vacía (decisión 69) |
| `soloVencidos` | Query | booleano | Sin filtro | `true` devuelve solo tickets con estado del SLA `Vencido`, sin `Incumplido` (CA-02.3, decisiones 10 y 53) |
| `page` | Query | entero | `1` | ≥ 1 (RN-05, RNF-05) |
| `pageSize` | Query | entero | `20` | 1–100 (RN-05, RNF-05, decisión 9) |

Los filtros se combinan: solo se devuelven los tickets que cumplen todos (CA-02.3). Orden: `fechaCreacion` descendente y luego `id` descendente (decisión 59).

```http
GET /api/v1/tickets?page=1&pageSize=20
X-Usuario-Id: 1
```

**Respuesta:** `200 OK` con la respuesta paginada de §1.5. Cada elemento es un resumen con `id`, `codigo`, `titulo`, `estado`, `prioridad`, `tecnico`, `fechaVencimiento` y `estadoSla` (CA-02.4, decisión 21). Ana (`1`) solo ve sus tickets, el 11 y el 14 (RN-04). El 11 está `Vencido` (no resuelto, vencimiento 10:00 < 14:30) y el 14 está `Cumplido` (resuelto a las 16:00 ≤ 18:00) (RN-10, RN-11).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [
    {
      "id": 11,
      "codigo": "HD-000011",
      "titulo": "No abre el sistema de nómina",
      "estado": "Abierto",
      "prioridad": "Critica",
      "tecnico": null,
      "fechaVencimiento": "2026-10-28T10:00:00Z",
      "estadoSla": "Vencido"
    },
    {
      "id": 14,
      "codigo": "HD-000014",
      "titulo": "Restablecer contraseña de correo",
      "estado": "Resuelto",
      "prioridad": "Media",
      "tecnico": { "id": 6, "nombre": "Fabián Técnico" },
      "fechaVencimiento": "2026-10-27T18:00:00Z",
      "estadoSla": "Cumplido"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 2
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `403` | `FILTRO_NO_AUTORIZADO` | Un técnico filtra por el `tecnicoId` de otro técnico | CA-02.7 |
| `400` | `VALIDACION_FALLIDA` | `page` < 1; `pageSize` < 1 o > 100; filtro con valor inválido o enum numérico | CA-02.4, CA-02.8, RNF-05, decisiones 53 y 58 |

### 2.4 Reclasificar

`POST /api/v1/tickets/{id}/reclasificar`

| Aspecto | Valor |
| ------- | ----- |
| Roles autorizados | Técnico asignado, Supervisor (RN-13, matriz de permisos) |
| Origen | RF-01 · HU-09 · CA-01.6 a CA-01.9 |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

| Campo | Tipo | Reglas |
| ----- | ---- | ------ |
| `categoria` | `Categoria` | Obligatorio; valor de RN-02 (RN-13, decisión 58) |

Fabián (`6`), técnico asignado del ticket 12, lo reclasifica:

```http
POST /api/v1/tickets/12/reclasificar
X-Usuario-Id: 6
Content-Type: application/json

{
  "categoria": "Otros"
}
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle) (CA-01.6, decisión 23). Si la categoría es igual a la actual, responde `200` sin cambios: no se modifica `fechaActualizacion` (decisión 67). Esto solo vale para tickets modificables: si el ticket está `Resuelto` o `Cerrado`, responde `409` aunque el valor sea igual (decisión 70). `fechaActualizacion` pasa a 14:30 (decisión 74). Estado del SLA `EnTiempo` (14:30 ≤ 17:00, RN-11).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 12,
  "codigo": "HD-000012",
  "titulo": "Monitor sin imagen",
  "descripcion": "El monitor del puesto 4 no muestra imagen al encender.",
  "categoria": "Otros",
  "prioridad": "Media",
  "estado": "Asignado",
  "solicitante": { "id": 2, "nombre": "Bruno Solicitante" },
  "tecnico": { "id": 6, "nombre": "Fabián Técnico" },
  "fechaCreacion": "2026-10-28T09:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-28T17:00:00Z",
  "fechaResolucion": null,
  "fechaCierre": null,
  "estadoSla": "EnTiempo",
  "comentarios": []
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien reclasifica es un solicitante | CA-01.8, RN-13 |
| `403` | `TICKET_AJENO` | Quien reclasifica es un técnico que no es el asignado | CA-01.8, RN-13 |
| `400` | `VALIDACION_FALLIDA` | Categoría inexistente, numérica o ausente; la categoría no cambia | CA-01.9, RN-02, decisión 58 |
| `409` | `TICKET_NO_MODIFICABLE` | El ticket está `Resuelto` o `Cerrado`, aunque la categoría sea igual a la actual; la categoría no cambia | CA-01.7, RN-13, decisiones 40 y 70 |

### 2.5 Cambiar prioridad

`POST /api/v1/tickets/{id}/cambiar-prioridad`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Supervisor (RN-12, matriz de permisos) |
| Origen | RF-05 · HU-07, HU-10 · CA-05.1, CA-05.4 a CA-05.7 |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

| Campo | Tipo | Reglas |
| ----- | ---- | ------ |
| `prioridad` | `Prioridad` | Obligatorio; valor de RN-02 (decisiones 55 y 58) |

Jorge (`10`) cambia el ticket 12 de `Media` a `Critica`:

```http
POST /api/v1/tickets/12/cambiar-prioridad
X-Usuario-Id: 10
Content-Type: application/json

{
  "prioridad": "Critica"
}
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle) (CA-05.4, decisión 2). Si la prioridad es igual a la actual, responde `200` sin cambios: no se modifican `fechaActualizacion` ni `fechaVencimiento` (decisión 67). Esto solo vale para tickets modificables: si el ticket está `Resuelto` o `Cerrado`, responde `409` aunque el valor sea igual (decisión 70). El vencimiento se recalcula desde la fecha de creación original: 09:00 + 2 h = 11:00. Como el instante de referencia es posterior, el estado del SLA pasa a `Vencido` (RN-10, RN-11, RN-12). `fechaActualizacion` pasa a 14:30 (decisión 74).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 12,
  "codigo": "HD-000012",
  "titulo": "Monitor sin imagen",
  "descripcion": "El monitor del puesto 4 no muestra imagen al encender.",
  "categoria": "Hardware",
  "prioridad": "Critica",
  "estado": "Asignado",
  "solicitante": { "id": 2, "nombre": "Bruno Solicitante" },
  "tecnico": { "id": 6, "nombre": "Fabián Técnico" },
  "fechaCreacion": "2026-10-28T09:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-28T11:00:00Z",
  "fechaResolucion": null,
  "fechaCierre": null,
  "estadoSla": "Vencido",
  "comentarios": []
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien cambia la prioridad no es supervisor; la prioridad no cambia | CA-05.6, RN-12 |
| `400` | `VALIDACION_FALLIDA` | Prioridad inexistente, numérica o ausente; la prioridad no cambia | CA-05.7, RN-02, decisión 58 |
| `409` | `TICKET_NO_MODIFICABLE` | El ticket está `Resuelto` o `Cerrado`, aunque la prioridad sea igual a la actual | CA-05.5, RN-12, decisiones 40 y 70 |

### 2.6 Reporte de tickets vencidos

`GET /api/v1/reportes/tickets-vencidos`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Supervisor (CA-06.3, matriz de permisos) |
| Origen | RF-06 · HU-08 · CA-06.1 a CA-06.3 |
| Parámetros | — |

```http
GET /api/v1/reportes/tickets-vencidos
X-Usuario-Id: 10
```

**Respuesta:** `200 OK` (decisión 2) con dos agrupaciones separadas, por técnico y por prioridad, y el total (CA-06.1; decisiones 11 y 56).

- Solo se incluyen grupos con `cantidad` mayor que 0, y `total` es igual a la suma de cada agrupación (decisión 56).
- `porPrioridad` se ordena por severidad. `porTecnico` se ordena por `cantidad` descendente y luego por nombre, con `tecnico: null` ("Sin asignar") al final (CA-06.2; decisiones 50 y 57).
- En el elenco hay dos tickets vencidos, es decir, no resueltos con vencimiento anterior a las 14:30 (RN-10, RN-11): el 13 (Gabriela, `Alta`, vence a las 12:00) y el 11 (sin técnico, `Critica`, vence a las 10:00).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "porTecnico": [
    { "tecnico": { "id": 7, "nombre": "Gabriela Técnico" }, "cantidad": 1 },
    { "tecnico": null, "cantidad": 1 }
  ],
  "porPrioridad": [
    { "prioridad": "Critica", "cantidad": 1 },
    { "prioridad": "Alta", "cantidad": 1 }
  ],
  "total": 2
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `403` | `ROL_NO_AUTORIZADO` | Quien solicita el reporte no es supervisor | CA-06.3 |

---

## 3. Endpoints – Grupo B

Asignación y ciclo de estados. Valen los mismos comunes del Grupo A (§2): encabezados, `Content-Type`, `500` `ERROR_INTERNO`, orden de las tablas de errores según §1.4 y objeto detalle de §2.2. Los `409` siguen la tabla de RN-08: cualquier combinación de estado y acción que no figura en ella responde `409` `TRANSICION_INVALIDA` (RN-08, decisiones 40 y 42).

### 3.1 Asignar

`POST /api/v1/tickets/{id}/asignar`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Supervisor (RN-06, matriz de permisos) |
| Origen | RF-03 · HU-04 · CA-03.1, CA-03.3 a CA-03.5 |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

| Campo | Tipo | Reglas |
| ----- | ---- | ------ |
| `tecnicoId` | entero positivo | Obligatorio; Id de un técnico activo (RN-06, decisión 71) |

Jorge (`10`) asigna el ticket 11, que está `Abierto`, a Hugo (`8`):

```http
POST /api/v1/tickets/11/asignar
X-Usuario-Id: 10
Content-Type: application/json

{
  "tecnicoId": 8
}
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle). El ticket queda con ese técnico y pasa a `Asignado` (CA-03.1, RN-07, decisión 2). `fechaActualizacion` pasa a 14:30 (decisión 74). Estado del SLA `Vencido`: no resuelto y 14:30 > 10:00 (RN-10: `Critica` = 2 h; RN-11).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 11,
  "codigo": "HD-000011",
  "titulo": "No abre el sistema de nómina",
  "descripcion": "El sistema de nómina no abre al iniciar sesión.",
  "categoria": "Software",
  "prioridad": "Critica",
  "estado": "Asignado",
  "solicitante": { "id": 1, "nombre": "Ana Solicitante" },
  "tecnico": { "id": 8, "nombre": "Hugo Técnico" },
  "fechaCreacion": "2026-10-28T08:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-28T10:00:00Z",
  "fechaResolucion": null,
  "fechaCierre": null,
  "estadoSla": "Vencido",
  "comentarios": []
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien asigna no es supervisor | CA-03.5, RN-06 |
| `400` | `VALIDACION_FALLIDA` | `tecnicoId` ausente o no es un entero positivo (`errors.tecnicoId`) | RNF-05, decisión 71 |
| `400` | `TECNICO_NO_VALIDO` | `tecnicoId` bien formado que no existe, no tiene rol Técnico o no está activo (p. ej., Irene, `9`) | CA-03.3, RN-06, decisiones 4 y 71 |
| `409` | `TRANSICION_INVALIDA` | El ticket no está `Abierto`: si está `Asignado` o `En progreso`, corresponde reasignar (decisión 42); si está `Resuelto` o `Cerrado`, no admite asignación | CA-03.4, RN-07, RN-08 |

Los `400` se evalúan en este orden: `VALIDACION_FALLIDA`, `TECNICO_NO_VALIDO` (decisión 72).

### 3.2 Reasignar

`POST /api/v1/tickets/{id}/reasignar`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Supervisor (RN-06, matriz de permisos) |
| Origen | RF-03 · HU-11 · CA-03.2, CA-03.3, CA-03.5, CA-03.8 |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

| Campo | Tipo | Reglas |
| ----- | ---- | ------ |
| `tecnicoId` | entero positivo | Obligatorio; Id de un técnico activo distinto del asignado (RN-06, decisión 71) |

Jorge (`10`) reasigna el ticket 12, `Asignado` a Fabián (`6`), a Gabriela (`7`):

```http
POST /api/v1/tickets/12/reasignar
X-Usuario-Id: 10
Content-Type: application/json

{
  "tecnicoId": 7
}
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle). Cambia el técnico y el ticket queda en `Asignado`; también si estaba `En progreso` (CA-03.2, RN-07, RN-08). `fechaActualizacion` pasa a 14:30 (decisión 74). Estado del SLA `EnTiempo` (14:30 ≤ 17:00, RN-11).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 12,
  "codigo": "HD-000012",
  "titulo": "Monitor sin imagen",
  "descripcion": "El monitor del puesto 4 no muestra imagen al encender.",
  "categoria": "Hardware",
  "prioridad": "Media",
  "estado": "Asignado",
  "solicitante": { "id": 2, "nombre": "Bruno Solicitante" },
  "tecnico": { "id": 7, "nombre": "Gabriela Técnico" },
  "fechaCreacion": "2026-10-28T09:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-28T17:00:00Z",
  "fechaResolucion": null,
  "fechaCierre": null,
  "estadoSla": "EnTiempo",
  "comentarios": []
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien reasigna no es supervisor | CA-03.5, RN-06 |
| `400` | `VALIDACION_FALLIDA` | `tecnicoId` ausente o no es un entero positivo (`errors.tecnicoId`) | RNF-05, decisión 71 |
| `400` | `TECNICO_NO_VALIDO` | `tecnicoId` bien formado que no existe, no tiene rol Técnico o no está activo | CA-03.3, RN-06, decisión 71 |
| `400` | `MISMO_TECNICO` | `tecnicoId` es el técnico ya asignado; el ticket no cambia | CA-03.8, RN-06, decisión 14 |
| `409` | `TRANSICION_INVALIDA` | El ticket está `Abierto` (corresponde asignar, decisión 42), `Resuelto` o `Cerrado` | CA-03.4, RN-07, RN-08 |

Los `400` se evalúan en este orden: `VALIDACION_FALLIDA`, `TECNICO_NO_VALIDO`, `MISMO_TECNICO` (decisión 72).

### 3.3 Consultar técnicos activos

`GET /api/v1/tecnicos`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Supervisor (decisión 12, matriz de permisos) |
| Origen | RF-03 · HU-14 · CA-03.6, CA-03.7 |
| Parámetros | — |

```http
GET /api/v1/tecnicos
X-Usuario-Id: 10
```

**Respuesta:** `200 OK` con la lista completa, sin paginar, de técnicos activos como `{ id, nombre }` (CA-03.6; decisiones 24 y 45). Irene (`9`) no aparece porque está inactiva. La lista se ordena por nombre ascendente y luego por `id` (decisión 73).

```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  { "id": 6, "nombre": "Fabián Técnico" },
  { "id": 7, "nombre": "Gabriela Técnico" },
  { "id": 8, "nombre": "Hugo Técnico" }
]
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `403` | `ROL_NO_AUTORIZADO` | Quien consulta no es supervisor | CA-03.7 |

### 3.4 Iniciar atención

`POST /api/v1/tickets/{id}/iniciar-atencion`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Técnico asignado (RN-08, matriz de permisos) |
| Origen | RF-04 · HU-05 · CA-04.1, CA-04.6, CA-04.7 |
| Cuerpo | — |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

Fabián (`6`), técnico asignado del ticket 12, inicia la atención:

```http
POST /api/v1/tickets/12/iniciar-atencion
X-Usuario-Id: 6
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle). El ticket pasa de `Asignado` a `EnProgreso` (CA-04.1, RN-08). `fechaActualizacion` pasa a 14:30 (decisión 74). Estado del SLA `EnTiempo` (14:30 ≤ 17:00, RN-11).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 12,
  "codigo": "HD-000012",
  "titulo": "Monitor sin imagen",
  "descripcion": "El monitor del puesto 4 no muestra imagen al encender.",
  "categoria": "Hardware",
  "prioridad": "Media",
  "estado": "EnProgreso",
  "solicitante": { "id": 2, "nombre": "Bruno Solicitante" },
  "tecnico": { "id": 6, "nombre": "Fabián Técnico" },
  "fechaCreacion": "2026-10-28T09:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-28T17:00:00Z",
  "fechaResolucion": null,
  "fechaCierre": null,
  "estadoSla": "EnTiempo",
  "comentarios": []
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien inicia es un solicitante o un supervisor | CA-04.7, RN-08, decisión 7 |
| `403` | `TICKET_AJENO` | Quien inicia es un técnico que no es el asignado | CA-04.7, RN-08 |
| `409` | `TRANSICION_INVALIDA` | El ticket no está `Asignado`; el estado no cambia | CA-04.6, RN-08 |

### 3.5 Resolver

`POST /api/v1/tickets/{id}/resolver`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Técnico asignado (RN-08, matriz de permisos) |
| Origen | RF-04, RF-05 · HU-05, HU-07 · CA-04.2, CA-04.3, CA-04.6, CA-04.7, CA-04.9, CA-05.3 |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

| Campo | Tipo | Reglas |
| ----- | ---- | ------ |
| `solucion` | string | Obligatorio; como máximo 2000 caracteres tras recortar; no vacío (RN-09, decisión 31) |

Gabriela (`7`), técnica asignada del ticket 13, lo resuelve:

```http
POST /api/v1/tickets/13/resolver
X-Usuario-Id: 7
Content-Type: application/json

{
  "solucion": "Se renovó el certificado de la red Wi-Fi en la laptop."
}
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle).

- El ticket pasa a `Resuelto`, se registra la fecha de resolución y se agrega un comentario `Solucion` (CA-04.2, RN-09).
- Se resolvió a las 14:30, después del vencimiento de las 12:00 (RN-10: `Alta` = 4 h), así que el estado del SLA es `Incumplido` (RN-11).
- `fechaActualizacion` pasa a 14:30 (decisión 74).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 13,
  "codigo": "HD-000013",
  "titulo": "Sin acceso a la red Wi-Fi",
  "descripcion": "La laptop no se conecta a la red Wi-Fi de la oficina.",
  "categoria": "Red",
  "prioridad": "Alta",
  "estado": "Resuelto",
  "solicitante": { "id": 3, "nombre": "Carla Solicitante" },
  "tecnico": { "id": 7, "nombre": "Gabriela Técnico" },
  "fechaCreacion": "2026-10-28T08:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-28T12:00:00Z",
  "fechaResolucion": "2026-10-28T14:30:00Z",
  "fechaCierre": null,
  "estadoSla": "Incumplido",
  "comentarios": [
    {
      "id": 42,
      "tipo": "Solucion",
      "texto": "Se renovó el certificado de la red Wi-Fi en la laptop.",
      "autor": { "id": 7, "nombre": "Gabriela Técnico" },
      "fechaCreacion": "2026-10-28T14:30:00Z"
    }
  ]
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien resuelve es un solicitante o un supervisor | CA-04.7, RN-08, decisión 7 |
| `403` | `TICKET_AJENO` | Quien resuelve es un técnico que no es el asignado | CA-04.7, RN-08 |
| `400` | `VALIDACION_FALLIDA` | `solucion` ausente, vacía tras recortar o de más de 2000 caracteres; el estado no cambia | CA-04.3, CA-04.9, RN-09, decisión 31 |
| `409` | `TRANSICION_INVALIDA` | El ticket no está `En progreso`; el estado no cambia | CA-04.6, RN-08 |

### 3.6 Cerrar

`POST /api/v1/tickets/{id}/cerrar`

| Aspecto | Valor |
| ------- | ----- |
| Roles autorizados | Solicitante dueño, Supervisor (RN-08, matriz de permisos) |
| Origen | RF-04 · HU-06, HU-12 · CA-04.4, CA-04.6, CA-04.7 |
| Cuerpo | — |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

Ana (`1`), solicitante dueña del ticket 14, lo cierra:

```http
POST /api/v1/tickets/14/cerrar
X-Usuario-Id: 1
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle). El ticket pasa a `Cerrado` y se registra la fecha de cierre (CA-04.4). El estado del SLA sigue siendo `Cumplido`, porque "resuelto" incluye `Cerrado` y se resolvió a las 16:00, antes del vencimiento de las 18:00 (RN-11). `fechaActualizacion` pasa a 14:30 (decisión 74).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 14,
  "codigo": "HD-000014",
  "titulo": "Restablecer contraseña de correo",
  "descripcion": "No recuerdo la contraseña de mi cuenta de correo.",
  "categoria": "Accesos",
  "prioridad": "Media",
  "estado": "Cerrado",
  "solicitante": { "id": 1, "nombre": "Ana Solicitante" },
  "tecnico": { "id": 6, "nombre": "Fabián Técnico" },
  "fechaCreacion": "2026-10-27T10:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-27T18:00:00Z",
  "fechaResolucion": "2026-10-27T16:00:00Z",
  "fechaCierre": "2026-10-28T14:30:00Z",
  "estadoSla": "Cumplido",
  "comentarios": [
    {
      "id": 41,
      "tipo": "Solucion",
      "texto": "Se restableció la contraseña y se envió el enlace de acceso.",
      "autor": { "id": 6, "nombre": "Fabián Técnico" },
      "fechaCreacion": "2026-10-27T16:00:00Z"
    }
  ]
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien cierra es un técnico, incluido el asignado | CA-04.7, RN-08, decisión 7 |
| `403` | `TICKET_AJENO` | Quien cierra es un solicitante que no es el dueño | RN-08, matriz de permisos |
| `409` | `TRANSICION_INVALIDA` | El ticket no está `Resuelto`; el estado no cambia | CA-04.6, RN-08 |

### 3.7 Reabrir

`POST /api/v1/tickets/{id}/reabrir`

| Aspecto | Valor |
| ------- | ----- |
| Rol autorizado | Solicitante dueño (RN-08, matriz de permisos) |
| Origen | RF-04, RF-05 · HU-06, HU-07 · CA-04.5 a CA-04.9, CA-05.3 |

| Parámetro | Ubicación | Tipo | Valores válidos |
| --------- | --------- | ---- | --------------- |
| `id` | Ruta | entero positivo | Id de un ticket; otro valor → `404` (decisión 65) |

| Campo | Tipo | Reglas |
| ----- | ---- | ------ |
| `motivo` | string | Obligatorio; como máximo 2000 caracteres tras recortar; no vacío (RN-09, decisión 31) |

Ana (`1`), solicitante dueña del ticket 14, lo reabre:

```http
POST /api/v1/tickets/14/reabrir
X-Usuario-Id: 1
Content-Type: application/json

{
  "motivo": "El enlace de acceso venció antes de poder usarlo."
}
```

**Respuesta:** `200 OK` con el ticket actualizado (detalle).

- El ticket vuelve a `EnProgreso` con el mismo técnico, se borra la fecha de resolución y se agrega un comentario `Reapertura` (CA-04.5, RN-09).
- El vencimiento original se conserva (hallazgo 5 confirmado en [01-modelo-dominio.md §7](01-modelo-dominio.md#7-contradicciones-y-vacíos-detectados)) y ya pasó, así que el estado del SLA es `Vencido` (RN-11).
- Los comentarios se ordenan por `fechaCreacion` ascendente (decisión 68).
- `fechaActualizacion` pasa a 14:30, también por el comentario agregado (decisión 74).

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 14,
  "codigo": "HD-000014",
  "titulo": "Restablecer contraseña de correo",
  "descripcion": "No recuerdo la contraseña de mi cuenta de correo.",
  "categoria": "Accesos",
  "prioridad": "Media",
  "estado": "EnProgreso",
  "solicitante": { "id": 1, "nombre": "Ana Solicitante" },
  "tecnico": { "id": 6, "nombre": "Fabián Técnico" },
  "fechaCreacion": "2026-10-27T10:00:00Z",
  "fechaActualizacion": "2026-10-28T14:30:00Z",
  "fechaVencimiento": "2026-10-27T18:00:00Z",
  "fechaResolucion": null,
  "fechaCierre": null,
  "estadoSla": "Vencido",
  "comentarios": [
    {
      "id": 41,
      "tipo": "Solucion",
      "texto": "Se restableció la contraseña y se envió el enlace de acceso.",
      "autor": { "id": 6, "nombre": "Fabián Técnico" },
      "fechaCreacion": "2026-10-27T16:00:00Z"
    },
    {
      "id": 43,
      "tipo": "Reapertura",
      "texto": "El enlace de acceso venció antes de poder usarlo.",
      "autor": { "id": 1, "nombre": "Ana Solicitante" },
      "fechaCreacion": "2026-10-28T14:30:00Z"
    }
  ]
}
```

| HTTP | `codigo` | Condición | Origen |
| ---- | -------- | --------- | ------ |
| `401` | `USUARIO_NO_IDENTIFICADO` | Falta `X-Usuario-Id` o no es válido | RNF-01, decisión 30 |
| `401` | `USUARIO_INEXISTENTE` | El Id no corresponde a ningún usuario | RNF-01 |
| `404` | `TICKET_NO_ENCONTRADO` | No existe el ticket `{id}`, o `{id}` no es un entero positivo | RNF-04, decisiones 4 y 65 |
| `403` | `ROL_NO_AUTORIZADO` | Quien reabre es un técnico o un supervisor | CA-04.7, RN-08, decisión 7 |
| `403` | `TICKET_AJENO` | Quien reabre es un solicitante que no es el dueño | RN-08, matriz de permisos |
| `400` | `VALIDACION_FALLIDA` | `motivo` ausente, vacío tras recortar o de más de 2000 caracteres; el estado no cambia | CA-04.8, CA-04.9, RN-09, decisión 31 |
| `409` | `TRANSICION_INVALIDA` | El ticket no está `Resuelto` (por ejemplo, `Cerrado`); el estado no cambia | CA-04.6, RN-08 |

---

## 4. Ejemplos de ProblemDetails

`type` y `title` son los valores por defecto de ASP.NET Core (decisión 29). Los textos de los mensajes en `errors` son ilustrativos (decisión 61), y el valor de `traceId` también (decisión 62).

### 4.1 `400` · Validación

Ana Solicitante (`1`) crea un ticket con un título de 3 caracteres, una descripción que queda vacía tras recortar espacios y una categoría inexistente (CA-01.3, CA-01.4, RN-01, RN-02, decisión 31).

```http
POST /api/v1/tickets
X-Usuario-Id: 1
Content-Type: application/json

{
  "titulo": "Red",
  "descripcion": "     ",
  "categoria": "Impresoras",
  "prioridad": "Alta"
}
```

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "codigo": "VALIDACION_FALLIDA",
  "traceId": "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-00",
  "errors": {
    "titulo": ["El título debe tener entre 5 y 150 caracteres."],
    "descripcion": ["La descripción es obligatoria."],
    "categoria": ["La categoría no es válida."]
  }
}
```

El ticket no se crea (CA-01.3).

### 4.2 `409` · Transición inválida

Fabián Técnico (`6`) es el técnico asignado del ticket `HD-000015` (Id `15` del elenco de §1.10), que está `Cerrado`, e intenta resolverlo. El rol y la asignación son válidos, así que no aplica el `403`. El cuerpo es válido, así que no aplica el `400`. `Cerrado` es un estado final (RN-08, CA-04.6).

```http
POST /api/v1/tickets/15/resolver
X-Usuario-Id: 6
Content-Type: application/json

{
  "solucion": "Se reinició el servicio de impresión."
}
```

```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "codigo": "TRANSICION_INVALIDA",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00"
}
```

El estado del ticket no cambia (CA-04.6, RNF-04).

---

## 5. Verificación

Revisión del 28/10/2026 contra los 45 CA de [01-requisitos-funcionales.md](../functional/01-requisitos-funcionales.md), la [matriz de permisos](../functional/02-reglas-negocio.md#matriz-de-permisos) y el catálogo de `codigo` de [ADR-004](adr/ADR-004-problemdetails-errores.md#enmienda-3--28102026).

### 5.1 CA → endpoints

| CA | Endpoints que lo cubren |
| -- | ----------------------- |
| CA-01.1 | §2.1 Crear (`201`, `Location`) |
| CA-01.2 | §2.1 Crear (`fechaVencimiento` en la respuesta) |
| CA-01.3 | §2.1 Crear (`400` `VALIDACION_FALLIDA`) |
| CA-01.4 | §2.1 Crear (`400` `VALIDACION_FALLIDA`) |
| CA-01.5 | §2.1 Crear (`401` `USUARIO_INEXISTENTE`) |
| CA-01.6 | §2.4 Reclasificar (`200`) |
| CA-01.7 | §2.4 Reclasificar (`409` `TICKET_NO_MODIFICABLE`) |
| CA-01.8 | §2.4 Reclasificar (`403` `ROL_NO_AUTORIZADO` / `TICKET_AJENO`) |
| CA-01.9 | §2.4 Reclasificar (`400` `VALIDACION_FALLIDA`) |
| CA-01.10 | §2.1 Crear (`403` `ROL_NO_AUTORIZADO`) |
| CA-02.1 | §2.2 Consultar por Id (`200` detalle) |
| CA-02.2 | §2.2 Consultar por Id (`404` `TICKET_NO_ENCONTRADO`) |
| CA-02.3 | §2.3 Listar (filtros) |
| CA-02.4 | §2.3 Listar (paginación, orden, resumen, `400`) |
| CA-02.5 | §2.3 Listar (visibilidad por rol) |
| CA-02.6 | §2.2 Consultar por Id (`403` `TICKET_AJENO`) |
| CA-02.7 | §2.3 Listar (`403` `FILTRO_NO_AUTORIZADO`) |
| CA-02.8 | §2.3 Listar (`400` `VALIDACION_FALLIDA`) |
| CA-03.1 | §3.1 Asignar (`200`) |
| CA-03.2 | §3.2 Reasignar (`200`) |
| CA-03.3 | §3.1 Asignar, §3.2 Reasignar (`400` `TECNICO_NO_VALIDO`) |
| CA-03.4 | §3.1 Asignar, §3.2 Reasignar (`409` `TRANSICION_INVALIDA`) |
| CA-03.5 | §3.1 Asignar, §3.2 Reasignar (`403` `ROL_NO_AUTORIZADO`) |
| CA-03.6 | §3.3 Consultar técnicos activos (`200`) |
| CA-03.7 | §3.3 Consultar técnicos activos (`403` `ROL_NO_AUTORIZADO`) |
| CA-03.8 | §3.2 Reasignar (`400` `MISMO_TECNICO`) |
| CA-04.1 | §3.4 Iniciar atención (`200`) |
| CA-04.2 | §3.5 Resolver (`200`) |
| CA-04.3 | §3.5 Resolver (`400` `VALIDACION_FALLIDA`) |
| CA-04.4 | §3.6 Cerrar (`200`) |
| CA-04.5 | §3.7 Reabrir (`200`) |
| CA-04.6 | §3.4 Iniciar atención, §3.5 Resolver, §3.6 Cerrar, §3.7 Reabrir (`409` `TRANSICION_INVALIDA`) |
| CA-04.7 | §3.4 Iniciar atención, §3.5 Resolver, §3.6 Cerrar, §3.7 Reabrir (`403`) |
| CA-04.8 | §3.7 Reabrir (`400` `VALIDACION_FALLIDA`) |
| CA-04.9 | §3.5 Resolver, §3.7 Reabrir (`400` `VALIDACION_FALLIDA`) |
| CA-05.1 | §2.1 Crear, §2.5 Cambiar prioridad (`fechaVencimiento` según RN-10) |
| CA-05.2 | §2.2 Consultar por Id, §2.3 Listar (`estadoSla` `EnTiempo` / `Vencido`) |
| CA-05.3 | §2.2 Consultar por Id, §2.3 Listar (`estadoSla` `Cumplido` / `Incumplido`); §3.5 Resolver (`Incumplido` al resolver), §3.7 Reabrir (deja de ser resuelto) |
| CA-05.4 | §2.5 Cambiar prioridad (`200`, vencimiento recalculado) |
| CA-05.5 | §2.5 Cambiar prioridad (`409` `TICKET_NO_MODIFICABLE`) |
| CA-05.6 | §2.5 Cambiar prioridad (`403` `ROL_NO_AUTORIZADO`) |
| CA-05.7 | §2.5 Cambiar prioridad (`400` `VALIDACION_FALLIDA`) |
| CA-06.1 | §2.6 Reporte de vencidos (`200`) |
| CA-06.2 | §2.6 Reporte de vencidos (`tecnico: null`) |
| CA-06.3 | §2.6 Reporte de vencidos (`403` `ROL_NO_AUTORIZADO`) |

**Resultado: limpia.** Los 45 CA tienen al menos un endpoint y todos se citan en la fila "Origen" de las operaciones que los cubren. CA-05.1 a CA-05.3 se agregaron el 28/10/2026.

### 5.2 Rol × endpoint

✔ autorizado · ✔\* autorizado con restricción · `403` no autorizado.

| Endpoint | Solicitante | Técnico | Supervisor | Matriz de permisos |
| -------- | ----------- | ------- | ---------- | ------------------ |
| §2.1 Crear ticket | ✔ | `403` | `403` | Coincide |
| §2.2 Consultar por Id | ✔\* los suyos | ✔\* los asignados | ✔ | Coincide |
| §2.3 Listar | ✔\* los suyos | ✔\* los asignados; otro `tecnicoId` → `403` | ✔ | Coincide |
| §2.4 Reclasificar | `403` | ✔\* el asignado | ✔ | Coincide |
| §2.5 Cambiar prioridad | `403` | `403` | ✔ | Coincide |
| §2.6 Reporte de vencidos | `403` | `403` | ✔ | Coincide |
| §3.1 Asignar | `403` | `403` | ✔ | Coincide |
| §3.2 Reasignar | `403` | `403` | ✔ | Coincide |
| §3.3 Consultar técnicos activos | `403` | `403` | ✔ | Coincide |
| §3.4 Iniciar atención | `403` | ✔\* el asignado | `403` | Coincide |
| §3.5 Resolver | `403` | ✔\* el asignado | `403` | Coincide |
| §3.6 Cerrar | ✔\* el dueño | `403` | ✔ | Coincide |
| §3.7 Reabrir | ✔\* el dueño | `403` | `403` | Coincide |

**Resultado: limpia.** Los permisos de las 13 operaciones coinciden con las 13 filas de la matriz de permisos, que desde el 28/10/2026 separa "Asignar" y "Reasignar" (decisión 42).

### 5.3 `codigo` → endpoints

| `codigo` | HTTP | Endpoints donde puede aparecer |
| -------- | ---- | ------------------------------ |
| `VALIDACION_FALLIDA` | `400` | §2.1, §2.3, §2.4, §2.5, §3.1, §3.2, §3.5, §3.7 |
| `TECNICO_NO_VALIDO` | `400` | §3.1, §3.2 |
| `MISMO_TECNICO` | `400` | §3.2 |
| `USUARIO_NO_IDENTIFICADO` | `401` | Las 13 operaciones |
| `USUARIO_INEXISTENTE` | `401` | Las 13 operaciones |
| `ROL_NO_AUTORIZADO` | `403` | §2.1, §2.4, §2.5, §2.6, §3.1, §3.2, §3.3, §3.4, §3.5, §3.6, §3.7 |
| `TICKET_AJENO` | `403` | §2.2, §2.4, §3.4, §3.5, §3.6, §3.7 |
| `FILTRO_NO_AUTORIZADO` | `403` | §2.3 |
| `TICKET_NO_ENCONTRADO` | `404` | §2.2, §2.4, §2.5, §3.1, §3.2, §3.4, §3.5, §3.6, §3.7 (las operaciones con `{id}`) |
| `TRANSICION_INVALIDA` | `409` | §3.1, §3.2, §3.4, §3.5, §3.6, §3.7 |
| `TICKET_NO_MODIFICABLE` | `409` | §2.4, §2.5 |
| `ERROR_INTERNO` | `500` | Las 13 operaciones |

**Resultado: limpia.** Se usan los 12 valores del catálogo de ADR-004, y no aparece ningún `codigo` fuera de él.
