# CLAUDE.md

Este archivo contiene instrucciones específicas para que Claude Code (la herramienta de terminal de Anthropic) entienda cómo debe trabajar dentro de ese repositorio.

## Estado del proyecto

API REST de tickets de soporte de TI (curso "Agentic Software Development .NET 10"). Existen las especificaciones funcionales y técnicas; todavía no hay código ni contrato de endpoints. Las carpetas `src/` y `test/` están vacías y Git no las versiona hasta que tengan contenido; se completarán al crear la solución.

Especificaciones funcionales ([specs/functional/](specs/functional/)):

- [00-vision.md](specs/functional/00-vision.md): problema, roles, alcance del MVP, supuestos y stack.
- [01-requisitos-funcionales.md](specs/functional/01-requisitos-funcionales.md): RF-xx → HU-xx → CA-xx.y (criterios Dado/Cuando/Entonces) y convenciones de respuesta de la API.
- [02-reglas-negocio.md](specs/functional/02-reglas-negocio.md): RN-xx, la máquina de estados, la tabla de SLA y la matriz de permisos.
- [03-requisitos-no-funcionales.md](specs/functional/03-requisitos-no-funcionales.md): RNF-xx con su criterio de verificación.

Especificaciones técnicas ([specs/technical/](specs/technical/)):

- [01-modelo-dominio.md](specs/technical/01-modelo-dominio.md): entidades, enums, relaciones y ubicación de cada RN por capa.
- [02-arquitectura-c4.md](specs/technical/02-arquitectura-c4.md): C4 niveles 1 y 2 y capas de Clean Architecture.
- [adr/README.md](specs/technical/adr/README.md): índice de las decisiones de arquitectura (ADR-001 a ADR-004, con enmiendas).

Auditoría y decisiones: [docs/auditoria-documentacion.md](docs/auditoria-documentacion.md). Su sección 9 registra las decisiones de la desarrolladora que aclaran las especificaciones.

Antes de implementar un RF, lee sus CA y las RN que referencia.

## Stack

- .NET 10 · ASP.NET Core Web API · OpenAPI / Swagger UI (no hay frontend; la API se prueba desde Swagger)
- Clean Architecture con cuatro proyectos: **Domain**, **Application**, **Infrastructure**, **Api**
- SQL Server + Entity Framework Core
- FluentValidation · xUnit

## Comandos

Todavía no hay una solución `.sln`. Una vez creada, los comandos habituales son:

```powershell
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~<Clase o Método>"   # una sola prueba
dotnet run --project <ruta al proyecto Api>
dotnet ef migrations add <Nombre> --project <Infrastructure> --startup-project <Api>
```

`package.json` existe solo para `@playwright/test` (lo usa el MCP de Playwright); no es parte de la API.

## Arquitectura y reglas transversales

- **Ubicación de las reglas** ([01-modelo-dominio.md §4](specs/technical/01-modelo-dominio.md#4-ubicación-de-las-reglas-de-negocio)): el formato de la entrada se valida en **Application** con FluentValidation (`400` con detalle). **Domain** protege las invariantes de RN-01 y RN-09 (`InvarianteVioladaException` → `500`) y las reglas de estado (`TransicionInvalidaException` → `409`). Lo que requiere consultar la base de datos o conocer al actor (rol, dueño) va en **Application** (`403` o `400`). Cada RN debe tener al menos una prueba.
- **Identidad sin JWT (S-02):** el usuario llega en el encabezado `X-Usuario-Id`, que debe ser un entero positivo dentro del rango de `int`. Si falta, no es un Id válido o el usuario no existe se responde `401`. Si el rol o el dueño no corresponden, `403` (ver la matriz de permisos de [02-reglas-negocio.md](specs/functional/02-reglas-negocio.md#matriz-de-permisos)). JWT queda fuera del MVP.
- **Orden de evaluación de errores:** `401` → `404` → `403` → `400` → `409`. El `404` aplica solo al recurso de la ruta; un dato inválido del cuerpo (p. ej., un técnico inexistente) es `400`. Los errores usan Problem Details con `type` y `title` por defecto de ASP.NET Core y el campo `codigo` del [catálogo de ADR-004](specs/technical/adr/ADR-004-problemdetails-errores.md#enmienda-3--28102026).
- Los textos se recortan antes de validar; si quedan vacíos se responde `400` (RN-01, RN-09).
- Los usuarios (Solicitante, Técnico, Supervisor) se cargan como datos semilla: 5 solicitantes, 4 técnicos (uno inactivo) y 1 supervisor ([tabla](specs/technical/01-modelo-dominio.md#8-datos-semilla)). No hay endpoint de registro (S-01). Las migraciones de EF Core son la fuente del modelo físico; el desarrollo y las pruebas de integración usan LocalDB.
- Las fechas se guardan en UTC y el SLA se cuenta en horas calendario 24/7 (S-03, S-04).
- **Máquina de estados (RN-08):** `Abierto → Asignado → En progreso → Resuelto → Cerrado`. Además, se puede reasignar desde Asignado o En progreso (vuelve a Asignado) y reabrir desde Resuelto (vuelve a En progreso). `Cerrado` es final. Cualquier transición no listada responde `409`. En la API el estado "En progreso" es `EnProgreso`.
- Resolver y reabrir: ver RN-09 en [02-reglas-negocio.md](specs/functional/02-reglas-negocio.md).
- **SLA (RN-10):** `FechaVencimiento = FechaCreacion + HorasSla(Prioridad)`, con Crítica/Alta/Media/Baja = 2/4/8/24 h. Al cambiar la prioridad (solo el supervisor, y solo si el ticket no está Resuelto ni Cerrado) se recalcula desde la fecha de creación original. El estado del SLA (`EnTiempo`/`Vencido`/`Cumplido`/`Incumplido`) **se calcula, no se almacena** (RN-11).
- Código de ticket: `HD-` + 6 dígitos, único y creciente desde `HD-000001`; se aceptan huecos. El límite `HD-999999` es una limitación conocida del MVP.
- **Listados:** `page` por defecto 1 y `pageSize` por defecto 20, máximo 100; fuera de rango responde `400`. La respuesta tiene `items`, `page`, `pageSize` y `totalItems`, ordenada por fecha de creación descendente.
- Visibilidad (RN-04): el solicitante ve solo sus tickets, el técnico solo los asignados a él y el supervisor ve todos.
- **Respuestas:** cada acción sobre un ticket es una operación propia. Crear → `201` con el ticket y `Location`; las acciones → `200` con el ticket actualizado (detalle); las consultas → `200` (ver convenciones en [01-requisitos-funcionales.md](specs/functional/01-requisitos-funcionales.md)).
- **Fuera del MVP:** JWT, Docker, eliminar tickets, historial de cambios y control de concurrencia ([00-vision.md §4](specs/functional/00-vision.md#4-alcance)).

## Trazabilidad y evidencias

- Cada RF debe poder seguirse hasta su historia, endpoint, prueba y commit. Usa los IDs (RF/HU/CA/RN) en los mensajes de commit y en las pruebas, con `[Trait("CA", "CA-xx.y")]` y `[Trait("RN", "RN-xx")]` (RNF-10). La prueba de arquitectura de RNF-07 es obligatoria.
- [docs/evidencias/bitacora.md](docs/evidencias/bitacora.md) registra las interacciones con la IA (prompt, resultado, decisiones ✅/✏️/❌ y commit). Cada entrada sigue la plantilla `E-NNN` que está al inicio de la bitácora.

## Git

- Rama principal `main`; ramas de funcionalidad `feat/nombre-corto`; nunca hacer commit directo a `main`. Los cambios llegan a `main` mediante pull request.
