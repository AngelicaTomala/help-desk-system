# ADR-002 · SQL Server con Entity Framework Core

| Campo  | Valor      |
| ------ | ---------- |
| Estado | Aceptado   |
| Fecha  | 27/10/2026 |

## Contexto

Los tickets necesitan persistencia relacional con integridad: código consecutivo y único (RN-03), relaciones Ticket–Usuario–Comentario y fechas en UTC (S-04). Los listados deben filtrarse, ordenarse y paginarse en la base de datos (RN-05, CA-02.3, CA-02.4, RNF-06). El acceso a datos debe ser parametrizado y sin secretos en el repositorio (RNF-02). El MVP no usa Docker, por lo que la base debe correr en local.

## Decisión

Usar **SQL Server** (LocalDB o instancia local en desarrollo) con **EF Core** en el proyecto Infrastructure, que implementa las interfaces de persistencia definidas en Application (ADR-001).

- Consultas LINQ sobre `IQueryable` para que filtros, `ORDER BY FechaCreacion DESC`, `OFFSET/FETCH` y el conteo se traduzcan a SQL (RNF-06).
- Índices sobre `FechaCreacion`, `Estado`, `TecnicoId`, `SolicitanteId` e índice único sobre `Codigo` (RN-03, RNF-06).
- Una SEQUENCE de SQL Server alimenta `IGeneradorCodigoTicket` para el consecutivo `HD-000001` (RN-03).
- Columnas `datetime2` con valores UTC (S-04); enums guardados como texto (RNF-09).
- Migraciones de EF Core y datos semilla de usuarios (S-01). Cadena de conexión en user-secrets (RNF-02).

## Consecuencias

**Positivas**

- Consultas parametrizadas por defecto (RNF-02) y SQL verificable con `ToQueryString()` (RNF-06).
- La SEQUENCE garantiza consecutivos sin duplicados ante peticiones concurrentes (RN-03).
- El esquema evoluciona con migraciones versionadas en el repositorio.

**Negativas**

- Requiere SQL Server instalado en cada equipo de desarrollo (no hay Docker).
- Riesgo de evaluación en memoria o consultas N+1 si se materializa antes de paginar; se mitiga revisando el SQL generado.
- Las pruebas de integración necesitan una base real; el proveedor InMemory no aplica índices únicos ni SEQUENCE.

## Alternativas consideradas

| Alternativa                 | Motivo de descarte                                                                                                         |
| --------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Dapper con SQL escrito a mano | Sin migraciones ni configuración de relaciones; más riesgo de concatenar SQL (RNF-02) y más código repetido para paginar.    |
| PostgreSQL o SQLite con EF Core | El stack del proyecto fija SQL Server; SQLite no tiene SEQUENCE y su manejo de fechas difiere, lo que complica RN-03 y S-04. |
| EF Core InMemory como base principal | No es relacional: no aplica restricciones únicas ni traduce a SQL, así que no permite verificar RNF-06 ni RN-03.         |

## Requisitos relacionados

RNF-02, RNF-06, RNF-09 · RN-03, RN-05 · CA-02.3, CA-02.4 · S-01, S-04

## Enmienda · 28/10/2026

Origen: decisiones 13 y 16 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos H-45 y H-38).

- Donde dice "consecutivo" o "consecutivos", léase **único y creciente**: se aceptan huecos en la numeración del código (RN-03). La SEQUENCE garantiza códigos sin duplicados, no sin huecos.
- El ticket guarda también `FechaActualizacion` (UTC, `datetime2`). El historial de cambios queda fuera del MVP.

## Enmienda 2 · 28/10/2026

Origen: decisiones 32, 33, 35, 36 y 37 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos H-12, H-29, H-42, H-49 y H-15).

- **Modelo físico:** las migraciones de EF Core son la fuente del modelo físico (tablas, columnas, índices y SEQUENCE). Los usuarios semilla están en [01-modelo-dominio.md §8](../01-modelo-dominio.md#8-datos-semilla).
- **Índices:** además de los de la Decisión, `Usuario.Email` tiene índice único.
- **Código del ticket:** la SEQUENCE inicia en 1 (`HD-000001`). El límite `HD-999999` es una limitación conocida del MVP.
- **Entorno:** el desarrollo y las pruebas de integración usan LocalDB.
- **Justificación adicional:** la desarrolladora tiene más de 10 años de experiencia desarrollando software con .NET y con SQL Server.
