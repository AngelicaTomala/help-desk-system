# Guion de la presentación · Help Desk System

Defensa del proyecto integrador · Agentic Software Development .NET 10 (Galaxy Training) · 31/10/2026

> Documento para revisión antes de construir el `.pptx` (Fase 3). Todo el contenido sale del repositorio en la rama `feat/rf-01` (commit `e6030c9`). Lo que no existe en el repo está marcado como **[PENDIENTE: …]** y se lista al final.

---

## Resumen de la investigación (Fase 1)

### Historial de Git

- **56 commits** (`21ae3a5` Initial commit, 26/10/2026 → `e6030c9`, 31/10/2026), incluidos 3 merges.
- **Ramas:** `main`, `feat/specs-tecnicas` (PR #1, fusionada), `feat/estructura-solucion` (PR #2, fusionada) y `feat/rf-01` (actual, 9 commits que aún no están en `main`). Sin tags.
- **Etapas del historial:**
  1. Especificaciones funcionales y técnicas (`61b123f` → `e2646c1`): visión, RF/HU/CA, RN, RNF, modelo de dominio, C4, 4 ADR, auditoría, contrato de API y trazabilidad.
  2. Decisiones de Domain (`f120971`, `b2bbf14`, `496b0e4`: decisiones 75 a 84).
  3. Solución y capas (`8db87ac` solución · `7a04a34` Domain · `98a59f4`…`8cb8f55` Infrastructure · `75c7cfd` prueba de arquitectura).
  4. RF-01 (`7f0f2b0` decisiones 92-99 · `530463f` crear · `abc4a3d` reclasificar · `5be7323` repositorios · `5027dfc` Problem Details · `6851b9c` middleware · `eebf67e` endpoints · `e6030c9` OpenAPI).
- Los commits con IA llevan `Co-Authored-By: Claude …` (por ejemplo, `7577e08`).

### Estado real del código

| Capa | Implementado |
| ---- | ------------ |
| Domain | Completo: `Ticket` (los 10 métodos del modelo), `Usuario`, `Comentario`, 6 enums, `PoliticaSla`, `TransicionInvalidaException`, `InvarianteVioladaException`. Sin referencias (RNF-07). |
| Application | `CrearTicket` y `ReclasificarTicket` (comando, validador FluentValidation y handler); interfaces `ITicketRepositorio`, `IUsuarioRepositorio`, `IUnidadDeTrabajo`, `IConsultasTicket`, `IGeneradorCodigoTicket`, `IUsuarioActual`. |
| Infrastructure | `HelpDeskDbContext`, configuraciones, migración `20261031022819_Inicial` (tablas, índices, SEQUENCE `dbo.CodigoTicket`, 10 usuarios semilla), repositorios, unidad de trabajo, `ConversorFechaUtc`. |
| Api | `TicketsController` con **2 de las 13 operaciones** del contrato; middleware `X-Usuario-Id`; `ManejadorExcepciones` (Problem Details); JSON camelCase y enums como texto; OpenAPI + Swagger UI. |

### Build y pruebas (ejecutados hoy, 31/10/2026)

- `dotnet build`: **Compilación correcta · 0 advertencias · 0 errores**.
- `dotnet test`: **472 superadas · 0 con error · 0 omitidas** (39 s).
- 213 métodos `[Fact]`/`[Theory]` por carpeta: Domain 48 · Application 35 · Infrastructure 28 · Integración (LocalDB) 22 · Api 72 · Arquitectura 8.
- `[Trait("CA")]` presentes: 24 de 45 CA. `[Trait("RN")]` presentes: 11 de 13 RN (faltan RN-04 y RN-05, que son de RF-02).

### Herramientas para la Fase 3

- Python 3.11.4 disponible; **python-pptx no está instalado** (habrá que ejecutar `pip install python-pptx`).
- **LibreOffice no está instalado**: la Fase 4 (conversión a PDF para revisar desbordes) necesita instalarlo, o la revisión se hará abriendo el `.pptx` en PowerPoint.

---

## Diapositiva 0 · Portada

**Título:** Help Desk System

**Subtítulo:** API REST de tickets de soporte de TI con control de SLA · .NET 10 · Clean Architecture · desarrollo asistido por IA

**Datos:**

- Angélica Tomalá Torres
- Proyecto integrador · Agentic Software Development .NET 10 · Galaxy Training
- 31/10/2026

**Visual:** fondo azul oscuro, título grande en blanco, una franja con el color de acento.

**Notas del orador:**
Buenos días. Soy Angélica Tomalá Torres y voy a presentar mi proyecto integrador del curso Agentic Software Development con .NET 10: Help Desk System, una API REST para gestionar tickets de soporte de TI con control de SLA. Durante los próximos minutos les voy a contar qué problema resuelve, cómo lo especifiqué antes de escribir código, cómo está construida la solución y, sobre todo, cómo usé Claude Code como asistente sin perder el control de las decisiones. Toda la evidencia que voy a mostrar está en el repositorio: commits, bitácora de interacciones con la IA, especificaciones y pruebas. Al final haré una demostración en vivo desde Swagger UI y les mostraré el resultado real de la compilación y de las pruebas.

---

## Diapositiva 1 · El problema: soporte técnico sin sistema

**Viñetas** (fuente: `specs/functional/00-vision.md` §1):

- Las solicitudes llegan por correo, llamadas o mensajes sueltos.
- Solicitudes que se pierden o se atienden dos veces.
- Sin prioridad: un problema crítico espera igual que uno menor.
- No se sabe quién atiende qué ni en qué estado está cada solicitud.
- Ninguna medición del cumplimiento de tiempos de respuesta (SLA).

**Visual:** cuatro íconos o tarjetas (canales dispersos → pérdida, prioridad, trazabilidad, métricas).

**Notas del orador:**
El punto de partida es una situación muy común en las áreas de TI. Las solicitudes llegan por cualquier canal: un correo, una llamada, un mensaje. Eso provoca cuatro problemas concretos que documenté en la visión del producto. Primero, solicitudes que se pierden o que dos técnicos atienden a la vez. Segundo, no hay prioridad: un sistema de nómina caído espera lo mismo que un monitor sin imagen. Tercero, no hay trazabilidad: nadie puede decir quién está atendiendo qué ni en qué estado está. Y cuarto, no hay métricas: no se puede saber si se cumplen los tiempos de respuesta comprometidos. El objetivo del proyecto es centralizar ese ciclo de vida en una API: registro, asignación a un técnico, seguimiento por estados y control del SLA según la prioridad.

---

## Diapositiva 2 · El valor del producto: para quién y qué resuelve

**Viñetas** (fuente: `00-vision.md` §2-§3, `02-reglas-negocio.md`):

- **Solicitante:** crea tickets, consulta los suyos, cierra o reabre un ticket resuelto.
- **Técnico:** ve sus tickets asignados, inicia la atención, resuelve y reclasifica.
- **Supervisor:** ve todo, asigna y reasigna, cambia la prioridad y ve el reporte de vencidos.
- Ciclo de vida controlado: `Abierto → Asignado → EnProgreso → Resuelto → Cerrado` (con reapertura).
- SLA automático por prioridad: Crítica 2 h · Alta 4 h · Media 8 h · Baja 24 h.

**Visual:** tres columnas por rol y debajo el diagrama de estados de RN-08 con formas nativas (5 estados y flechas).

**Notas del orador:**
El sistema tiene tres roles. El solicitante es el colaborador que reporta un problema; el técnico es quien lo atiende; y el supervisor coordina el área. Cada operación tiene una matriz de permisos explícita: por ejemplo, solo el solicitante crea tickets y solo el técnico asignado puede resolverlos. El valor concreto está en dos reglas. La primera es la máquina de estados: un ticket nace Abierto, el supervisor lo asigna, el técnico inicia la atención y lo resuelve, y el solicitante lo cierra o lo reabre. Cualquier otra transición responde 409. La segunda es el SLA: el vencimiento se calcula automáticamente desde la prioridad, en horas calendario y en UTC, y el estado del SLA se calcula en cada consulta, no se guarda. Así el supervisor puede ver qué tickets están vencidos sin depender de nadie.

---

## Diapositiva 3 · Especificaciones: requisitos funcionales

**Tabla** (fuente: `01-requisitos-funcionales.md`, Resumen):

| RF | Nombre | HU | CA | Prioridad |
| -- | ------ | -- | -- | --------- |
| RF-01 | Registrar y reclasificar el ticket | HU-01, HU-09 | 10 | Obligatorio |
| RF-02 | Consultar, filtrar y paginar tickets | HU-02, HU-03, HU-13 | 8 | Obligatorio |
| RF-03 | Asignar y reasignar técnico | HU-04, HU-11, HU-14 | 8 | Obligatorio |
| RF-04 | Cambiar estado del ticket | HU-05, HU-06, HU-12 | 9 | Obligatorio |
| RF-05 | Calcular y controlar el SLA | HU-07, HU-10 | 7 | Obligatorio |
| RF-06 | Reporte de tickets vencidos | HU-08 | 3 | Deseable |

**Franja inferior:** 6 RF · 14 HU · 45 CA (Dado/Cuando/Entonces) · 13 RN · 10 RNF · 13 operaciones REST · 99 decisiones registradas en la auditoría.

**Notas del orador:**
Antes de escribir una sola línea de código, el proyecto pasó por una fase completa de especificación. Hay seis requisitos funcionales, cinco obligatorios y uno deseable, el reporte de vencidos. Cada RF se descompone en historias de usuario y cada historia en criterios de aceptación con formato Dado, Cuando, Entonces, con el código HTTP esperado. En total son 14 historias y 45 criterios. Además hay 13 reglas de negocio, 10 requisitos no funcionales, un modelo de dominio, diagramas C4, cuatro ADR y un contrato de API con 13 operaciones. Todo esto pasó por una auditoría de documentación que empezó con veredicto "no lista", con 55 hallazgos, y terminó en "lista" después de registrar decisiones. Hoy la auditoría tiene 99 decisiones numeradas, y el código las cita en comentarios y pruebas.

---

## Diapositiva 4 · Matriz de trazabilidad (RF-01, implementado)

**Tabla** (fuentes: `specs/trazabilidad.md`, `[Trait("CA")]` en `test/`, `git log`):

| RF | HU | CA | Endpoint | Clase que lo implementa | Test que lo valida | Commit |
| -- | -- | -- | -------- | ----------------------- | ------------------ | ------ |
| RF-01 | HU-01 | CA-01.1 | `POST /tickets` | `CrearTicketHandler`, `Ticket.Crear` | `CrearTicketEndpointTests`, `CrearTicketHandlerTests` | `530463f`, `eebf67e` |
| RF-01 | HU-01 | CA-01.3 | `POST /tickets` | `CrearTicketValidator` | `CrearTicketValidatorTests` (5), `CrearTicketEndpointTests` (4) | `530463f` |
| RF-01 | HU-01 | CA-01.5 | `POST /tickets` | `MiddlewareUsuarioActual` | `CrearTicketEndpointTests` | `6851b9c` |
| RF-01 | HU-01 | CA-01.10 | `POST /tickets` | `CrearTicketHandler` (403) | `CrearTicketEndpointTests`, `CrearTicketHandlerTests` | `530463f` |
| RF-01 | HU-09 | CA-01.6 | `POST /tickets/{id}/reclasificar` | `ReclasificarTicketHandler`, `Ticket.Reclasificar` | `ReclasificarTicketEndpointTests`, `TicketRepositorioIntegracionTests` | `abc4a3d`, `eebf67e` |
| RF-01 | HU-09 | CA-01.7 | `POST /tickets/{id}/reclasificar` | `Ticket.Reclasificar` (409) | `TicketReclasificarCambiarPrioridadTests`, `ReclasificarTicketEndpointTests` | `7a04a34`, `abc4a3d` |

**Pie:** los CU vienen de `specs/casos-de-uso.md` (CU-01 crear ticket, CU-02 reclasificar ticket, commit `353a0b2`); la matriz completa con los nombres de cada test está en `specs/trazabilidad.md`.

**Notas del orador:**
La trazabilidad es un requisito no funcional del proyecto, el RNF-10. Cada prueba lleva un atributo Trait con el criterio de aceptación y la regla de negocio que valida, así que puedo ejecutar, por ejemplo, `dotnet test --filter "CA=CA-01.3"` y ver solo las pruebas de ese criterio. En esta tabla muestro RF-01, que es el requisito implementado de punta a punta. Por ejemplo, CA-01.3, que exige que el título tenga entre 5 y 150 caracteres, se implementa en el validador de FluentValidation y se verifica en dos niveles: cinco pruebas unitarias del validador y cuatro pruebas de extremo a extremo contra la API. Cada fila llega hasta el commit que la introdujo. Los 10 criterios de RF-01 tienen al menos una prueba, y cada mensaje de commit lleva el identificador del requisito.

---

## Diapositiva 5 · Cobertura de la trazabilidad por RF (estado real)

**Tabla** (fuente: conteo de `[Trait("CA")]` y endpoints de `TicketsController`):

| RF | CA con prueba | Endpoints implementados | Estado |
| -- | ------------- | ----------------------- | ------ |
| RF-01 | 10 / 10 | 2 / 2 | Implementado de punta a punta |
| RF-02 | 1 / 8 (CA-02.1, consulta de Infrastructure) | 0 / 3 | Pendiente en Application y Api |
| RF-03 | 3 / 8 (reglas de Domain) | 0 / 3 | Domain listo |
| RF-04 | 5 / 9 (reglas de Domain) | 0 / 4 | Domain listo |
| RF-05 | 5 / 7 (SLA en Domain) | 0 / 1 | Domain listo |
| RF-06 | 0 / 3 | 0 / 1 | Pendiente |

**Pie:** 24 de 45 CA y 11 de 13 RN con prueba etiquetada · prueba de arquitectura RNF-07 obligatoria y en verde.

**Visual:** barra de progreso por RF (verde = cubierto, gris = pendiente).

**Notas del orador:**
Quiero ser transparente con el estado real del proyecto. La capa de dominio está completa: los diez métodos del ticket, incluida la máquina de estados con sus 23 transiciones inválidas, el cálculo de SLA y la reasignación, ya tienen pruebas. Por eso RF-03, RF-04 y RF-05 ya tienen criterios cubiertos aunque todavía no tengan endpoint. Lo que está implementado de punta a punta, desde HTTP hasta SQL Server, es RF-01: crear y reclasificar tickets. Las otras 11 operaciones del contrato están especificadas, con sus errores y ejemplos, pero falta construir sus handlers y controladores. En total, 24 de los 45 criterios y 11 de las 13 reglas ya tienen una prueba etiquetada. Las dos reglas que faltan, RN-04 y RN-05, son de visibilidad y paginación, y llegarán con RF-02.

---

## Diapositiva 6 · Arquitectura: Clean Architecture

**Visual (formas nativas):** cuatro rectángulos concéntricos o en columnas, con flechas que apuntan hacia dentro:

- **Api** (exterior): `TicketsController`, `MiddlewareUsuarioActual`, `ManejadorExcepciones`, OpenAPI.
- **Infrastructure:** `HelpDeskDbContext`, migraciones, `TicketRepositorio`, `GeneradorCodigoTicket` (SEQUENCE).
- **Application:** handlers, validadores FluentValidation, interfaces (`ITicketRepositorio`, `IGeneradorCodigoTicket`…).
- **Domain** (centro): `Ticket`, `Usuario`, `Comentario`, enums, `PoliticaSla`, excepciones.

Flechas: Api → Application · Api → Infrastructure (solo en `Program.cs`) · Infrastructure → Application → Domain. Etiqueta: "Domain no referencia ningún proyecto ni paquete".

**Viñetas laterales:**

- Regla verificada por 8 pruebas de arquitectura (`NetArchTest.Rules` + lectura de los `.csproj`).
- Errores por capa: Application `400`/`403`/`404` · Domain `409` · Api traduce a Problem Details.

**Notas del orador:**
La solución sigue Clean Architecture en cuatro proyectos. En el centro está Domain, que contiene el ticket con su máquina de estados y el cálculo del SLA, y que no referencia ningún proyecto ni paquete. Application contiene los casos de uso: valida la entrada con FluentValidation, comprueba permisos y define las interfaces de persistencia. Infrastructure implementa esas interfaces con Entity Framework Core y SQL Server. Y la Api solo traduce HTTP: recibe la petición, llama al handler y convierte las excepciones a Problem Details. La regla de dependencias no queda solo en un diagrama: hay ocho pruebas de arquitectura que fallan si, por ejemplo, Domain llega a depender de Entity Framework o si un controlador usa Infrastructure directamente. La única excepción permitida es `Program.cs`, que registra las dependencias.

---

## Diapositiva 7 · Stack tecnológico

**Viñetas** (fuente: `.csproj`, `README.md`):

- **.NET 10** (SDK 10.0.301) · ASP.NET Core Web API con controladores · solución `HelpDesk.slnx`.
- **SQL Server LocalDB** + **EF Core 10.0.14** · SEQUENCE `dbo.CodigoTicket` para `HD-000001`.
- **FluentValidation 12.2.0** · Problem Details nativo de ASP.NET Core (RFC 9457).
- **OpenAPI** (`Microsoft.AspNetCore.OpenApi` 10.0.14) + **Swagger UI** (Swashbuckle 10.3.1).
- **Pruebas:** xUnit · `Mvc.Testing` (`WebApplicationFactory`) · `FakeTimeProvider` · `NetArchTest.Rules` 1.3.2.
- **Sin Docker:** LocalDB con user-secrets para las cadenas de conexión (RNF-02).

**Visual:** logos o etiquetas por capa (no usar logos de marca si no se dispone de ellos; etiquetas de texto).

**Notas del orador:**
El stack es el que fija el curso. .NET 10 con ASP.NET Core Web API y controladores. La base de datos es SQL Server, en desarrollo LocalDB, con Entity Framework Core 10. El código del ticket, HD seguido de seis dígitos, sale de una SEQUENCE de SQL Server, lo que garantiza que no haya duplicados aunque se creen tickets a la vez. La validación usa FluentValidation y los errores usan Problem Details nativo, sin librerías externas. La documentación es OpenAPI con Swagger UI. Para las pruebas uso xUnit, WebApplicationFactory para las pruebas de extremo a extremo, FakeTimeProvider para controlar el reloj en las pruebas de SLA y NetArchTest para la regla de dependencias. No hay Docker: las cadenas de conexión se guardan en user-secrets, nunca en el repositorio. Un detalle: al crear los proyectos, la IA detectó dos paquetes transitivos vulnerables y los fijó a versiones corregidas.

---

## Diapositiva 8 · Decisiones de arquitectura (ADR)

**Tabla** (fuente: `specs/technical/adr/ADR-001` a `ADR-004`):

| ADR | Decisión | Alternativas descartadas | Justificación |
| --- | -------- | ------------------------ | ------------- |
| 001 | Clean Architecture en 4 proyectos | N capas con entidades EF · un solo proyecto con carpetas · Vertical Slice | La regla de dependencias se verifica en los `.csproj` (RNF-07); reglas en Domain (RNF-08) |
| 002 | SQL Server + EF Core | Dapper · PostgreSQL/SQLite · EF InMemory | SEQUENCE e índice único (RN-03); paginación en SQL (RNF-06); experiencia de la desarrolladora |
| 003 | FluentValidation en Application + invariantes en Domain | DataAnnotations · todo en Domain · validar solo en Application | `400` con detalle por campo (RNF-05) y un ticket inválido nunca puede existir (RNF-08) |
| 004 | Problem Details nativo + campo `codigo` | Formato propio · `try/catch` por controlador · `Result<T>` | Estándar RFC 9457 (RNF-03); códigos uniformes (RNF-04); 12 valores de `codigo` |

**Pie:** los ADR aceptados no se reescriben: los cambios se agregan como enmiendas fechadas.

**Notas del orador:**
Las cuatro decisiones de arquitectura están documentadas con el formato de Michael Nygard, y cada una descarta al menos dos alternativas con un motivo ligado a un requisito. Destaco dos. En el ADR-002 descarté EF Core InMemory para las pruebas, porque no aplica restricciones únicas ni SEQUENCE; por eso las pruebas de integración corren contra LocalDB real, y eso reveló comportamientos que un modelo en memoria nunca habría mostrado. En el ADR-003 la IA propuso validar solo en Application; yo lo corregí para que Domain también proteja las invariantes, porque un ticket con un título vacío no debe poder existir, ni siquiera en una prueba. Ese cambio lo muestro más adelante como diff. Una regla importante: un ADR aceptado no se reescribe; los cambios se agregan como enmiendas con fecha, para conservar el historial.

---

## Diapositiva 9 · Cómo usé Claude Code en el flujo de trabajo

**Visual:** flujo horizontal de 5 pasos con formas nativas:
Especificar → Auditar → Planificar (modo Plan) → Implementar con pruebas → Revisar y registrar.

**Viñetas** (fuente: `docs/evidencias/bitacora.md` E-001 a E-017, `CLAUDE.md`, `.claude/rules/`, `.mcp.json`):

- Plantilla fija en cada prompt: **Contexto / Tarea / Restricciones / Formato / Verificación**.
- Memoria del proyecto: `CLAUDE.md` con imports de las specs y reglas por capa en `.claude/rules/` (se cargan por `paths`).
- MCP de GitHub (PR, verificación de commits) y MCP de Playwright (Swagger UI).
- Modo Plan para el código: el plan se aprueba o se rechaza antes de escribir.
- Bitácora: **252 elementos revisados · 198 ✅ aceptados · 38 ✏️ modificados · 16 ❌ rechazados**.

**Notas del orador:**
Esta es la sección más importante. Claude Code no escribió el proyecto solo: participó en cada fase con un flujo controlado. Todos los prompts siguen la misma plantilla: contexto, tarea, restricciones, formato y verificación. Las restricciones más útiles fueron "no modifiques otros archivos" y "si encuentras un hueco, no lo resuelvas: señálalo y pregúntame". El agente tiene memoria del proyecto en `CLAUDE.md`, que importa las reglas de negocio y el modelo de dominio, y reglas por capa que solo se cargan cuando trabaja en esa carpeta. Para el código usé el modo Plan: el agente propone, yo apruebo o rechazo, y solo entonces implementa. Cada interacción quedó en la bitácora con su prompt, el resultado y mi decisión. En las entradas E-001 a E-017 revisé 252 elementos: acepté 198, modifiqué 38 y rechacé 16. Eso es desarrollo asistido con revisión, no vibe coding.

---

## Diapositiva 10 · Prompts clave (resumidos, reales)

**Tres tarjetas** (fuente: bitácora):

1. **E-004 · Vincular el repositorio** — "Si el remoto ya tiene commits, detente antes del push y explícame las opciones. No uses `push --force`. Si encuentras un secreto, detente." → El agente se detuvo, propuso merge/rebase/reset; elegí merge (`a69d28d`).
2. **E-009 · Contrato de API en 6 partes** — "No inventes endpoints, campos ni reglas; señala los huecos sin resolverlos; no hagas commit sin revisión." → 13 operaciones y **33 decisiones nuevas (42 a 74)** que el agente habría tomado implícitamente.
3. **E-016 · Domain en modo Plan** — "Firmas exactas del modelo, setters privados, sin `DateTime.Now`, cada prueba con `[Trait("RN")]`, no inventes reglas." → Plan **rechazado** con 4 correcciones (p. ej., 28 → 23 transiciones inválidas); luego 128 pruebas en verde.

**Notas del orador:**
Elegí tres prompts que muestran cómo dirigí al agente. El primero, al vincular el repositorio, le puse una condición de parada: si el remoto ya tenía commits, debía detenerse antes del push y no usar push force. El remoto tenía un commit creado desde la web, el agente se detuvo, me dio tres opciones y yo elegí merge para no reescribir el historial. El segundo es el contrato de API: un prompt así de grande no se podía controlar de una vez, así que lo dividí en seis partes con pausa de revisión. Gracias a la regla de "señala los huecos, no los resuelvas", salieron a la luz 33 decisiones que quedaron registradas. El tercero es la capa de dominio en modo Plan. Rechacé el primer plan porque contaba 28 transiciones inválidas cuando son 23: seis acciones por cinco estados, menos siete válidas. Revisar el plan antes del código evitó pruebas equivocadas.

---

## Diapositiva 11 · Evidencia de revisión: dos diffs reales

**Diff 1 · ✏️ Propuesta corregida** — commit `eb0f1a1` (`specs/technical/01-modelo-dominio.md`), bitácora E-008:

```diff
-| `Crear(...)`               | Valida invariantes, estado `Abierto`, sin técnico, calcula vencimiento    |
+| `Crear(...)`               | Valida las longitudes de RN-01 (excepción de dominio si se violan), estado `Abierto`, sin técnico, calcula vencimiento |
-| `Resolver(solucion, ahora)`| `EnProgreso` → `Resuelto`; fija FechaResolucion; agrega comentario `Solucion` |
+| `Resolver(solucion, ahora)`| Valida la solución (no vacía, ≤ `Comentario.TextoLongitudMaxima`; excepción de dominio); `EnProgreso` → `Resuelto`; ... |
-| RN-09 | Domain               | `Ticket.Resolver` (solución obligatoria, fija FechaResolucion) · ...
+| RN-09 | Domain + Application | `Ticket.Resolver` / `Ticket.Reabrir` (invariante: texto no vacío y ≤ `Comentario.TextoLongitudMaxima`; ...) · `ResolverTicketValidator` / ...
```

**Criterio humano:** la IA propuso "Domain no valida longitudes; solo el validador de Application" y aplicó la corrección solo a RN-01. Lo modifiqué: Domain también protege RN-01 **y RN-09**, para que un ticket inválido nunca pueda existir (RNF-08), con las mismas constantes en ambas capas.

**Diff 2 · ✅ Observación de la IA aceptada** — commit `7577e08` (`test/HelpDesk.Tests/Domain/TicketResolverReabrirTests.cs`), bitácora E-016:

```diff
     [Trait("RN", "RN-09")]
     public void Resolver_TextoEnBlancoEnEstadoNoPermitido_LanzaInvarianteViolada()
     {
-        // El texto se valida antes que el estado (decisión de la desarrolladora en el plan de Domain).
+        // El texto se valida antes que el estado (decisión 85).
         AfirmarResolverRechazado(EstadoTicket.Abierto, "");
```

**Criterio humano:** la IA señaló que el comentario no era rastreable. Acepté: la decisión ya estaba registrada como decisión 85 en la auditoría §9, y el código debe citar la fuente, no una conversación.

**Formato en el .pptx:** Consolas, líneas `+` en verde y `-` en rojo; las líneas largas se recortan con "…" para que quepan.

**Notas del orador:**
Aquí están dos diffs reales del repositorio. El primero es una propuesta que corregí. Al redactar los ADR, la IA propuso que solo Application validara las longitudes del texto. Eso permitía crear un ticket con título vacío si alguien llamaba al dominio sin pasar por el validador. Le pedí que Domain también protegiera la regla, y cuando revisé el diff vi que lo había aplicado de forma literal, solo a RN-01. El mismo principio aplicaba a RN-09: un ticket resuelto sin solución también es un estado inválido. Lo extendí. La lección es que hay que comunicar el criterio, no solo el caso concreto. El segundo diff es una observación de la IA que acepté: dos comentarios de prueba decían "decisión de la desarrolladora en el plan", algo que nadie podría rastrear. Ahora citan la decisión 85, que está registrada en la auditoría.

---

## Diapositiva 12 · Más evidencia: cuando la IA se equivocó

**Viñetas** (fuente: bitácora E-007, E-010, E-015, E-016, E-017):

- **Cobertura falsa (E-016):** `Reasignar_MismoTecnico_NoLanzaExcepcion` tenía `[Trait("RN", "RN-06")]`, aunque documenta lo contrario de la regla → se quitó; el filtro `RN=RN-06` ya no la cuenta.
- **Aviso incorrecto (E-015):** "`.claude/settings.local.json` se subiría con `git add .`" → ❌, ya estaba ignorado; comprobado con `git check-ignore`.
- **Arreglar el síntoma (E-017):** cambiar un enlace correcto para que el script no lo marcara roto → ❌, el error estaba en el script; se versionó un script probado.
- **"La sintaxis es válida" (E-007):** el agente no renderizó los diagramas Mermaid → la verificación se hace donde se lee el documento.
- **Verificación independiente (E-010):** 45/45 CA y 13/13 endpoints recontados con `Select-String` + `Compare-Object` propios.

**Código final (visual en Consolas):**

```csharp
[Fact]
[Trait("RN", "RN-07")]
public void Reasignar_MismoTecnico_NoLanzaExcepcion()
{
    // MISMO_TECNICO (RN-06) se valida en ReasignarTicketHandler (decisión 81); Domain no lo rechaza.
```

**Notas del orador:**
También quiero mostrar casos en que la IA se equivocó, porque ahí es donde la revisión humana marca la diferencia. El más interesante es una cobertura falsa: una prueba de dominio estaba etiquetada con RN-06, la regla que prohíbe reasignar al mismo técnico, pero la prueba demuestra justo lo contrario: que el dominio no lo rechaza, porque esa validación vive en el handler. Si la dejaba, la matriz de trazabilidad decía que RN-06 estaba probada cuando no lo estaba. Otro caso: el agente quiso cambiar un enlace correcto para que su propio script dejara de marcarlo como roto; lo rechacé, porque el error estaba en el script. Y aprendí que cuando el agente dice "verificado", tengo que repetir la verificación con mis propios comandos, mostrando los conteos.

---

## Diapositiva 13 · Demostración: endpoints documentados en OpenAPI

**Tabla** (fuente: `TicketsController.cs`, `03-contrato-api.md`):

| Método | Ruta | Propósito | Respuestas documentadas | Estado |
| ------ | ---- | --------- | ----------------------- | ------ |
| `POST` | `/api/v1/tickets` | Crear ticket (solicitante) | `201` · `400` · `401` · `403` · `415` · `500` | ✅ Implementado |
| `POST` | `/api/v1/tickets/{id}/reclasificar` | Reclasificar (técnico asignado o supervisor) | `200` · `400` · `401` · `403` · `404` · `409` · `415` · `500` | ✅ Implementado |
| `GET` | `/api/v1/tickets`, `/api/v1/tickets/{id}` | Listar y consultar | — | Especificado |
| `POST` | `/tickets/{id}/asignar`, `reasignar`, `iniciar-atencion`, `resolver`, `cerrar`, `reabrir`, `cambiar-prioridad` | Ciclo de vida | — | Especificado |
| `GET` | `/api/v1/tecnicos`, `/api/v1/reportes/tickets-vencidos` | Técnicos activos y reporte | — | Especificado |

**Pie:** encabezado `X-Usuario-Id` documentado en OpenAPI · enums con sus valores (RNF-09) · `GET /openapi/v1.json` y `/swagger` solo en Development.

**Espacio reservado:** recuadro 16:9 "[Captura Swagger UI]" (5,4 × 3,04 in) para insertar la captura de http://localhost:3001/swagger.

**Notas del orador:**
Esta es la superficie de la API documentada en OpenAPI. Hoy hay dos operaciones implementadas: crear un ticket y reclasificarlo. Cada una documenta todas sus respuestas posibles, incluidos los errores, porque el requisito RNF-09 lo exige, y una prueba automática comprueba que el documento OpenAPI coincide con el contrato. El encabezado X-Usuario-Id aparece en cada operación, y los campos de categoría y prioridad muestran sus valores permitidos. Las otras once operaciones, como asignar, resolver o el reporte de vencidos, están especificadas en el contrato con sus rutas, cuerpos, errores y ejemplos, y la lógica de dominio que necesitan ya está probada. Lo que falta es construir sus handlers y controladores siguiendo el mismo patrón de RF-01.

---

## Diapositiva 14 · Reglas y validaciones: request válido e inválido

**Columna izquierda · válido** (fuente: contrato §2.1, prueba `Crear_DatosValidos_Responde201ConDetalleYLocationRelativo`):

```http
POST /api/v1/tickets
X-Usuario-Id: 1
{ "titulo": "No puedo instalar el antivirus",
  "descripcion": "El instalador muestra un error de permisos al iniciar.",
  "categoria": "Software", "prioridad": "Alta" }

→ 201 Created · Location: /api/v1/tickets/{id}
  "codigo": "HD-000001", "estado": "Abierto", "tecnico": null,
  "estadoSla": "EnTiempo"   // vence 4 h después (Alta, RN-10)
```

**Columna derecha · inválido** (fuente: contrato §4.1, prueba `Crear_EjemploDelContrato_Responde400ConTresClaves`):

```http
POST /api/v1/tickets
X-Usuario-Id: 1
{ "titulo": "Red", "descripcion": "     ",
  "categoria": "Impresoras", "prioridad": "Alta" }

→ 400 Bad Request · application/problem+json
  "codigo": "VALIDACION_FALLIDA",
  "errors": { "titulo": [...], "descripcion": [...], "categoria": [...] }
```

**Franja inferior · orden de errores:** `401` → `404` → `403` → `400` → `409` (un técnico con título inválido recibe `403`, no `400`: prueba `Crear_TecnicoConTituloInvalido_Responde403YNoCrea`).

**Notas del orador:**
Aquí vemos las reglas en acción con los ejemplos del contrato, que además son pruebas automáticas. A la izquierda, una petición válida de Ana, una solicitante: responde 201, con el encabezado Location, el código HD generado por la SEQUENCE, estado Abierto, sin técnico, y el vencimiento a las cuatro horas porque la prioridad es Alta. A la derecha, una petición con tres errores: un título de tres caracteres, una descripción que queda vacía al recortar los espacios y una categoría que no existe. La API responde 400 con Problem Details y los tres errores a la vez, uno por campo, y no crea el ticket. Algo que cuidé mucho es el orden de evaluación: si un técnico intenta crear un ticket con un título inválido, recibe 403 y no 400, porque primero se comprueba quién es y después qué envía.

---

## Diapositiva 15 · Resultado real: build y pruebas

**Visual:** dos bloques grandes tipo terminal (Consolas, fondo oscuro):

```text
> dotnet build
Compilación correcta.
    0 Advertencia(s)
    0 Errores
```

```text
> dotnet test
Correctas! - Con error: 0, Superado: 472, Omitido: 0, Total: 472, Duración: 39 s
```

**Tabla pequeña · métodos de prueba por carpeta:**

| Domain | Application | Infrastructure | Integración (LocalDB) | Api (extremo a extremo) | Arquitectura |
| ------ | ----------- | -------------- | --------------------- | ----------------------- | ------------ |
| 48 | 35 | 28 | 22 | 72 | 8 |

**Pie:** 213 métodos `[Fact]`/`[Theory]` → 472 casos ejecutados · ejecutado el 31/10/2026 sobre `e6030c9`.

**Notas del orador:**
Este es el resultado real, ejecutado hoy sobre el último commit de la rama. La solución compila sin errores y sin advertencias, algo que exigí desde el primer commit. Y las 472 pruebas pasan. Son 213 métodos de prueba que, con los casos parametrizados, se convierten en 472 ejecuciones. Están repartidos por capa: las de dominio prueban la máquina de estados y el SLA sin base de datos; las de Application usan dobles de prueba; las de integración corren contra una base LocalDB separada, que se borra y se recrea en cada ejecución; las de la Api levantan la aplicación completa con WebApplicationFactory y un reloj falso; y las de arquitectura vigilan la regla de dependencias. Incluso hay una prueba que agota la SEQUENCE hasta HD-999999 y comprueba que la API responde 500 sin crear el ticket.

---

## Diapositiva 16 · Aprendizajes y retos superados

**Viñetas** (fuente: secciones "Aprendizaje" de la bitácora):

- Dividir un prompt grande en partes con pausa de revisión hizo visibles 33 decisiones implícitas (E-009).
- La IA es fuerte en coherencia documental, pero pasa por alto lo que depende del framework: enums numéricos, `DateTimeKind.Unspecified`, el `404` sin Problem Details (E-009).
- Las verificaciones del agente se repiten con comandos propios y con conteos (E-010, E-012).
- Revisar el plan antes del código y definir un criterio de salida evita revisiones sin fin (E-013, E-016).
- Probar contra la base real reveló lo que el modelo no ve: el `RESTART` de la SEQUENCE (E-017).

**Retos superados:** veredicto de la auditoría de NO LISTA a LISTA (55 hallazgos resueltos) · paquetes transitivos vulnerables detectados y corregidos (E-014).

**Notas del orador:**
Me llevo cinco aprendizajes. Primero, un prompt grande se controla mejor dividido en partes con pausas: en el contrato de API eso sacó a la luz 33 decisiones que la IA habría tomado sola. Segundo, la IA es excelente en coherencia documental, en cruces y conteos, pero se le escapan detalles del comportamiento por defecto de .NET, como que acepta enums numéricos o que EF Core lee las fechas sin la marca UTC; eso requiere conocimiento del framework. Tercero, cuando el agente dice "verificado", lo repito con mis propios comandos. Cuarto, revisar el plan antes del código y fijar cuándo detener una revisión evita ciclos infinitos. Y quinto, probar contra la base real: la SEQUENCE de SQL Server tenía un comportamiento al reiniciarse que ningún modelo en memoria habría mostrado.

---

## Diapositiva 17 · Mejoras pendientes

**Viñetas** (fuentes: `03-contrato-api.md`, `00-vision.md` §4, `README.md`, `specs/trazabilidad.md`, bitácora):

- **Implementar 11 de 13 operaciones:** RF-02 (listar, consultar), RF-03 (asignar, reasignar, técnicos), RF-04 (iniciar, resolver, cerrar, reabrir), RF-05 (cambiar prioridad) y RF-06 (reporte).
- **Trazabilidad:** llenar las columnas Test y Commit de `specs/trazabilidad.md`; registrar RF-01 en la bitácora (E-018); PR de `feat/rf-01` a `main`.
- **Documentación desactualizada:** `README.md` aún dice que no existen el middleware ni los controladores.
- **Fuera del MVP (visión §4):** autenticación JWT (hoy `X-Usuario-Id`, S-02), Docker, notificaciones por correo, SLA en horario laboral, historial de cambios y control de concurrencia.
- **Límite conocido:** el código llega hasta `HD-999999` (RN-03).

**Notas del orador:**
Para cerrar el proyecto quedan pendientes claros. El principal es implementar las once operaciones restantes. La buena noticia es que el camino ya está trazado: el dominio está probado, el contrato define cada respuesta y RF-01 dejó un patrón completo, con middleware, manejo de errores y pruebas de extremo a extremo, que se repite para cada operación. También tengo deudas de trazabilidad: llenar las columnas de test y commit de la matriz, registrar la implementación de RF-01 en la bitácora, actualizar una frase del README y abrir el pull request a main. Y hay mejoras que quedaron fuera del MVP de forma consciente y documentada: autenticación con JWT en lugar del encabezado X-Usuario-Id, Docker, notificaciones por correo, SLA en horario laboral, historial de cambios y control de concurrencia.

---

## Diapositiva 18 · Cierre

**Título:** Gracias · ¿Preguntas?

**Datos:** Angélica Tomalá Torres · Help Desk System (sin datos de contacto).

**Mensaje central:** Desarrollo asistido con revisión: cada decisión de la IA tiene un prompt, un diff, un criterio humano y un commit.

**Notas del orador:**
Para resumir: Help Desk System resuelve un problema real del soporte de TI con una API que controla el ciclo de vida del ticket y su SLA. Antes del código hubo especificaciones completas, una auditoría y un contrato. La arquitectura separa el negocio de la infraestructura, y eso se comprueba con pruebas. Y, sobre todo, la IA fue un asistente dirigido: cada propuesta pasó por mi revisión, quedó registrada en la bitácora con su decisión y llegó a un commit. Hoy son 472 pruebas en verde y una ruta clara para lo que falta. Muchas gracias por su atención. Quedo atenta a sus preguntas.

---

## Guion de la demo en vivo (va en las notas de las diapositivas 13 a 15)

**Preparación (antes de la presentación):**

1. Comprobar la cadena de conexión: `dotnet user-secrets list --project src/HelpDesk.Api`.
2. Comprobar que la migración está aplicada: `dotnet ef migrations list --project src/HelpDesk.Infrastructure --startup-project src/HelpDesk.Api` (sin `(Pending)`).
3. Cerrar cualquier instancia de la Api en ejecución (bloquea la compilación, E-016).

**En vivo:**

1. `dotnet run --project src/HelpDesk.Api` → escucha en `http://localhost:3001`.
2. Abrir `http://localhost:3001/swagger`; mostrar las dos operaciones y el documento `http://localhost:3001/openapi/v1.json`.
3. **Crear (201):** `POST /api/v1/tickets` con `X-Usuario-Id: 1` (Ana) y el cuerpo válido de la diapositiva 14. Anotar el `id` devuelto y mostrar `Location`, `codigo`, `fechaVencimiento` (+4 h) y `estadoSla`.
4. **Validación (400):** mismo endpoint con el cuerpo inválido de la diapositiva 14 → `VALIDACION_FALLIDA` con 3 claves en `errors`.
5. **Identidad (401):** sin `X-Usuario-Id` → `USUARIO_NO_IDENTIFICADO`; con `X-Usuario-Id: 999` → `USUARIO_INEXISTENTE`.
6. **Permiso (403):** crear con `X-Usuario-Id: 6` (Fabián, técnico) → `ROL_NO_AUTORIZADO`.
7. **Reclasificar (200):** `POST /api/v1/tickets/{id}/reclasificar` con `X-Usuario-Id: 10` (Jorge, supervisor) y `{ "categoria": "Otros" }`; el ticket está `Abierto`, así que solo el supervisor puede (RN-13).
8. **Permiso (403):** reclasificar con `X-Usuario-Id: 1` (solicitante) → `ROL_NO_AUTORIZADO`; con `X-Usuario-Id: 7` (técnico no asignado) → `TICKET_AJENO`.
9. **Recurso (404):** reclasificar `{id}` = `012` o un Id que no existe → `TICKET_NO_ENCONTRADO`.
10. Cerrar con `dotnet test` en la terminal (o mostrar la diapositiva 15 si el tiempo es corto).

**Nota:** no hay CRUD completo: el MVP no tiene `GET` implementado todavía ni elimina tickets (decisión 18). El `409` `TICKET_NO_MODIFICABLE` no se puede mostrar en vivo, porque no hay endpoint para resolver o cerrar; está cubierto por `ReclasificarTicketEndpointTests`.

---

## Lista de [PENDIENTE] para completar a mano

1. **Diapositiva 13:** insertar la captura de Swagger UI en el recuadro "[Captura Swagger UI]".
2. **Bitácora E-018:** poner el veredicto (✅/✏️/❌), las justificaciones, el prompt, las incidencias y el aprendizaje del borrador.

## Decisiones que tomé al armar el guion (para que las revises)

- 19 diapositivas contando portada y cierre (0 a 18), aprobadas por la desarrolladora.
- Mostré con honestidad que solo RF-01 está implementado de punta a punta (diapositivas 5, 13 y 17), en lugar de presentar las 13 operaciones como hechas.
- Para el ejemplo de request válido e inválido usé los ejemplos del contrato, porque son también pruebas automáticas del repositorio.
- El conteo 198 ✅ / 38 ✏️ / 16 ❌ sale de contar las filas de las tablas de revisión de la bitácora; algunas entradas antiguas usan otro formato, así que es una cifra aproximada al alza o a la baja en pocas filas.
