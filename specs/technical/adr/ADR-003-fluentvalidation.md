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
