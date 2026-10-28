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
