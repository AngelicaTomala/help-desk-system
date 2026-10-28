# ADR-001 · Clean Architecture en cuatro proyectos

| Campo  | Valor      |
| ------ | ---------- |
| Estado | Aceptado   |
| Fecha  | 27/10/2026 |

## Contexto

El sistema concentra su valor en reglas de negocio: máquina de estados (RN-08), SLA calculado (RN-10, RN-11), permisos por rol y dueño (RN-04, RN-06, RN-12, RN-13). Estas reglas deben probarse de forma aislada y trazarse hasta su prueba (RNF-10). Además, RNF-07 exige una regla de dependencias verificable en los `.csproj` y RNF-08 exige que las reglas no vivan en los controladores y que la hora actual sea inyectable.

## Decisión

Organizar la solución en cuatro proyectos: **Domain**, **Application**, **Infrastructure** y **Api** (ver `02-arquitectura-c4.md`, sección 3).

- Domain contiene entidades, enums, `PoliticaSla` y las reglas de estado del ticket (RN-03, RN-07 a RN-13), sin dependencias externas. Una transición o cambio no permitido lanza una excepción de dominio que termina en `409`.
- Application contiene casos de uso, validadores e interfaces de persistencia (repositorios, `IGeneradorCodigoTicket`); aplica las reglas que dependen de datos o del actor (RN-04, RN-05, RN-06) y decide el `403` por rol o dueño.
- Infrastructure implementa esas interfaces con EF Core (ADR-002); Api expone HTTP, traduce excepciones a Problem Details (ADR-004) y registra dependencias.
- Las dependencias apuntan hacia Domain (inversión de dependencias).

## Consecuencias

**Positivas**

- Las reglas de Domain se prueban con xUnit sin base de datos ni HTTP, con un reloj falso (RNF-08, RNF-10).
- La regla de dependencias se comprueba revisando `ProjectReference` (RNF-07).
- Cambiar la persistencia o la forma de identidad (JWT a futuro, S-02) no afecta a Domain.

**Negativas**

- Más proyectos, interfaces y mapeos DTO ↔ entidad que en una API de un solo proyecto.
- Ceremonia alta para un MVP con tres entidades; requiere disciplina para no saltarse capas.

## Alternativas consideradas

| Alternativa                                         | Motivo de descarte                                                                                                         |
| --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Arquitectura en N capas (Controller → Service → Repository con entidades EF) | Las entidades quedan acopladas a EF Core y las reglas tienden a dispersarse en servicios; incumple RNF-07 y dificulta RNF-08. |
| Un solo proyecto con carpetas por capa              | La regla de dependencias no se puede verificar en los `.csproj` (RNF-07); el compilador no impide referencias indebidas.    |
| Vertical Slice Architecture                         | Válida, pero el stack del curso fija los cuatro proyectos y las reglas del ticket son compartidas por varios casos de uso.  |

## Requisitos relacionados

RNF-07, RNF-08, RNF-10 · RN-01 a RN-13 · S-02

## Enmienda · 28/10/2026

Origen: decisión 5 de `docs/auditoria-documentacion.md` §9 (hallazgo H-18).

"Excepción de dominio" en la sección Decisión se precisa en dos tipos:

- `TransicionInvalidaException`: transición o cambio no permitido por el estado del ticket → `409`.
- `InvarianteVioladaException`: invariante de la entidad violada (RN-01, RN-09) → `500` (ver ADR-003).
