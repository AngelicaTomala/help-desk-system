# ADR-004 · Manejo de errores con Problem Details

| Campo  | Valor      |
| ------ | ---------- |
| Estado | Aceptado   |
| Fecha  | 27/10/2026 |

## Contexto

La API responde con cinco tipos de error definidos por los CA: `400` validación, `401` identidad (S-02), `403` permiso, `404` recurso inexistente y `409` transición o estado no permitido (RN-07, RN-08, RN-12, RN-13). RNF-03 exige que todos los errores usen Problem Details (RFC 9457) y que un `500` no exponga stack traces fuera de Development. RNF-04 exige que el mapeo de códigos sea uniforme y que una operación rechazada no modifique datos.

## Decisión

Usar el soporte nativo de ASP.NET Core, sin librerías externas: `AddProblemDetails()`, `UseExceptionHandler()` y un `IExceptionHandler` en el proyecto Api que traduce excepciones a respuestas `application/problem+json` (`ValidationProblemDetails` para el `400`).

| Excepción (capa de origen)                        | HTTP  |
| ------------------------------------------------- | ----- |
| Validación de FluentValidation (Application)      | `400` con `errors` por campo |
| Usuario no identificado o inexistente (Api)       | `401` |
| Acceso denegado por rol o dueño (Application)     | `403` |
| Recurso no encontrado (Application)               | `404` |
| `TransicionInvalidaException` y conflictos de estado (Domain) | `409` |
| Cualquier otra                                    | `500` sin detalle fuera de Development |

La excepción se lanza antes de `SaveChanges`, de modo que una operación rechazada no persiste cambios (RNF-04).

## Consecuencias

**Positivas**

- Formato estándar y único para todos los errores, documentable en OpenAPI (RNF-03, RNF-09).
- Domain y Application no conocen HTTP: solo lanzan excepciones con significado de negocio (RNF-07).
- Un único punto donde se decide el código; las pruebas de integración verifican `type`, `title` y `status` (RNF-03).

**Negativas**

- Usar excepciones para casos esperados (`403`, `409`) tiene un costo de rendimiento, despreciable para el volumen del MVP.
- La tabla de mapeo debe mantenerse al día cuando se agreguen nuevas excepciones.

## Alternativas consideradas

| Alternativa                                     | Motivo de descarte                                                                                                  |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| Formato de error propio (`{ "error": "..." }`)  | No es estándar, incumple RNF-03 y obliga a documentar un contrato adicional.                                         |
| `try/catch` en cada controlador                 | Repite la lógica de mapeo, es fácil que los códigos diverjan (RNF-04) y pone lógica en los controladores (RNF-08).   |
| Patrón `Result<T>` sin excepciones              | Válido, pero agrega código de propagación en cada handler; se puede adoptar más adelante sin cambiar el formato HTTP. |

## Requisitos relacionados

RNF-01, RNF-03, RNF-04, RNF-07, RNF-09 · RN-07, RN-08, RN-12, RN-13 · CA-01.5, CA-01.7, CA-02.2, CA-02.6, CA-03.4, CA-04.6, CA-05.5 · S-02

## Enmienda · 28/10/2026

Origen: decisiones 4, 5, 6 y 7 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos H-17, H-18, H-33, H-34).

- **`404` solo para el recurso de la ruta.** "Recurso no encontrado" se refiere únicamente al recurso identificado en la ruta (el ticket). Una referencia inválida en el cuerpo, como un técnico inexistente al asignar, es un dato inválido y responde `400` (CA-03.3).
- **Excepciones de dominio.** `TransicionInvalidaException` → `409`; `InvarianteVioladaException` → `500` sin detalle fuera de Development (cubierta por la fila "Cualquier otra").
- **Orden de evaluación.** Cuando aplican varios errores, se responde el primero de: `401` → `404` → `403` → `400` → `409` (RNF-04).
- **Actor no autorizado.** Un usuario que no figura en RN-08 como autorizado para una acción (p. ej., el técnico asignado que cierra o reabre, o el supervisor que inicia atención o resuelve) recibe `403` (CA-04.7).

## Enmienda 2 · 28/10/2026

Origen: corrección de la documentación, parte 2 ([auditoria-documentacion.md](../../../docs/auditoria-documentacion.md), hallazgo H-20).

- En la tabla de mapeo, "Usuario no identificado o inexistente (Api)" significa que la Api decide el `401` con el usuario que obtiene a través de Application, sin acceder a Infrastructure (ver [ADR-001](ADR-001-clean-architecture.md) y [02-arquitectura-c4.md §2](../02-arquitectura-c4.md#2-nivel-2--contenedores)).

## Enmienda 3 · 28/10/2026

Origen: decisiones 28, 29 y 30 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos H-20, H-41 y H-43).

- **Confirmación:** se confirma el mecanismo de la Enmienda 2 para el `401`.
- **Esquema de error:** `type` y `title` usan los valores por defecto de ASP.NET Core. Se agrega el campo de extensión `codigo` con el caso de negocio, según el catálogo siguiente.
- **`X-Usuario-Id`:** debe ser un entero positivo dentro del rango de `int`; cualquier otro valor (ausente, no numérico, `0`, negativo o fuera de rango) responde `401`.

**Catálogo de `codigo` (propuesto el 28/10/2026, pendiente de aprobación):**

| `codigo`                  | HTTP  | Caso                                                                                              | Origen                                                                 |
| ------------------------- | ----- | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| `VALIDACION_FALLIDA`      | `400` | Datos de entrada inválidos detectados por un validador; el detalle va en `errors` por campo.     | CA-01.3, CA-01.4, CA-01.9, CA-02.4, CA-02.8, CA-04.3, CA-04.8, CA-04.9, CA-05.7 · RN-01, RN-02, RN-05, RN-09 |
| `TECNICO_NO_VALIDO`       | `400` | Al asignar: el técnico no existe, no tiene rol Técnico o no está activo.                         | CA-03.3 · RN-06                                                        |
| `MISMO_TECNICO`           | `400` | Reasignar al técnico que ya está asignado.                                                       | CA-03.8 · RN-06                                                        |
| `USUARIO_NO_IDENTIFICADO` | `401` | Falta `X-Usuario-Id` o su valor no es un entero positivo dentro del rango de `int`.              | RNF-01 · S-02                                                          |
| `USUARIO_INEXISTENTE`     | `401` | El Id es válido, pero no corresponde a ningún usuario.                                           | CA-01.5 · RNF-01                                                       |
| `ROL_NO_AUTORIZADO`       | `403` | El rol del usuario no puede ejecutar la operación (matriz de permisos).                          | CA-01.8 (solicitante), CA-01.10, CA-03.5, CA-03.7, CA-04.7, CA-05.6, CA-06.3 |
| `TICKET_AJENO`            | `403` | El rol puede ejecutar la operación, pero el ticket no es suyo (solicitante) o no está asignado a él (técnico). | CA-01.8 (técnico no asignado), CA-02.6, CA-04.7 · RN-04, RN-08, RN-13 |
| `FILTRO_NO_AUTORIZADO`    | `403` | Un técnico filtra el listado por el `tecnicoId` de otro técnico.                                 | CA-02.7                                                                |
| `TICKET_NO_ENCONTRADO`    | `404` | El ticket de la ruta no existe.                                                                  | CA-02.2                                                                |
| `TRANSICION_INVALIDA`     | `409` | La transición no está permitida por RN-08 (incluye asignar un ticket `Resuelto` o `Cerrado`). `TransicionInvalidaException`. | CA-03.4, CA-04.6 · RN-07, RN-08 |
| `TICKET_NO_MODIFICABLE`   | `409` | Reclasificar o cambiar la prioridad de un ticket `Resuelto` o `Cerrado`. `TransicionInvalidaException`. | CA-01.7, CA-05.5 · RN-12, RN-13                         |
| `ERROR_INTERNO`           | `500` | Error no controlado, incluida `InvarianteVioladaException`; sin detalle fuera de Development.    | RNF-03 · ADR-003                                                       |

## Enmienda 4 · 28/10/2026

Origen: decisiones 39, 40 y 41 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas).

- **Catálogo aprobado:** el catálogo de `codigo` de la Enmienda 3 queda aprobado tal como se propuso, con los códigos separados (decisión 39).
- **Propiedad `Codigo` en las excepciones de dominio:** las excepciones de dominio exponen una propiedad `Codigo` con un valor del catálogo. `TransicionInvalidaException` se lanza con `TRANSICION_INVALIDA` (transición no permitida por RN-08) o con `TICKET_NO_MODIFICABLE` (reclasificar o cambiar la prioridad de un ticket `Resuelto` o `Cerrado`). El `IExceptionHandler` de la Api copia ese valor al campo `codigo` de Problem Details. Así, ambos casos comparten el `409` y la misma excepción, pero el cliente los distingue por `codigo` (decisión 40).
- **Errores `500`:** la Api ignora el `Codigo` de la excepción, incluida `InvarianteVioladaException`, y responde siempre `ERROR_INTERNO`, sin exponer detalles internos. El detalle de la excepción se registra en el log (decisión 41).

## Enmienda 5 · 29/10/2026

Origen: decisiones 75, 80 y 82 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (revisión de Domain).

- **`Codigo` de `InvarianteVioladaException`:** la excepción se lanza con el `Codigo` `ERROR_INTERNO`, un valor del catálogo, como pide la Enmienda 4. La Api sigue respondiendo `ERROR_INTERNO` en todos los `500`, sin exponer detalles internos (decisión 41).
- **Casos cruzados de `TRANSICION_INVALIDA`:** además de asignar un ticket `Resuelto` o `Cerrado`, que ya figura en el catálogo, `TRANSICION_INVALIDA` incluye asignar un ticket que no está `Abierto` y reasignar uno que no está `Asignado` ni `En progreso` (decisiones 42 y 75).

## Enmienda 6 · 30/10/2026

Origen: decisiones 93, 94 y 98 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos R-02, R-03 y R-07 de la revisión de RF-01).

- **Cuerpo que no se puede leer:** un cuerpo ausente, vacío, `null`, que no es un objeto JSON o con JSON mal formado responde `400` `VALIDACION_FALLIDA` con la clave `cuerpo` en `errors`. Una petición sin `Content-Type` se trata como un cuerpo ausente o ilegible. Un campo con un tipo JSON incorrecto responde `400` `VALIDACION_FALLIDA` con la clave de ese campo, y solo se informa ese error, porque la lectura del JSON se detiene en el primer error de tipo. Es una excepción al orden de evaluación de la Enmienda 1: todo error de lectura del cuerpo, incluido un campo con tipo JSON incorrecto, ocurre antes del handler, así que su `400` se responde antes que el `404` y el `403` (decisión 93).
- **Respuestas del framework sin `codigo`:** las rutas inexistentes (`404`), los métodos no admitidos (`405`) y los tipos de contenido no admitidos (`415`) responden con Problem Details (RNF-03), con `traceId` y sin el campo `codigo`, porque no son casos de negocio y quedan fuera del catálogo de la Enmienda 3. El `415` se responde cuando el `Content-Type` está presente y no es JSON; si falta, aplica el punto anterior (decisión 94).
- **Errores de Application:** Application señala sus errores con excepciones que llevan `Codigo`: una para el `404` (`TICKET_NO_ENCONTRADO`), otra para el `403`, que admite cualquier `codigo` `403` del catálogo (`ROL_NO_AUTORIZADO`, `TICKET_AJENO` y `FILTRO_NO_AUTORIZADO`), y otra para los `400`, que admite cualquier `codigo` `400` del catálogo (`VALIDACION_FALLIDA` con `errors`, `TECNICO_NO_VALIDO` y `MISMO_TECNICO`). Las constantes de esos códigos viven en Application. El `IExceptionHandler` de la Api las traduce a Problem Details y copia su `Codigo` al campo `codigo`, como con las excepciones de dominio de la Enmienda 4 (decisión 98).
