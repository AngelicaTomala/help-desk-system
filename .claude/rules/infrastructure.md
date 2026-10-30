---
paths:
  - "src/HelpDesk.Infrastructure/**"
---
# Reglas de la capa Infrastructure

- Configura cada entidad en una clase `IEntityTypeConfiguration<T>` propia (convención del proyecto, E-015) (02-arquitectura-c4.md §3: "configuraciones de entidades").
- Persiste los enums como texto (01-modelo-dominio.md §2, ADR-002).
- Usa columnas `datetime2` con valores UTC y marca las fechas como `DateTimeKind.Utc` al leerlas, para serializarlas con `Z` (ADR-002, decisión 60, 03-contrato-api.md §1.6).
- Genera el código del ticket con una SEQUENCE de SQL Server que inicia en 1 (`HD-000001`) e implementa `IGeneradorCodigoTicket` (RN-03, ADR-002).
- Nunca concatenes ni interpoles SQL en `FromSqlRaw` o `ExecuteSqlRaw` (RNF-02).
- No guardes la cadena de conexión en `appsettings*.json` versionados; léela de user-secrets o de variables de entorno (RNF-02).
- Crea índices sobre `FechaCreacion`, `Estado`, `TecnicoId` y `SolicitanteId`, e índices únicos sobre `Ticket.Codigo` y `Usuario.Email` (RNF-06, decisión 33).
- Filtra, ordena, cuenta y pagina en SQL Server, no en memoria (RNF-06).
