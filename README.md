# help-desk-system

API REST para registrar, asignar, priorizar y consultar tickets de soporte de TI, con control de SLA según la prioridad. Proyecto integrador del curso "Agentic Software Development .NET 10".

## Stack

- .NET 10 · ASP.NET Core Web API · OpenAPI / Swagger UI (no hay frontend; la API se prueba desde Swagger UI)
- Clean Architecture: Domain, Application, Infrastructure, Api
- SQL Server + Entity Framework Core
- FluentValidation · xUnit

## Requisitos para ejecutar

- SDK de .NET 10.
- SQL Server LocalDB, sin Docker: lo usan el desarrollo y las pruebas de integración ([ADR-002](specs/technical/adr/ADR-002-sqlserver-efcore.md)).
- Usuarios cargados como datos semilla: 5 solicitantes, 4 técnicos (uno inactivo) y 1 supervisor ([tabla](specs/technical/01-modelo-dominio.md#8-datos-semilla)). La identidad se envía en el encabezado `X-Usuario-Id`.

Todavía no hay una solución `.sln`; los comandos de compilación y pruebas están en [CLAUDE.md](CLAUDE.md#comandos).

## Estructura

| Carpeta                                     | Contenido                                                  |
| ------------------------------------------- | ---------------------------------------------------------- |
| [specs/functional/](specs/functional/)      | Visión, requisitos funcionales, reglas de negocio y RNF    |
| [specs/technical/](specs/technical/)        | Modelo de dominio, arquitectura C4 y ADR                   |
| [docs/](docs/)                              | Auditoría de la documentación y bitácora de evidencias     |
| `src/`, `test/`                             | Código y pruebas (vacías hasta crear la solución)          |

## Documentación

- [00 · Visión](specs/functional/00-vision.md)
- [01 · Requisitos funcionales](specs/functional/01-requisitos-funcionales.md)
- [02 · Reglas de negocio](specs/functional/02-reglas-negocio.md)
- [03 · Requisitos no funcionales](specs/functional/03-requisitos-no-funcionales.md)
- [01 · Modelo de dominio](specs/technical/01-modelo-dominio.md)
- [02 · Arquitectura C4](specs/technical/02-arquitectura-c4.md)
- [Índice de ADR](specs/technical/adr/README.md)
- [Auditoría de la documentación](docs/auditoria-documentacion.md)
- [Bitácora de evidencias](docs/evidencias/bitacora.md)
