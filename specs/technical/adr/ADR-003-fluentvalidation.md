# ADR-003 · Validación de entrada con FluentValidation

| Campo  | Valor      |
| ------ | ---------- |
| Estado | Aceptado   |
| Fecha  | 27/10/2026 |

## Contexto

Parte de las reglas son de formato de la petición: longitudes de título y descripción (RN-01), categoría y prioridad válidas (RN-02, RN-13), paginación (RN-05), solución y motivo obligatorios con máximo 2000 caracteres (RN-09). Los CA exigen `400` con el detalle del error (CA-01.3, CA-01.4, CA-01.9, CA-02.4, CA-04.3) y RNF-05 pide validar antes del dominio, devolver errores por campo y probar cada validador con valores frontera. Otras reglas dependen del estado del ticket (RN-07, RN-08, RN-12, RN-13) y responden `409`.

## Decisión

Usar **FluentValidation** en Application, con un validador por comando o consulta (`CrearTicketValidator`, `ReclasificarTicketValidator`, `ListarTicketsValidator`, `ResolverTicketValidator`, `ReabrirTicketValidator`). Se invoca explícitamente antes del handler; si falla, Api responde `400` con `errors` por campo (ADR-004).

Reparto de responsabilidades:

| Tipo de regla                                         | Dónde                    | Respuesta | Ejemplos                       |
| ----------------------------------------------------- | ------------------------ | --------- | ------------------------------ |
| Formato de la entrada (obligatorio, longitud, valor de enum, rango) | Validador (Application) | `400` con detalle por campo | RN-01, RN-02, RN-05, RN-09 (texto), RN-13 (categoría válida) |
| Invariantes de la entidad                             | Entidad `Ticket` (Domain) | Excepción de dominio | RN-01 (longitudes de título y descripción), RN-09 (solución y motivo obligatorios, ≤ 2000) |
| Reglas que dependen del estado del ticket             | Entidad `Ticket` (Domain) | `409`    | RN-07, RN-08, RN-12, RN-13 (no `Resuelto`/`Cerrado`) |
| Reglas que dependen de datos o del actor              | Handler (Application)    | `400`/`403` | RN-04, RN-06                 |

- Los límites (5–150, 10–2000, 1–50, 2000) se declaran **una sola vez** como constantes públicas en Domain (p. ej., `Ticket.TituloLongitudMaxima`, `Comentario.TextoLongitudMaxima`). Los validadores y las invariantes de Domain usan esas mismas constantes, así que la regla se define en un solo lugar aunque se compruebe en dos.
- `Ticket` valida las longitudes de RN-01 al crearse, y `Resolver` y `Reabrir` validan que la solución o el motivo no estén vacíos ni superen `Comentario.TextoLongitudMaxima` (RN-09). Si se violan, lanzan una excepción de dominio: un Ticket inválido, como uno resuelto sin solución, nunca puede existir (RNF-08).
- El validador de Application se ejecuta antes y es el que responde `400` con el detalle por campo (CA-01.3, CA-04.3). Si la excepción de dominio llega a la API, indica que falta un validador y se trata como error de programación (`500`, ADR-004).

## Consecuencias

**Positivas**

- Cada validador es una clase pura que se prueba con valores frontera sin HTTP (RNF-05, RNF-10).
- Devuelve todos los errores a la vez y agrupados por campo, como exige RNF-05.
- Un Ticket nunca queda con longitudes inválidas ni resuelto o reabierto sin texto, aunque se opere fuera de un caso de uso (p. ej., en una prueba) (RNF-08).
- Sin duplicación de límites: validadores e invariantes comparten las constantes de Domain.

**Negativas**

- RN-01 y RN-09 se comprueban dos veces (validador e invariante) y necesitan pruebas en ambas capas.
- Dependencia adicional y un paso de invocación explícito en cada caso de uso (no se usa la autovalidación de `FluentValidation.AspNetCore`, ya no recomendada).

## Alternativas consideradas

| Alternativa                            | Motivo de descarte                                                                                                       |
| -------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| DataAnnotations en los DTO             | Mezcla validación con contratos HTTP, es incómodo para reglas condicionales y difícil de probar de forma aislada (RNF-05). |
| Validar todo en Domain con excepciones | Se detiene en el primer error y no entrega detalle por campo (CA-01.3, RNF-05).                                          |
| Validar en ambas capas con límites propios en cada una | Los números se repiten y pueden divergir entre capas.                                                    |
| Validar solo en Application                | Un Ticket operado sin pasar por el validador podría tener longitudes inválidas o quedar resuelto sin solución, lo que contradice RNF-08.        |

## Requisitos relacionados

RNF-05, RNF-08, RNF-10 · RN-01, RN-02, RN-05, RN-09, RN-13 · CA-01.3, CA-01.4, CA-01.9, CA-02.4, CA-04.3

## Enmienda · 28/10/2026

Origen: decisiones 5, 9, 10, 14 y 15 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos H-18, H-35, H-36, H-37).

- La "excepción de dominio" de las invariantes de RN-01 y RN-09 es `InvarianteVioladaException` → `500`. Las reglas de estado usan `TransicionInvalidaException` → `409` (ver ADR-001 y ADR-004).
- El rango de paginación es `pageSize` 1–100 (antes 1–50), con valores por defecto `page` = 1 y `pageSize` = 20 (RN-05).
- `ListarTicketsValidator` también rechaza con `400` los valores de filtro inválidos (CA-02.8).
- Responden `400` desde los validadores: reabrir sin motivo (CA-04.8), solución o motivo de más de 2000 caracteres (CA-04.9) y prioridad inexistente al cambiarla (CA-05.7). Reasignar al mismo técnico (CA-03.8) también responde `400`.

## Enmienda 2 · 28/10/2026

Origen: decisiones 31 y 34 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos H-44 y H-26).

- **Recorte de textos:** los textos se recortan antes de validar; si quedan vacíos, el validador responde `400` (RN-01, RN-09).
- **Constantes de paginación:** viven en Application, junto a `ListarTicketsValidator`, y no en Domain. Las demás constantes (longitudes de RN-01 y RN-09) siguen en Domain.

## Enmienda 3 · 29/10/2026

Origen: decisión 79 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgo D-05 de la revisión de Domain).

- **Recorte y valor guardado:** Application recorta los textos antes de validar y de llamar a Domain. Domain no recorta: valida el texto que recibe (no vacío y dentro de las longitudes de RN-01 y RN-09) con las mismas constantes y guarda ese valor.

## Enmienda 4 · 29/10/2026

Origen: decisión 83 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgo D-06 de la revisión de Domain).

- **Texto recortado como invariante:** Domain exige que el texto llegue ya recortado. `Ticket.Crear`, `Ticket.Resolver` y `Ticket.Reabrir` lanzan `InvarianteVioladaException` si el texto está en blanco (vacío o solo espacios) o tiene espacios al inicio o al final. Las longitudes de RN-01 y RN-09 se miden sobre ese texto. Precisa el "no vacío" de la Enmienda 3.
- **Definición de espacio:** "espacio" es cualquier carácter de espacio en blanco, con la misma definición que `string.Trim()` y `string.IsNullOrWhiteSpace()` de .NET (incluye tabuladores y saltos de línea), tanto en Domain como en Application. Los espacios y saltos de línea en medio del texto están permitidos; la regla solo aplica a los extremos y al texto completamente en blanco.

## Enmienda 5 · 29/10/2026

Origen: decisión 84 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgo D-07 de la revisión de regresión de Domain).

- **Valores de enum no definidos:** `Ticket.Crear`, `Ticket.CambiarPrioridad` y `Ticket.Reclasificar` lanzan `InvarianteVioladaException` (`Codigo` `ERROR_INTERNO`) si reciben un valor de `Categoria` o `Prioridad` que no está definido en el enum, y `PoliticaSla.HorasPara` lanza la misma excepción con una prioridad no definida. Así, las invariantes de Domain de la tabla de reparto se amplían a RN-02. Por un caso de uso no ocurre: el validador lo rechaza antes con `400` (decisión 58).
- **Orden:** la comprobación del valor del enum se hace antes que la guarda de estado. Un valor no definido sobre un ticket `Resuelto` o `Cerrado` lanza `InvarianteVioladaException`, no `TICKET_NO_MODIFICABLE`, en el mismo orden que el `400` antes del `409` (decisión 6).

## Enmienda 6 · 30/10/2026

Origen: decisiones 85 y 86 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (plan de implementación de Domain).

- **Orden en `Resolver` y `Reabrir`:** la invariante del texto (RN-09) se comprueba antes que la regla de estado (RN-08). Un texto inválido sobre un estado no permitido lanza `InvarianteVioladaException` (`Codigo` `ERROR_INTERNO`), no `TransicionInvalidaException` con `TRANSICION_INVALIDA`. Sigue el mismo orden que el `400` antes del `409` (decisión 6) y que la Enmienda 5. Por un caso de uso no ocurre: el validador rechaza antes el texto con `400` (CA-04.3, CA-04.8, CA-04.9).
- **Reasignar al mismo técnico:** CA-03.8 responde `400` `MISMO_TECNICO` desde `ReasignarTicketHandler`, no desde un validador, porque necesita conocer el técnico actual del ticket (decisiones 81 y 86). `Ticket.Reasignar` no lo rechaza. Precisa lo que dice la Enmienda 1 sobre CA-03.8.

## Enmienda 7 · 30/10/2026

Origen: decisiones 92, 93 y 96 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos R-01, R-02 y R-05 de la revisión de RF-01).

- **Quién invoca el validador:** donde la Decisión dice "Se invoca explícitamente antes del handler", léase que **el handler** invoca el validador, en el orden de la decisión 6. Primero comprueba la existencia del recurso de la ruta (`404`), después el permiso (`403`), después invoca el validador (`400`) y al final llama a Domain (`409`). El validador sigue siendo una clase de FluentValidation, y sigue sin usarse la autovalidación de `FluentValidation.AspNetCore`.
- **Errores de lectura del cuerpo:** un campo con un tipo JSON incorrecto se rechaza al leer el cuerpo, antes del handler y del validador, y solo se informa el error de ese campo, porque la lectura del JSON se detiene en el primer error de tipo. Es una excepción a "Devuelve todos los errores a la vez" de las Consecuencias (decisión 93).
- **Enums de entrada como texto:** los DTO de petición reciben `categoria` y `prioridad` como texto. El validador los convierte sin distinguir mayúsculas y rechaza los textos numéricos con `400` (decisión 58). El esquema OpenAPI de esos campos se anota con los valores del enum (RNF-09).
- **`{id}` de la ruta:** llega a Application como `int`, ya validado por la Api con el formato de la decisión 95.

## Enmienda 8 · 31/10/2026

Origen: decisiones 101 a 105 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos R2-02 a R2-06 de la revisión de RF-02).

- **Parámetros de la URL como texto:** los parámetros de la URL del listado (`estado`, `prioridad`, `tecnicoId`, `soloVencidos`, `page` y `pageSize`) llegan a Application como texto, igual que los enums del cuerpo de la Enmienda 7. Los valida `ListarTicketsValidator`, que el handler invoca después del `403` (Enmienda 7, decisión 92). Un error de la URL no es un error de lectura del cuerpo, así que no aplica la excepción de la decisión 93. El documento OpenAPI anota sus valores y su formato (RNF-09) (decisión 101).
- **`soloVencidos`:** acepta solo `true` y `false`, sin distinguir mayúsculas; `false` equivale a no enviarlo; cualquier otro valor responde `400` `VALIDACION_FALLIDA` (decisión 102).
- **Parámetros repetidos:** un parámetro que aparece más de una vez responde `400` `VALIDACION_FALLIDA` con la clave de ese parámetro (decisión 103).
- **Valores vacíos:** un parámetro presente con valor vacío responde `400` `VALIDACION_FALLIDA` con su clave; solo un parámetro ausente toma su valor por defecto o no filtra (decisión 104).
- **Formato de los números:** `tecnicoId`, `page` y `pageSize` usan el formato de la decisión 95; después, el validador comprueba los rangos de RN-05 con las constantes de paginación de Application (Enmienda 2) (decisión 105).
