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

## Enmienda 3 · 30/10/2026

Origen: decisiones 87, 88 y 89 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (hallazgos I-01 e I-02 de la revisión de Infrastructure, y la revisión de ese informe).

- **Claves foráneas:** `Ticket.SolicitanteId`, `Ticket.TecnicoId` y `Comentario.AutorId` usan `Restrict` al borrar; solo `Comentario.TicketId` usa cascada. Así no hay varios caminos de cascada hasta `Comentario`, que SQL Server no admite. El MVP no borra usuarios (no hay ningún RF para hacerlo; son datos semilla, S-01) ni tickets (decisión 18).
- **Enums:** las columnas `Ticket.Estado`, `Ticket.Prioridad`, `Ticket.Categoria`, `Usuario.Rol` y `Comentario.Tipo` son `nvarchar(20)`. Al tener una longitud acotada, admiten el índice sobre `Estado` (RNF-06).
- **Datos semilla:** los 10 usuarios de [01-modelo-dominio.md §8](../01-modelo-dominio.md#8-datos-semilla) tienen los Id 1 a 10, en el orden de la tabla.
- **Alcance:** precisa la Enmienda 2 y la decisión 32. Las migraciones de EF Core siguen siendo la fuente del modelo físico; estas decisiones fijan solo lo que la convención de EF Core no resuelve o lo que afecta a otras capas.

## Enmienda 4 · 30/10/2026

Origen: decisiones 90 y 91 de [auditoria-documentacion.md §9](../../../docs/auditoria-documentacion.md#9-decisiones-tomadas) (implementación de la capa Infrastructure).

- **Límite del código:** la SEQUENCE `CodigoTicket` tiene `MAXVALUE` 999999 y no es cíclica, y `FormatoCodigoTicket` rechaza los números fuera de 1 a 999999. Al superar el máximo, la creación del ticket debe fallar con `500` `ERROR_INTERNO` (decisión 41) sin crear el ticket. Completa la Enmienda 2 y la decisión 35. La prueba `GenerarAsync_SecuenciaEnMaximo_DevuelveHD999999YLuegoFalla` comprueba que la SEQUENCE entrega `HD-999999` y que la llamada siguiente falla; el `500` y que no se cree el ticket se comprobarán al implementar RF-01, cuando existan el handler de crear y la Api.
- **Base de las pruebas de integración:** precisa lo que dice la Enmienda 2 sobre el entorno de las pruebas de integración. Usan una base propia en LocalDB, distinta de `HelpDesk` (por convención, `HelpDesk_Pruebas`), que se recrea con la migración al empezar cada ejecución y se conserva al terminar. Su cadena de conexión se lee de los user-secrets del proyecto de pruebas, con la clave `ConnectionStrings:HelpDeskPruebas`, o de la variable de entorno `ConnectionStrings__HelpDeskPruebas` (RNF-02). Si la cadena apunta a la base de desarrollo `HelpDesk` o no nombra ninguna base, las pruebas fallan antes de conectarse.
