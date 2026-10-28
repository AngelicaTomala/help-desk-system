# Bitácora

Registro de las acciones relevantes del proyecto y de las interacciones con asistentes de IA (Claude y Claude Code). Cada entrada documenta qué se pidió, qué propuso la IA, qué se decidió y qué se aceptó, con su correspondiente justificación.

**Leyenda de decisión:** ✅ Aceptado · ✏️ Modificado · ❌ Rechazado

---

## E-001 · 26/10/2026 · Fase 0 Creación del repositorio y estructura

**Herramienta:** Ninguna (acción manual)

**Acciones:**

- Creación del repositorio `AngelicaTomala/help-desk-system` en GitHub.
- Creación de la carpeta local del proyecto con la estructura definida por la guía del curso:
  `src/`, `tests/`, `specs/functional/`, `specs/technical/`, `docs/`, `evidencias/`.
- Creación del `.gitignore` basado en la plantilla de .NET, excluyendo además archivos
  locales del agente (`.claude/settings.local.json`, `CLAUDE.local.md`).

**Decisiones:**

- Se excluye la carpeta `docker/`: el despliegue con contenedores queda fuera de alcance.

**Commit relacionado:** (pendiente)

---

## E-002 · 26/10/2026 · Fase 1 Especificaciones funcionales

**Herramienta:** Asistente de IA (Claude)

**Decisiones propias tomadas:**

- Tema del proyecto: API REST para gestionar el ciclo de vida de los tickets de soporte de TI.
- Roles: Solicitante, Técnico y Supervisor.
- SLA por prioridad: Crítica 2 h, Alta 4 h, Media 8 h, Baja 24 h.
- Estados: Abierto → Asignado → En progreso → Resuelto → Cerrado, con Reabrir desde Resuelto.

**Resultado de la IA:**
Borradores de `00-vision.md`, `01-requisitos-funcionales.md`
(6 RF, 8 historias, 31 criterios de aceptación) y `02-reglas-negocio.md` (12 reglas y
diagrama de estados), ubicados en `specs/functional/`.

**Revisión humana:**

| Elemento propuesto                                     | Decisión | Justificación                                                                                                                                                                      |
| ------------------------------------------------------ | -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SLA propuesto de 4 / 8 / 24 / 72 h (RN-10)             | ✏️       | Se ajustó a 2 / 4 / 8 / 24 h porque en un área de producción una falla crítica no puede esperar 4 horas.                                                                           |
| Comentario obligatorio para resolver el ticket (RN-09) | ✅       | Se podrá llevar una trazabilidad de las actividades que realizó el técnico.                                                                                                        |
| Reasignar devuelve el ticket a `Asignado`              | ✅       | Se considera que debe quedar asignado porque el ticket pasa a un nuevo técnico.                                                                                                    |
| Reabrir vuelve a `En progreso` con el mismo técnico    | ✅       | Estoy de acuerdo con que sea el mismo técnico porque conoce el problema y puede realizar una rápida revisión.                                                                      |
| Cambio de prioridad recalcula el SLA (RN-12)           | ✅       | Si se cambia la prioridad, la fecha de vencimiento también debe recalcularse en base a la nueva prioridad.                                                                         |
| Categorías fijas (RN-02)                               | ✅       | Se mantiene la lista fija, pero se agrega la reclasificación (RN-13): el solicitante puede equivocarse al elegir la categoría y quien atiende el caso puede corregirla. Ver E-005. |

**Incidencias:**
Se evaluaron dos opciones: (A) lista fija y (B) tabla administrable con CRUD. Se eligió A porque resuelve el problema real (errores de clasificación del solicitante), frente a un CRUD completo.

**Commit relacionado:** (pendiente)

---

## E-003 · 27/10/2026 · Fase 1.1 Configuración del agente y MCP

**Herramienta:** Claude Code (VS Code)

**Acciones:**

- generación del archivo `CLAUDE.md` (memoria del proyecto) por medio del comando `/init`.
- Configuración de dos servidores MCP a nivel de proyecto en `.mcp.json`:

| Servidor                      | Tipo                      | Propósito                                                                            |
| ----------------------------- | ------------------------- | ------------------------------------------------------------------------------------ |
| `github-mcp-server-help-desk` | HTTP (GitHub MCP remoto)  | Permite al agente consultar el repositorio remoto, crear issues y verificar commits. |
| `playwright`                  | stdio (`@playwright/mcp`) | Permite al agente abrir Swagger UI en Chromium y validar endpoints visualmente.      |

**Decisiones de seguridad:**

- El token de GitHub no está en el repositorio: se lee de la variable de entorno `GITHUB_TOKEN_MCP_HELP_DESK`.
- El token es _fine-grained_, con acceso solo a este repositorio y permisos mínimos.
- Se excluyen de Git los archivos locales del agente (`.claude/settings.local.json`, `CLAUDE.local.md`).

**Decisión técnica:**
Se eligió Playwright MCP en lugar de Claude in Chrome porque usa un navegador aislado, sin las sesiones personales del usuario, y permite validaciones repetibles.

**Revisión humana:**
Se revisó que `.mcp.json` no contuviera secretos antes de incluirlo en el commit.

**Commit relacionado:** (pendiente)

---

## E-004 · 27/10/2026 · Fase 1.2 Vinculación del repositorio y primer commit

**Herramienta:** Claude Code (VS Code) + MCP de GitHub

**Prompt:**

**Contexto:**
El proyecto Help Desk System contiene especificaciones funcionales (`specs/functional/`), bitácora de evidencias (`evidencia/bitacora.md`), `CLAUDE.md`, `.gitignore` y `.mcp.json`.
Existe un repositorio en GitHub (`AngelicaTomala/help-desk-system`), pero la carpeta local NO está vinculada con Git; solo tengo configurado el MCP de GitHub.

**Tarea:**

1. Verifica si la carpeta ya es un repositorio Git (`git status`). Si no lo es, ejecuta `git init` y crea la rama `main`.
2. Usa el MCP de GitHub para revisar si el repositorio remoto ya tiene commits y dime qué contiene.
3. Configura el remoto: `git remote add origin` con la URL HTTPS del repositorio.
4. Revisa que ningún archivo contenga tokens, claves o cadenas de conexión.
5. Haz el commit: `docs(specs): visión, requisitos funcionales y reglas de negocio`
6. Haz push a `main`.
7. Confirma con el MCP de GitHub que el commit aparece en el remoto y dame su hash.

**Restricciones:**

- No modifiques el contenido de ningún archivo.
- No uses el MCP de GitHub para subir archivos; el commit y el push van con git.
- Si encuentras un posible secreto, detente y avísame antes de hacer commit.
- Si el remoto ya tiene commits, detente antes del paso 6 y explícame las opciones para unir ambos historiales sin perder archivos. No uses `push --force`.

**Verificación:**
Muéstrame la lista de archivos incluidos en el commit y el hash confirmado desde GitHub.

**Resultado de la IA:**

- Estado inicial de la carpeta: No era un repositorio Git. Se ejecutó `git init -b main` y se configuró `origin` → `https://github.com/AngelicaTomala/help-desk-system.git`.
- Contenido del remoto según el MCP: Un commit (`21ae3a5` "Initial commit", creado desde la web el 26/10/2026) con solo un `README.md` de 124 bytes.
- Revisión de secretos:Sin hallazgos. `.mcp.json` solo referencia la variable `${GITHUB_TOKEN_MCP_HELP_DESK}`
- Archivos incluidos en el commit:`.gitignore`, `.mcp.json`, `CLAUDE.md`, `docs/evidencias/bitacora.md`, `package.json`, `package-lock.json`, `specs/functional/00-vision.md`, `specs/functional/01-requisitos-funcionales.md`, `specs/functional/02-reglas-negocio.md`.
- Comportamiento del agente: Se detuvo antes del push al detectar historiales distintos, como indicaba la restricción, y propuso tres opciones (merge, rebase, reset).
- Hash confirmado por el MCP:61b123f21d2b8c3f6559737df3e5e4dea393a36d

**Revisión humana:**

| Acción propuesta por la IA                               | Decisión | Justificación                                                                                                                   |
| -------------------------------------------------------- | -------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Inicializar Git y crear rama `main`                      | ✅       | La carpeta no estaba versionada y `main` es la rama principal del repositorio remoto.                                           |
| Configurar el remoto `origin` por HTTPS                  | ✅       | Coincide con el repositorio creado en GitHub; las credenciales las gestiona Windows, no el token del MCP.                       |
| Resultado de la revisión de secretos                     | ✅       | Verifiqué que `.mcp.json` solo contiene la referencia a la variable de entorno.                                                 |
| Unir historiales con merge                               | ✅       | Conserva el commit inicial de GitHub sin reescribir el historial                                                                |
| Commit directo en `main` pese a la regla del `CLAUDE.md` | ✅       | Excepción justificada por ser el commit inicial. Desde el siguiente cambio se trabajará en una rama diferente con pull request. |

**Incidencias:**
El repositorio remoto tenía un commit inicial creado desde la web, por lo que el push directo no era posible. Se resolvió con merge de historiales

**Aprendizaje:**
El MCP de GitHub no vincula la carpeta local con el repositorio remoto; solo da acceso a la API de GitHub. Fue necesario inicializar Git y configurar el remoto (`git remote add origin`). Desde entonces el flujo es: commit y push con Git, y verificación con el MCP.

**Commit relacionado:**
a69d28dd920a9046bbdc325bd3b4d5c7d11abc4b

---

## E-005 · 27/10/2026 · Requisitos no funcionales

**Herramienta:** Claude Code (VS Code)

**Prompt:**

**Contexto:**
Proyecto Help Desk System con API REST .NET 10, Clean Architecture,
SQL Server y EF Core. Lee specs/functional/ (visión, requisitos y reglas).

**Tarea:**
Crea specs/functional/03-requisitos-no-funcionales.md con requisitos
RNF-01 al RNF-06 agrupados en: seguridad, manejo de errores, validación,
rendimiento, mantenibilidad, documentación (OpenAPI) y pruebas.
Cada RNF debe tener ID, descripción y un criterio medible de verificación.

**Restricciones:**

- Máximo 12 RNF; solo los que se puedan cumplir en una API sin frontend ni Docker.
- Respeta los supuestos de 00-vision.md (S-02 identidad por encabezado, S-03, S-04).
- No modifiques otros archivos ni escribas código.

**Formato:**
Tabla Markdown con ID | Categoría | Requisito | Criterio de verificación | RF/RN relacionados.

**Verificación:**
Confirma que cada RNF se puede comprobar con una prueba o una revisión concreta.

**Resultado de la IA:**

- Creó `specs/functional/03-requisitos-no-funcionales.md` con 10 RNF (máximo permitido: 12), sin modificar otros archivos ni escribir código.
- Distribución por categoría:

| Categoría         | RNF                                                                                                              |
| ----------------- | ---------------------------------------------------------------------------------------------------------------- |
| Seguridad         | RNF-01 identidad por `X-Usuario-Id` (401/403) · RNF-02 consultas parametrizadas y sin secretos en el repositorio |
| Manejo de errores | RNF-03 ProblemDetails y 500 sin stack trace · RNF-04 códigos HTTP uniformes (409 no modifica datos)              |
| Validación        | RNF-05 FluentValidation con casos frontera                                                                       |
| Rendimiento       | RNF-06 paginación y filtros en SQL, índices, menos de 1 s con 10 000 tickets                                     |
| Mantenibilidad    | RNF-07 regla de dependencias entre capas · RNF-08 reglas en Domain, TimeProvider y fechas UTC                    |
| Documentación     | RNF-09 OpenAPI con códigos de respuesta y encabezado `X-Usuario-Id`                                              |
| Pruebas           | RNF-10 una prueba por cada CA y RN con `[Trait]` ·                                                               |

- Cada criterio indica si se verifica con prueba automatizada o con revisión concreta (código, SQL generado, `.csproj` u `openapi.json`). Solo RNF-06 requiere medición manual.
- El agente señaló cuatro decisiones que las especificaciones no definían y pidió revisarlas.

**Revisión humana:**

| Elemento propuesto                                                | Decisión | Justificación                                                                                                           |
| ----------------------------------------------------------------- | -------- | ----------------------------------------------------------------------------------------------------------------------- |
| 10 RNF en 7 categorías                                            | ✅       | Cubren las categorías pedidas sin exceder el máximo y todos son verificables.                                           |
| `pageSize` fuera de rango responde 400 en lugar de ajustarse a 50 | ✅       | Un error explícito evita que el cliente asuma que recibió todos los registros. Se actualiza CA-02.4 para reflejarlo.    |
| Índice único sobre `Codigo`                                       | ✅       | Garantiza a nivel de base de datos la unicidad que exige RN-03.                                                         |
| Pruebas de JWT expirado (pedidas en `CLAUDE.local.md`)            | ❌       | JWT está fuera del alcance del MVP (S-02). Se corrige `CLAUDE.local.md` para que no pida pruebas de algo que no existe. |
| Tiempo de respuesta < 1 s con 10 000 tickets                      | ✅       | Valor razonable para una consulta paginada con índices; se medirá manualmente.                                          |
| Etiquetar cada prueba con `[Trait]` por CA y RN (RNF-10)          | ✅       | Permite llenar la matriz de trazabilidad y ejecutar las pruebas de un requisito específico.                             |

**Incidencias:**

- Se detectó una contradicción entre `CLAUDE.local.md` (pruebas de JWT) y el alcance del MVP.

**Aprendizaje:**
El agente señaló por iniciativa propia decisiones que las especificaciones no definían (comportamiento de `pageSize` fuera de rango) y una contradicción entre `CLAUDE.local.md` y el alcance del MVP.
Detectarlas antes de implementar evita corregir código después.
Desde ahora, los prompts de diseño incluirán en la Verificación: "Señala las decisiones que tomaste y que no estaban definidas en las especificaciones"

**Commit relacionado:**
d9fea13

---

## E-006 · 27/10/2026 · Modelo de dominio

**Herramienta:** Claude Code (VS Code)

**Prompt Inicial:**

**Contexto:**
Lee la documentacion del proyecto Help Desk System que se encuentra dentro de specs/functional/

**Tarea:**
Propón el modelo de dominio y espera mi aprobación antes de crear el archivo.

Debe incluir:

1. Entidades con atributos y tipos (Ticket, Usuario, Comentario).
2. Enumeraciones (Estado, Prioridad, Rol, EstadoSla).
3. Relaciones y cardinalidades.
4. Qué reglas de negocio (RN-xx) vive en cada entidad y cuáles en Application. Con sus respectivos responsables
5. Diagrama de clases en Mermaid (classDiagram).
6. Diagrama de estados del ticket en Mermaid, coherente con RN-08.

**Restricciones:**

- EstadoSla es un valor calculado, (RN-11).
- Fechas en UTC (S-04). El Usuario necesita un indicador de activo (RN-06).
- No escribas código C#; solo el documento.

**Verificación:**
Señala cualquier contradicción entre requisitos y reglas de negocio.

**Resultado de la IA:**

1. **Primera propuesta:** 3 entidades (Usuario, Ticket, Comentario), 6 enumeraciones, ubicación de RN-01 a RN-12, diagrama de clases, diagrama de estados y 9 hallazgos. No incluía la reclasificación.
2. **Segunda propuesta** (tras agregar RN-13 a las especificaciones): incluyó `Ticket.Reclasificar`, la guarda común `EstaAbiertoParaCambios()`, la ubicación de RN-13 y 4 hallazgos nuevos (13 en total).
3. **Ejecución:**
   - Creó `specs/technical/01-modelo-dominio.md` con las secciones 1–7 y la columna "Decisión".
   - Mostró el diff de `specs/functional/` antes de guardarlo; tras la aprobación, actualizó `00-vision`, `01-requisitos-funcionales`, `02-reglas-negocio` y `03-requisitos-no-funcionales`.
   - Resultado: 9 HU, 36 CA y 13 RN, con los totales coherentes en los cuatro documentos.

**Revisión humana — hallazgos del agente:**

| #   | Hallazgo                                                   | Decisión | Justificación                                                                                                                                                                                                                                                                     |
| --- | ---------------------------------------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Técnico inexistente o inactivo sin código de respuesta     | ✅       | Se responde 400 y se amplía CA-03.3; RN-06 queda cubierta por una prueba.                                                                                                                                                                                                         |
| 2   | La visión prometía "registrar comentarios" sin endpoint    | ✏️       | Se elimina el tipo `General` y la frase de la visión; no se promete lo que no se implementará.                                                                                                                                                                                    |
| 3   | Falta el 403 al cambiar la prioridad                       | ✅       | Se agrega CA-05.6: toda regla de permiso necesita su prueba.                                                                                                                                                                                                                      |
| 4   | Técnico consultando un ticket que no tiene asignado        | ✅       | Se extiende CA-02.6, coherente con RN-04.                                                                                                                                                                                                                                         |
| 5   | Reabrir conserva el vencimiento original                   | ✅       | El SLA mide el tiempo hasta la solución real; si se reabre, la primera solución falló y el incumplimiento debe reflejarse. Recalcular ocultaría la falla y permitiría manipular las métricas. Se evaluaron como alternativas recalcular desde la reapertura y un plazo de gracia. |
| 6   | Grafía `Critica` / "Crítica"                               | ✅       | Enum sin tilde; la tilde solo en textos.                                                                                                                                                                                                                                          |
| 7   | La longitud de la descripción no tenía CA                  | ✅       | Se amplía CA-01.3 para cubrir RN-01 completa.                                                                                                                                                                                                                                     |
| 8   | Sin límite para comentarios ni regla para `page` < 1       | ✅       | Texto ≤ 2000 y `page` < 1 → 400, mismo criterio que `pageSize`.                                                                                                                                                                                                                   |
| 9   | Técnico desactivado con tickets asignados                  | ✅       | Fuera del MVP (usuarios semilla); se anota en la visión.                                                                                                                                                                                                                          |
| 10  | Reclasificar con categoría inválida sin CA                 | ✅       | Se agrega CA-01.9; RN-13 queda cubierta completa.                                                                                                                                                                                                                                 |
| 11  | En un ticket Abierto solo el supervisor puede reclasificar | ✅       | Es coherente: aún no hay técnico asignado.                                                                                                                                                                                                                                        |
| 12  | El documento de RNF quedó desactualizado                   | ✅       | Se actualizan los totales y las referencias para que los documentos coincidan.                                                                                                                                                                                                    |
| 13  | La visión daba al técnico más permiso que RN-13            | ✅       | Se precisa: "reclasificar sus tickets asignados".                                                                                                                                                                                                                                 |

**Revisión humana — decisiones propias del agente:**

| Decisión del agente                                                | Decisión | Justificación                                                                                               |
| ------------------------------------------------------------------ | -------- | ----------------------------------------------------------------------------------------------------------- |
| Entidad Comentario con tipo (Solución, Reapertura)                 | ✅       | Registra la trazabilidad de RN-09 sin columnas extra en Ticket.                                             |
| Email en Usuario (≤ 150, único)                                    | ✅       | Aunque el MVP no lo usa, identifica de forma única a cada usuario                                           |
| SEQUENCE de SQL Server para el código del ticket                   | ✅       | Evita códigos duplicados ante creaciones simultáneas (RN-03).                                               |
| Enumeraciones guardadas como texto                                 | ✅       | La base de datos es legible y coherente con OpenAPI (RNF-09).                                               |
| Guarda común `EstaAbiertoParaCambios()`                            | ✅       | Reclasificar y cambiar prioridad comparten la misma condición; evita duplicar la regla.                     |
| 409 desde Domain y 403 desde Application                           | ✅       | Las reglas del ticket viven en el dominio; los permisos dependen del usuario y se validan en la aplicación. |
| Borrado en cascada de comentarios                                  | ✅       | Un comentario no existe sin su ticket.                                                                      |
| "y no cambia" en CA-01.9 y CA-05.6                                 | ✅       | La prueba verifica también que el dato no se modificó ante el error.                                        |
| RN-13 agregada a RNF-01, RNF-04 y RNF-05 (fuera del diff mostrado) | ✅       | Es coherente, porque esos RNF ya citan CA-01.7 a CA-01.9. Ver Incidencias.                                  |

**Incidencias:**

- La primera propuesta no incluía la reclasificación porque RN-13 aún no se había agregado a las especificaciones, aunque la decisión (opción A) ya estaba tomada en E-005. Se agregaron RN-13, HU-09 y CA-01.6 a CA-01.8, y se volvió a ejecutar el prompt.
- El agente hizo un cambio que no estaba en el diff aprobado (agregar RN-13 en tres RNF), aunque lo informó por iniciativa propia. Se aceptó por ser coherente, pero se registra porque se desvió de lo aprobado.

**Aprendizaje:**

- Revisar el plan contra las decisiones propias detectó una regla omitida antes de llegar al código.
- Pedir el diff antes de guardar permitió revisar cada criterio nuevo.
- Los hallazgos del agente completaron criterios faltantes: cada regla de negocio quedó con al menos una prueba posible.

**Commits relacionados:**

- `9fe9cff` docs(specs): modelo de dominio
- `f55293e` docs(specs): aplica decisiones del modelo de dominio en specs funcionales

---

## E-007 · 27/10/2026 · Diagramas C4 de niveles 1 y 2

**Herramienta:** Claude Code (VS Code)

**Prompt:**

**Contexto:**
Help Desk System, API REST .NET 10, Clean Architecture
(Domain, Application, Infrastructure, Api), SQL Server, EF Core.
Lee specs/functional/00-vision.md, specs/functional/03-requisitos-no-funcionales.md
y specs/technical/01-modelo-dominio.md.

**Tarea:**
Crea specs/technical/02-arquitectura-c4.md con:

1. Nivel 1 (Contexto): los 3 roles, el sistema y sistemas externos (si existen).
2. Nivel 2 (Contenedores): Swagger UI, API REST y base de datos SQL Server,
   indicando tecnología y forma de comunicación (HTTP/JSON, encabezado
   X-Usuario-Id, EF Core).
3. Una sección con las 4 capas de Clean Architecture, qué contiene cada una
   y la regla de dependencias (qué capa puede referenciar a cuál).
   Usa Mermaid (C4Context y C4Container) con una breve explicación de cada diagrama.

**Restricciones:**

- No incluyas Docker, colas, caché, correo, JWT ni servicios fuera del alcance.
- Mismo encabezado (versión, fecha, estado) que los demás documentos de specs.
- No modifiques otros archivos.

**Verificación:**

- Confirma que la sintaxis Mermaid sea válida para que GitHub la renderice.
- Señala las decisiones que tomaste y que no estaban definidas en las especificaciones.

**Resultado de la IA:**

- Creó `specs/technical/02-arquitectura-c4.md` con el mismo encabezado que los demás documentos, sin modificar otros archivos.
- **Nivel 1 (Contexto):** los tres roles y el Help Desk System. No hay sistemas externos, y el documento explica por qué.
- **Nivel 2 (Contenedores):** Swagger UI, API REST y SQL Server, con su tecnología y comunicación (HTTP/JSON con `X-Usuario-Id`; EF Core hacia la base de datos), más una tabla resumen.
- **Clean Architecture:** tabla con el contenido de cada capa y sus referencias permitidas, diagrama de dependencias y regla de dependencias.
- **Sintaxis Mermaid:** el agente la revisó manualmente contra la documentación, pero no la ejecutó en un renderizador. Advirtió que C4 es experimental en Mermaid y recomendó confirmarlo en GitHub.

**Revisión humana — decisiones del agente no definidas en las especificaciones:**

| Decisión del agente                                                                   | Decisión | Justificación                                                                                                                                                              |
| ------------------------------------------------------------------------------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Diagrama `flowchart` adicional para las capas                                         | ✅       | C4 no tiene notación para capas internas; el diagrama complementa la tabla y hace visible la regla de dependencias.                                                        |
| Swagger UI como contenedor separado                                                   | ✅       | Aunque lo sirve la propia API, separarlo deja claro cómo interactúan los usuarios con el sistema en el MVP sin frontend. Se documenta que solo está activo en Development. |
| Api → Infrastructure solo para registrar dependencias en `Program.cs`                 | ✅       | Es la única excepción aceptable en Clean Architecture: la Api arma la aplicación, pero no usa Infrastructure directamente.                                                 |
| Conversión de excepciones a ProblemDetails y lectura de `X-Usuario-Id` en la capa Api | ✅       | Son responsabilidades HTTP; mantenerlas fuera de Domain y Application conserva el negocio independiente del transporte.                                                    |
| Interfaces de persistencia en Application, implementadas en Infrastructure            | ✅       | Inversión de dependencias: Application no depende de EF Core, lo que permite probarla sin base de datos.                                                                   |
| Pruebas unitarias en Domain y Application; integración en Api                         | ✅       | Coherente con RNF-10 y RNF-11 (cobertura en Domain y Application).                                                                                                         |
| Conexión a SQL Server indicada solo como "EF Core"                                    | ✅       | El protocolo de red no aporta a la comprensión del diseño en el nivel 2.                                                                                                   |

**Revisión del contenido:**

| Elemento                           | Decisión | Justificación                                           |
| ---------------------------------- | -------- | ------------------------------------------------------- |
| Nivel 1 sin sistemas externos      | ✅       | Correo y JWT están fuera del alcance del MVP.           |
| Nivel 2: contenedores y protocolos | ✅       | Refleja el encabezado `X-Usuario-Id` del supuesto S-02. |
| Regla de dependencias entre capas  | ✅       | Coincide con RNF-07: Domain no depende de ninguna capa. |

**Incidencias:**
El agente no validó los diagramas en un renderizador; solo revisó la sintaxis manualmente.

**Aprendizaje:**
Que el agente diga "la sintaxis es válida" no equivale a comprobarlo. La verificación debe hacerse en el entorno donde se leerá el documento, en este caso GitHub.

**Commit relacionado:**

- `0ab5aa4` docs(specs): arquitectura C4

---

## E-008 · 27/10/2026 · Decisiones de arquitectura (ADRs)

**Herramienta:** Claude Code (VS Code)

**Prompt:**

**Contexto:**
Help Desk System, API REST .NET 10, Clean Architecture
(Domain, Application, Infrastructure, Api), SQL Server, EF Core.
Lee la documentacion que se encuentra dentro de specs/ .

**Tarea:**
Crea 4 ADR en specs/technical/adr/ con el formato de Michael Nygard
(Título, Estado, Fecha, Contexto, Decisión, Consecuencias positivas y negativas,
Alternativas consideradas):

- ADR-001-clean-architecture.md
- ADR-002-sqlserver-efcore.md
- ADR-003-fluentvalidation.md
- ADR-004-problemdetails-errores.md
  Crea también specs/technical/adr/README.md con un índice de los ADR
  (número, título, estado y enlace).

**Restricciones:**

- Cada ADR debe justificarse con requisitos concretos (RF, RN o RNF por su ID).
- Al menos 2 alternativas consideradas por ADR, con el motivo de descarte.
- Máximo 1 página por ADR. Estado: "Aceptado". Fecha: 27/10/2026.
- Los ADR deben ser coherentes con decisiones ya tomadas en el modelo de dominio
  y la arquitectura (SEQUENCE para el código del ticket, enums como texto,
  409 desde Domain y 403 desde Application, interfaces de persistencia en Application).
- ADR-003 debe explicar cómo se reparte la validación entre FluentValidation
  (Application) y las invariantes del Domain, sin duplicar reglas innecesariamente.
- ADR-004 debe usar las capacidades nativas de ASP.NET Core para ProblemDetails,
  sin librerías externas si no son necesarias.
- No modifiques otros archivos.

**Verificación:**

- Lista qué requisitos justifica cada ADR.
- Señala las decisiones que tomaste y que no estaban definidas en las especificaciones.

**Prompts de revisión:**

1. Corrección del reparto de la validación: el Domain también valida RN-01 con las constantes compartidas, y se alinea `01-modelo-dominio.md`.
   **Prompt**
   Revisé los ADR. haz el siguiente cambio:

ADR-003: modifica el reparto de la validación. El Domain sí valida las
invariantes de RN-01 (longitudes) usando las mismas constantes públicas que
reutilizan los validadores; lanza una excepción de dominio si se violan.
El validador de Application sigue siendo el que responde 400 con el detalle.
Justificación: un Ticket inválido nunca debe poder existir (RNF-08), y las
constantes compartidas evitan la duplicación.

Después, en un cambio aparte, revisa que 01-modelo-dominio.md quede
coherente con el ADR-003 corregido y muéstrame el diff antes de guardar.

Acepto el resto de tus decisiones

2. Extensión del mismo criterio a RN-09 y diff final de ambos archivos.
   **Prompt**
   Acepto el cambio en ADR-003 y la excepción de dominio como 500.
   Acepto quitar el 401 del criterio y la versión 1.1.

Un ajuste al diff antes de guardar: aplica el mismo criterio a RN-09.
Ticket.Resolver y Ticket.Reabrir también deben lanzar una excepción de
dominio si el texto está vacío o supera Comentario.TextoLongitudMaxima,
porque un ticket resuelto sin solución es un estado inválido (RNF-08).
Los validadores siguen respondiendo 400 con el detalle (CA-04.3).
Actualiza la fila de RN-09 en 01-modelo-dominio.md y la tabla de reparto
de ADR-003 para que digan lo mismo.

Muéstrame el diff final de ambos archivos antes de guardar.

3. Aprobación: validación del texto en `Ticket`, guardar, commits y push.
   **Prompt**
   Aprobado. La validación del texto se queda en Ticket, como raíz del agregado.
   Guarda ambos archivos y haz dos commits:
1. specs/technical/adr/ → "docs(adr): decisiones de arquitectura 001-004"
1. specs/technical/01-modelo-dominio.md → "docs(specs): alinea modelo de dominio con ADR-003"
   Luego haz push de feat/specs-tecnicas y dame los dos hashes.

**Resultado de la IA:**

- Creó 4 ADR en `specs/technical/adr/` con formato Nygard y un índice (`README.md`) con número, título, estado, fecha y enlace.
- Requisitos justificados por cada ADR:

| ADR                      | Requisitos                                                                                                                                 |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| 001 Clean Architecture   | RNF-07, RNF-08, RNF-10 · RN-01 a RN-13 · S-02                                                                                              |
| 002 SQL Server + EF Core | RNF-02, RNF-06, RNF-09 · RN-03, RN-05 · CA-02.3, CA-02.4 · S-01, S-04                                                                      |
| 003 FluentValidation     | RNF-05, RNF-08, RNF-10 · RN-01, RN-02, RN-05, RN-09, RN-13 · CA-01.3, CA-01.4, CA-01.9, CA-02.4, CA-04.3                                   |
| 004 ProblemDetails       | RNF-01, RNF-03, RNF-04, RNF-07, RNF-09 · RN-07, RN-08, RN-12, RN-13 · CA-01.5, CA-01.7, CA-02.2, CA-02.6, CA-03.4, CA-04.6, CA-05.5 · S-02 |

- ADR-004 usa solo piezas nativas de ASP.NET Core: `AddProblemDetails()`, `UseExceptionHandler()`, `IExceptionHandler` y `ValidationProblemDetails`.
- El agente detectó que `01-modelo-dominio.md` contradecía el ADR-003 y no lo modificó porque el prompt lo prohibía; lo propuso como cambio aparte.
- Tras las correcciones, actualizó ADR-003 y el modelo de dominio (versión 1.1), mostrando el diff antes de guardar.

**Revisión humana — decisiones del agente:**

| Decisión del agente                                                                                                | Decisión | Justificación                                                                                                                                                                 |
| ------------------------------------------------------------------------------------------------------------------ | -------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Domain no valida longitudes; solo el validador de Application                                                      | ✏️       | Se modificó: el Domain también valida las invariantes de RN-01, para que un ticket inválido nunca pueda existir (RNF-08). El validador sigue respondiendo 400 con el detalle. |
| Corrección aplicada solo a RN-01 (interpretación literal)                                                          | ✏️       | Se extendió el mismo criterio a RN-09: un ticket resuelto sin solución es un estado inválido, así que el Domain también lo impide.                                            |
| Límites como constantes públicas en Domain (`Ticket.TituloLongitudMaxima`, `Comentario.TextoLongitudMaxima`, etc.) | ✅       | El límite se define una sola vez y lo reutilizan Domain y validadores; la regla se comprueba en dos lugares sin contradicciones.                                              |
| Validación del texto en `Ticket` (raíz del agregado) y no en `Comentario`                                          | ✅       | Los comentarios solo se crean mediante `Resolver` y `Reabrir`; validar en la raíz del agregado protege la regla en su único punto de entrada.                                 |
| Excepción de dominio que llega a la API → 500                                                                      | ✅       | Indica que falta un validador (error de programación), no un error del usuario; ADR-004 no necesita cambios.                                                                  |
| Validadores invocados explícitamente, sin validación automática                                                    | ✅       | Control explícito en cada caso de uso; la validación automática de FluentValidation.AspNetCore está desaconsejada por sus propios autores.                                    |
| `ResolverTicketValidator` y `ReabrirTicketValidator`                                                               | ✅       | Cubren la obligatoriedad y la longitud de la solución y del motivo (RN-09, CA-04.3).                                                                                          |
| LocalDB o SQL Server local, columnas `datetime2`                                                                   | ✅       | `datetime2` tiene mayor precisión y rango que `datetime`; LocalDB permite ejecutar el proyecto sin un servidor dedicado.                                                      |
| Pruebas de integración contra una base real, no InMemory                                                           | ✅       | InMemory no reproduce restricciones reales (índice único, SEQUENCE). Requiere LocalDB para ejecutar las pruebas; se documentará en el README.                                 |
| Excepciones en lugar de `Result<T>`                                                                                | ✅       | Más simple y coherente con `IExceptionHandler` (ADR-004).                                                                                                                     |
| Excepciones lanzadas antes de `SaveChanges`                                                                        | ✅       | Una operación rechazada no deja cambios parciales en la base de datos.                                                                                                        |
| Quitar el 401 del criterio del modelo de dominio                                                                   | ✅       | La identidad se resuelve en la capa Api, como indica la arquitectura.                                                                                                         |
| Modelo de dominio a versión 1.1                                                                                    | ✅       | Registra el cambio de criterio de validación.                                                                                                                                 |
| ADR-002                                                                                                            | ✏️       | Se agrega la experiencia de la desarrolladora con SQL Server como justificación. (pendiente)                                                                                  |

**Incidencias:**

- El prompt de los ADR se ejecutó dos veces: en la segunda version exigía coherencia con el modelo de dominio y un reparto claro de la validación.
- El agente detectó una contradicción entre el ADR-003 y `01-modelo-dominio.md` y la reportó en lugar de corregirla, respetando la restricción del prompt. Se alineó en un commit aparte.

**Aporte humano:**

- Decisión de que el Domain también proteja las invariantes, contra la propuesta inicial del agente.
- Extensión del criterio a RN-09, que el agente no había aplicado.

**Aprendizaje:**

- Las restricciones del prompt ("no modifiques otros archivos") hicieron que el agente reportara la inconsistencia en lugar de corregirla por su cuenta, lo que permitió decidirla conscientemente.
- El agente aplicó la corrección de forma literal (solo a RN-01). Al revisar el diff se detectó que el mismo principio aplicaba a RN-09: hay que comunicar el criterio, no solo el caso concreto.

**Commits relacionados:**

- `afa2435` docs(adr): decisiones de arquitectura 001-004
- `eb0f1a1` docs(specs): alinea modelo de dominio con ADR-003
