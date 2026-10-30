# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Estado del proyecto

API REST de tickets de soporte de TI (curso "Agentic Software Development .NET 10"). Existen las especificaciones funcionales y técnicas, incluido el contrato de endpoints; la solución ya está creada con los 5 proyectos (`HelpDesk.Domain`, `HelpDesk.Application`, `HelpDesk.Infrastructure` y `HelpDesk.Api` en `src/`, y `HelpDesk.Tests` en `test/`), pero todavía sin entidades, controladores ni lógica de negocio.

Especificaciones funcionales ([specs/functional/](specs/functional/)):

- [00-vision.md](specs/functional/00-vision.md): problema, roles, alcance del MVP, supuestos y stack.
- [01-requisitos-funcionales.md](specs/functional/01-requisitos-funcionales.md): RF-xx → HU-xx → CA-xx.y (criterios Dado/Cuando/Entonces) y convenciones de respuesta de la API.
- [02-reglas-negocio.md](specs/functional/02-reglas-negocio.md): RN-xx, la máquina de estados, la tabla de SLA y la matriz de permisos.
- [03-requisitos-no-funcionales.md](specs/functional/03-requisitos-no-funcionales.md): RNF-xx con su criterio de verificación.

Especificaciones técnicas ([specs/technical/](specs/technical/)):

- [01-modelo-dominio.md](specs/technical/01-modelo-dominio.md): entidades, enums, relaciones y ubicación de cada RN por capa.
- [02-arquitectura-c4.md](specs/technical/02-arquitectura-c4.md): C4 niveles 1 y 2 y capas de Clean Architecture.
- [03-contrato-api.md](specs/technical/03-contrato-api.md): contrato REST de las 13 operaciones (rutas, cuerpos, respuestas, errores y verificación). Implementa los endpoints según este contrato.
- [adr/README.md](specs/technical/adr/README.md): índice de las decisiones de arquitectura (ADR-001 a ADR-004, con enmiendas).

Auditoría y decisiones: [docs/auditoria-documentacion.md](docs/auditoria-documentacion.md). Su sección 9 registra las decisiones de la desarrolladora que aclaran las especificaciones.

Antes de implementar un RF, lee sus CA y las RN que referencia.

## Stack

- .NET 10 · ASP.NET Core Web API · OpenAPI / Swagger UI (no hay frontend; la API se prueba desde Swagger)
- Clean Architecture con cuatro proyectos: **Domain**, **Application**, **Infrastructure**, **Api**
- SQL Server + Entity Framework Core
- FluentValidation · xUnit

## Fuentes de verdad

@specs/functional/02-reglas-negocio.md
@specs/technical/01-modelo-dominio.md
@specs/trazabilidad.md

Bajo demanda (no importar): 01-requisitos-funcionales.md, 03-contrato-api.md,
ADR-001 a ADR-004, docs/auditoria-documentacion.md §9 (decisiones).

## Comandos

La solución es `HelpDesk.slnx`. Los comandos habituales son:

```powershell
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~<Clase o Método>"   # una sola prueba
dotnet test --filter "CA=CA-01.1"                             # pruebas de un CA (también "RN=RN-01")
dotnet run --project src/HelpDesk.Api                         # http://localhost:3001, Swagger UI en /swagger
dotnet ef migrations add <Nombre> --project src/HelpDesk.Infrastructure --startup-project src/HelpDesk.Api
dotnet list package --vulnerable --include-transitive         # comprobar paquetes tras agregar o actualizar
```

`dotnet ef` requiere la herramienta global `dotnet-ef`. En Windows PowerShell 5.1 no existen `&&` ni `||`.

`package.json` existe solo para `@playwright/test` (lo usa el MCP de Playwright); no es parte de la API.

El documento OpenAPI se sirve en `/openapi/v1.json` (solo en `Development`). Las pruebas usan `NetArchTest.Rules` (prueba de arquitectura de RNF-07), `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory` para las de integración) y `FakeTimeProvider`.

## Arquitectura y reglas transversales

Las reglas de cada capa están en [.claude/rules/](.claude/rules/) (`domain.md`, `application.md`, `infrastructure.md`, `api.md`, `tests.md`) y se cargan solo al trabajar en los archivos de esa capa; no las repitas aquí. Lo que sigue es transversal.

- **Referencias entre proyectos (RNF-07):** Domain no referencia proyectos ni paquetes; Application → Domain; Infrastructure → Application y Domain; Api → Application e Infrastructure (esta última solo para registrar dependencias en `Program.cs`). `HelpDesk.Tests` referencia los cuatro. `System.Security.Cryptography.Pkcs` está fijado en Infrastructure para evitar la versión vulnerable que trae `Microsoft.Data.SqlClient`; no lo quites.
- **Ubicación de las reglas** ([01-modelo-dominio.md §4](specs/technical/01-modelo-dominio.md#4-ubicación-de-las-reglas-de-negocio)): el formato de la entrada se valida en **Application** con FluentValidation (`400` con detalle). **Domain** protege las invariantes de RN-01 y RN-09 (`InvarianteVioladaException` → `500`) y las reglas de estado (`TransicionInvalidaException` → `409`). Lo que requiere consultar la base de datos o conocer al actor (rol, dueño) va en **Application** (`403` o `400`). Cada RN debe tener al menos una prueba.
- **Identidad sin JWT (S-02):** el usuario llega en el encabezado `X-Usuario-Id`, que debe ser un entero positivo dentro del rango de `int`. Si falta, no es un Id válido o el usuario no existe se responde `401`. Si el rol o el dueño no corresponden, `403` (ver la matriz de permisos de [02-reglas-negocio.md](specs/functional/02-reglas-negocio.md#matriz-de-permisos)). JWT queda fuera del MVP.
- **Orden de evaluación de errores:** `401` → `404` → `403` → `400` → `409`. El `404` aplica solo al recurso de la ruta; un dato inválido del cuerpo (p. ej., un técnico inexistente) es `400`. Los errores usan Problem Details con `type` y `title` por defecto de ASP.NET Core y el campo `codigo` del [catálogo de ADR-004](specs/technical/adr/ADR-004-problemdetails-errores.md#enmienda-3--28102026).
- Los textos se recortan antes de validar; si quedan vacíos se responde `400` (RN-01, RN-09).
- Los usuarios (Solicitante, Técnico, Supervisor) se cargan como datos semilla: 5 solicitantes, 4 técnicos (uno inactivo) y 1 supervisor ([tabla](specs/technical/01-modelo-dominio.md#8-datos-semilla)). No hay endpoint de registro (S-01). Las migraciones de EF Core son la fuente del modelo físico; el desarrollo y las pruebas de integración usan LocalDB.
- Las fechas se guardan en UTC y el SLA se cuenta en horas calendario 24/7 (S-03, S-04).
- La máquina de estados (RN-08), el SLA (RN-10 a RN-12), el código de ticket (RN-03), la paginación (RN-05) y la visibilidad (RN-04) están en los archivos importados en "Fuentes de verdad". El estado del SLA **se calcula, no se almacena** (RN-11).
- **Listados:** la respuesta tiene `items`, `page`, `pageSize` y `totalItems`, ordenada por `fechaCreacion` y luego `id`, ambos descendentes.
- **Respuestas:** cada acción sobre un ticket es una operación propia. Crear → `201` con el ticket y `Location`; las acciones → `200` con el ticket actualizado (detalle); las consultas → `200` (ver convenciones en [01-requisitos-funcionales.md](specs/functional/01-requisitos-funcionales.md)).
- **Fuera del MVP:** JWT, Docker, eliminar tickets, historial de cambios y control de concurrencia ([00-vision.md §4](specs/functional/00-vision.md#4-alcance)).

## Trazabilidad y evidencias

- Cada RF debe poder seguirse hasta su historia, endpoint, prueba y commit. Usa los IDs (RF/HU/CA/RN) en los mensajes de commit y en las pruebas, con `[Trait("CA", "CA-xx.y")]` y `[Trait("RN", "RN-xx")]` (RNF-10). La prueba de arquitectura de RNF-07 es obligatoria.
- [docs/evidencias/bitacora.md](docs/evidencias/bitacora.md) registra las interacciones con la IA (prompt, resultado, decisiones ✅/✏️/❌ y commit). Cada entrada sigue la plantilla `E-NNN` que está al inicio de la bitácora.

## Git

- Rama principal `main`; ramas de funcionalidad `feat/nombre-corto`; nunca hacer commit directo a `main`. Los cambios llegan a `main` mediante pull request.
