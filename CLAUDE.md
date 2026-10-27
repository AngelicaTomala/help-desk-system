# CLAUDE.md

Este archivo contiene instrucciones específicas para que Claude Code (la herramienta de terminal de Anthropic) entienda cómo debe trabajar dentro de ese repositorio.

## Estado del proyecto

API REST de tickets de soporte de TI (curso "Agentic Software Development .NET 10"). Por ahora solo existen las especificaciones funcionales: `src/` y `test/` están vacíos y `specs/technical/` aún no tiene contenido. La fuente de verdad es `specs/functional/`:

- `00-vision.md`: problema, roles, alcance del MVP, supuestos y stack.
- `01-requisitos-funcionales.md`: RF-xx → HU-xx → CA-xx.y (criterios Dado/Cuando/Entonces).
- `02-reglas-negocio.md`: RN-xx, la máquina de estados y la tabla de SLA.

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

- Las reglas de negocio van en **Domain**. Si dependen de datos externos (p. ej., que un técnico exista y esté activo), van en **Application**. Cada RN debe tener al menos una prueba.
- **Identidad sin JWT (S-02):** el usuario llega en el encabezado `X-Usuario-Id`. Si el usuario no existe se responde `401`. Si el rol o el dueño no corresponden, `403`. JWT queda fuera del MVP.
- Los usuarios (Solicitante, Técnico, Supervisor) se cargan como datos semilla; no hay endpoint de registro (S-01).
- Las fechas se guardan en UTC y el SLA se cuenta en horas calendario 24/7 (S-03, S-04).
- **Máquina de estados (RN-08):** `Abierto → Asignado → En progreso → Resuelto → Cerrado`. Además, se puede reasignar desde Asignado o En progreso (vuelve a Asignado) y reabrir desde Resuelto (vuelve a En progreso). `Cerrado` es final. Cualquier transición no listada responde `409`.
- Resolver exige un comentario de solución. Reabrir exige un motivo y borra la fecha de resolución (RN-09).
- **SLA (RN-10):** `FechaVencimiento = FechaCreacion + HorasSla(Prioridad)`, con Crítica/Alta/Media/Baja = 2/4/8/24 h. Al cambiar la prioridad (solo el supervisor, y solo si el ticket no está Resuelto ni Cerrado) se recalcula desde la fecha de creación original. El estado del SLA (`EnTiempo`/`Vencido`/`Cumplido`/`Incumplido`) **se calcula, no se almacena** (RN-11).
- Código de ticket: `HD-` + 6 dígitos consecutivos (`HD-000001`).
- Listados: paginados con `page`/`pageSize` (máximo 50), ordenados por fecha de creación descendente, con el total de registros.
- Visibilidad (RN-04): el solicitante ve solo sus tickets, el técnico solo los asignados a él y el supervisor ve todos.

## Trazabilidad y evidencias

- Cada RF debe poder seguirse hasta su historia, endpoint, prueba y commit. Usa los IDs (RF/HU/CA/RN) en los nombres o atributos de las pruebas y en los mensajes de commit.
- `docs/evidencias/bitacora.md` registra las interacciones con la IA (prompt, resultado, decisiones ✅/✏️/❌ y commit). Cada entrada sigue la plantilla `E-NNN`.

## Git

- Rama principal `main`; ramas de funcionalidad `feat/nombre-corto`; nunca hacer commit directo a `main`.
- La carpeta todavía no está inicializada como repositorio git.
