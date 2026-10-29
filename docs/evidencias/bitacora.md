# Bitácora

Registro de las acciones relevantes del proyecto y de las interacciones con asistentes de IA (Claude y Claude Code). Cada entrada documenta qué se pidió, qué propuso la IA, qué se decidió y qué se aceptó, con su correspondiente justificación.

**Leyenda de decisión:** ✅ Aceptado · ✏️ Modificado · ❌ Rechazado

### Plantilla de entrada E-NNN

Campos que exige [CLAUDE.md](../../CLAUDE.md#trazabilidad-y-evidencias) (prompt, resultado, decisiones y commit), con el formato usado en las entradas E-001 a E-008:

```markdown
## E-NNN · Título

**Herramienta:** …

**Prompt:** …

**Resultado de la IA:** …

**Revisión humana:**

| Elemento propuesto | Decisión (✅/✏️/❌) | Justificación |
| ------------------ | ------------------- | ------------- |

**Incidencias:** … (si las hubo)

**Aprendizaje:** … (si aplica)

**Commit relacionado:** hash y mensaje
```

---

## E-001 · Fase 0 Creación del repositorio y estructura

**Herramienta:** Ninguna (acción manual)

**Acciones:**

- Crear el repositorio `help-desk-system` en GitHub.
- Crear en local el proyecto con la estructura definida por la guía del curso: `src/`, `tests/`, `specs/functional/`, `specs/technical/`, `docs/`, `evidencias/`.
- Colocar en el local el archivo `.gitignore` excluyendo archivos locales del agente (`.claude/settings.local.json`, `CLAUDE.local.md`).

**Decisiones:**

- Se excluye la carpeta `docker/`: el despliegue con contenedores queda fuera de alcance.

**Revisión humana:**
Se verificó en GitHub el repositorio creado `AngelicaTomala/help-desk-system` de acceso privado con el archivo README.md

**Commit relacionado:**
`21ae3a5` Initial commit (repositorio creado desde la web).

---

## E-002 · Fase 1 Especificaciones funcionales

**Herramienta:** Claude

**Prompt:**

Contexto

Actúa como un Analista de Software Senior y Experto en Ingeniería de Requisitos. Diseñar los primeros 3 documentos de arquitectura y requerimientos funcionales para el proyecto Help Desk System.

A continuación, te proporciono el contexto y las reglas fundamentales del negocio que debe cubrir el sistema:

- Tema del proyecto: API REST para gestionar el ciclo de vida de los tickets de soporte de TI.
- Stack tecnológico: .NET 10, Clean Architecture, SQL Server, EF Core, FluentValidation, xUnit.
- Roles del sistema: Solicitante, Técnico y Supervisor.
- SLA por prioridad (en horas calendario 24/7 y UTC): Crítica (2h), Alta (4h), Media (8h), Baja (24h).
- Estados y Ciclo de vida: Abierto → Asignado → En progreso → Resuelto → Cerrado. Se permite "Reabrir" desde Resuelto regresando a En progreso.
- Categorías permitidas: Hardware, Software, Red, Accesos, Otros

Tarea

Genera los siguientes 3 archivos en bloques de código Markdown independientes, con un tono profesional, técnico, conciso, sin introducciones ni textos de relleno:
La ubicacion de los 3 archivos será: `specs/functional/`

ARCHIVO 1: "00-Visión"

Incluye las siguientes secciones estructuradas:

1. Problema
2. Objetivo
3. Usuarios y roles
4. Alcance
5. Supuestos
6. Stack tecnológico
7. Criterio de éxito y un Glosario técnico básico.

ARCHIVO 2: "01-requisitos-funcionales"

Para cada RF, debes proveer una tabla detallada de Criterios de Aceptación (CA) escritos en formato Gherkin implícito ("Dado... cuando... entonces..."). Asegúrate de mapear explícitamente las respuestas HTTP (201, 200, 400, 401, 403, 404, 409) según los flujos del negocio, las longitudes de caracteres (ej. título de 5-150, descripción/comentarios hasta 2000), y las restricciones de rol. Termina el archivo con una tabla de "Resumen" que totalice los RF, Historias y sus 45 Criterios de Aceptación.

ARCHIVO 3: "02-reglas-negocio"

Debe incluir la tabla de metadatos y explicar la ubicación de las reglas por capa (Domain vs Application). Debe estructurarse con tablas que tengan ID, Regla y RF asociado, divididas en las siguientes categorías:

1. Registro y datos del ticket
2. Visibilidad
3. Asignación
4. Ciclo de vida
5. SLA
6. Matriz de permisos
7. Matriz regla → requisito

**Resultado de la IA:**
La IA genero los 3 archivos en la ubicación inidcad `00-vision.md`,
`01-requisitos-funcionales.md` (6 RF, 8 historias, 31 criterios de aceptación) y
`02-reglas-negocio.md` (12 reglas y diagrama de estados).
Ubicados en `specs/functional/`.

**Revisión humana:**

| Elemento propuesto        | Decisión | Justificación                                                                                                                                                                                        |
| ------------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 00 · Visión del producto  | ✅       | Describe a la perfección el problema de la gestión informal y define correctamente el alcance del MVP, delimitando las restricciones técnicas (ej. límite HD-999999).                                |
| 01-requisitos-funcionales | ✅       | Estructuró de manera impecable 45 criterios de aceptación en formato Gherkin implícito, aplicando rigurosamente los códigos HTTP (201, 200, 400, 401, 403, 404, 409) y las validaciones de longitud. |
| 02-reglas-negocio         | ✅       | Clasificó con precisión las reglas de negocio, generó una matriz de permisos cruzada excelente e integró un diagrama de estados Mermaid.                                                             |

**Incidencias:**
Ninguna

**Aprendizajes:**
La ia demostro una alta capacidad capacidad para generar los archivos solicitados, asi como tambien la capacidad para asociar las reglas de negocios con los requisitos funcionales.

**Commit relacionado:**
`61b123f` docs(specs): visión, requisitos funcionales y reglas de negocio.

---

## E-003 · Fase 1.1 Configuración del agente y MCP

**Herramienta:** Claude Code (VS Code)

**Acciones:**

- generación del archivo `CLAUDE.md` (memoria del proyecto) por medio del comando `/init`.
- Configuración de dos servidores MCP a nivel de proyecto en `.mcp.json`:

| Servidor                      | Tipo                      | Propósito                                                                            |
| ----------------------------- | ------------------------- | ------------------------------------------------------------------------------------ |
| `github-mcp-server-help-desk` | HTTP (GitHub MCP remoto)  | Permite al agente consultar el repositorio remoto, crear issues y verificar commits. |
| `playwright`                  | stdio (`@playwright/mcp`) | Permite al agente abrir Swagger UI en Chromium y validar endpoints visualmente.      |

**Decisiones**

Decisiones de seguridad:

- El token de GitHub no está en el repositorio: se lee de la variable de entorno `GITHUB_TOKEN_MCP_HELP_DESK`.
- El token es _fine-grained_, con acceso solo a este repositorio y permisos mínimos.
- Se excluyen de Git los archivos locales del agente (`.claude/settings.local.json`, `CLAUDE.local.md`).

Decisión técnica:
Se eligió Playwright MCP en lugar de Claude in Chrome porque usa un navegador aislado, sin las sesiones personales del usuario, y permite validaciones repetibles.

**Revisión humana:**
Se revisó que `.mcp.json` no contuviera secretos antes de incluirlo en el commit.

**Commit relacionado:**
`f3eba20 `

---

## E-004 · Fase 1.2 Vinculación del repositorio

**Herramienta:** Claude Code (VS Code) + MCP de GitHub

**Prompt:**

Contexto:
El proyecto Help Desk System contiene especificaciones funcionales (`specs/functional/`), bitácora de evidencias (`evidencia/bitacora.md`), `CLAUDE.md`, `.gitignore` y `.mcp.json`.
Existe un repositorio en GitHub (`AngelicaTomala/help-desk-system`), pero la carpeta local NO está vinculada con Git; solo tengo configurado el MCP de GitHub.

Tarea:

1. Verifica si la carpeta ya es un repositorio Git (`git status`). Si no lo es, ejecuta `git init` y crea la rama `main`.
2. Usa el MCP de GitHub para revisar si el repositorio remoto ya tiene commits y dime qué contiene.
3. Configura el remoto: `git remote add origin` con la URL HTTPS del repositorio.
4. Revisa que ningún archivo contenga tokens, claves o cadenas de conexión.
5. Haz el commit: `docs(specs): visión, requisitos funcionales y reglas de negocio`
6. Haz push a `main`.
7. Confirma con el MCP de GitHub que el commit aparece en el remoto y dame su hash.

Restricciones:

- No modifiques el contenido de ningún archivo.
- No uses el MCP de GitHub para subir archivos; el commit y el push van con git.
- Si encuentras un posible secreto, detente y avísame antes de hacer commit.
- Si el remoto ya tiene commits, detente antes del paso 6 y explícame las opciones para unir ambos historiales sin perder archivos. No uses `push --force`.

Verificación:
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

## E-005 · Requisitos no funcionales

**Herramienta:** Claude Code (VS Code)

**Prompt:**

Contexto:
Proyecto Help Desk System con API REST .NET 10, Clean Architecture,
SQL Server y EF Core. Lee specs/functional/ (visión, requisitos y reglas).

Tarea:
Crea specs/functional/03-requisitos-no-funcionales.md con requisitos
RNF-01 al RNF-06 agrupados en: seguridad, manejo de errores, validación,
rendimiento, mantenibilidad, documentación (OpenAPI) y pruebas.
Cada RNF debe tener ID, descripción y un criterio medible de verificación.

Restricciones:

- Máximo 12 RNF; solo los que se puedan cumplir en una API sin frontend ni Docker.
- Respeta los supuestos de 00-vision.md (S-02 identidad por encabezado, S-03, S-04).
- No modifiques otros archivos ni escribas código.

Formato:
Tabla Markdown con ID | Categoría | Requisito | Criterio de verificación | RF/RN relacionados.

Verificación:
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
| Pruebas           | RNF-10 una prueba por cada CA y RN con `[Trait]`, RNF-11 cobertura ≥ 80 % en Domain y Application                |

- Cada criterio indica si se verifica con prueba automatizada o con revisión concreta (código, SQL generado, `.csproj` u `openapi.json`). Solo RNF-06 requiere medición manual.
- El agente señaló cuatro decisiones que las especificaciones no definían y pidió revisarlas.

**Revisión humana:**

| Elemento propuesto                                                | Decisión | Justificación                                                                                                                                                         |
| ----------------------------------------------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 10 RNF en 7 categorías                                            | ✅       | Cubren las categorías pedidas sin exceder el máximo y todos son verificables.                                                                                         |
| `pageSize` fuera de rango responde 400 en lugar de ajustarse a 50 | ✅       | Un error explícito evita que el cliente asuma que recibió todos los registros. Se actualiza CA-02.4 para reflejarlo.                                                  |
| Índice único sobre `Codigo`                                       | ✅       | Garantiza a nivel de base de datos la unicidad que exige RN-03.                                                                                                       |
| Pruebas de JWT expirado (pedidas en `CLAUDE.local.md`)            | ❌       | JWT está fuera del alcance del MVP (S-02). Se corrige `CLAUDE.local.md` para que no pida pruebas de algo que no existe.                                               |
| Tiempo de respuesta < 1 s con 10 000 tickets                      | ✅       | Valor razonable para una consulta paginada con índices; se medirá manualmente.                                                                                        |
| Etiquetar cada prueba con `[Trait]` por CA y RN                   | ✅       | Permite llenar la matriz de trazabilidad y ejecutar las pruebas de un requisito específico.                                                                           |
| cobertura ≥ 80 % en Domain y Application                          | ❌       | Lo que realmente da calidad es que cada prueba compruebe un criterio de aceptación concreto, y la cobertura es solo un indicador de que no quedaron partes sin probar |

**Incidencias:**

- Se detectó una contradicción entre `CLAUDE.local.md` (pruebas de JWT) y el alcance del MVP.
- Luego de haberle dado mis decisiones, modofocado los archivos de requisitos funcionales y no funcionales se presentaron 2 nuevos ajustes; no habia relacion entre RNF-05 y CA-02.4, manejos de 2 variables (page, pagesize).

**Aprendizaje:**
El agente señaló por iniciativa propia decisiones que las especificaciones no definían (comportamiento de `pageSize` fuera de rango) y una contradicción entre `CLAUDE.local.md` y el alcance del MVP.
Detectarlas antes de implementar evita corregir código después.
Desde ahora, los prompts de diseño incluirán en la Verificación: "Señala las decisiones que tomaste y que no estaban definidas en las especificaciones"

**Commit relacionado:**
d9fea13 docs(specs): requisitos no funcionales RNF-01 a RNF-10

---

## E-006 · Modelo de dominio

**Herramienta:** Claude Code (VS Code)

**Prompt Inicial:**

Contexto:
Lee la documentacion del proyecto Help Desk System que se encuentra dentro de specs/functional/

Tarea:
Propón el modelo de dominio y espera mi aprobación antes de crear el archivo.

Debe incluir:

1. Entidades con atributos y tipos (Ticket, Usuario, Comentario).
2. Enumeraciones (Estado, Prioridad, Rol, EstadoSla).
3. Relaciones y cardinalidades.
4. Qué reglas de negocio (RN-xx) vive en cada entidad y cuáles en Application. Con sus respectivos responsables
5. Diagrama de clases en Mermaid (classDiagram).
6. Diagrama de estados del ticket en Mermaid, coherente con RN-08.

Restricciones:

- EstadoSla es un valor calculado, (RN-11).
- Fechas en UTC (S-04). El Usuario necesita un indicador de activo (RN-06).
- No escribas código C#; solo el documento.

Verificación:
Señala cualquier contradicción entre requisitos y reglas de negocio.

**Resultado de la IA:**

1. Primera propuesta: 3 entidades (Usuario, Ticket, Comentario), 6 enumeraciones, ubicación de RN-01 a RN-12, diagrama de clases, diagrama de estados y 9 hallazgos. No incluía la reclasificación.
2. Segunda propuesta (tras agregar RN-13 a las especificaciones): incluyó `Ticket.Reclasificar`, la guarda común `EstaAbiertoParaCambios()`, la ubicación de RN-13 y 4 hallazgos nuevos (13 en total).
3. Ejecución:
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

## E-007 · Diagramas C4 de niveles 1 y 2

**Herramienta:** Claude Code (VS Code)

**Prompt:**

Contexto:
Help Desk System, API REST .NET 10, Clean Architecture
(Domain, Application, Infrastructure, Api), SQL Server, EF Core.
Lee specs/functional/00-vision.md, specs/functional/03-requisitos-no-funcionales.md
y specs/technical/01-modelo-dominio.md.

Tarea:
Crea specs/technical/02-arquitectura-c4.md con:

1. Nivel 1 (Contexto): los 3 roles, el sistema y sistemas externos (si existen).
2. Nivel 2 (Contenedores): Swagger UI, API REST y base de datos SQL Server,
   indicando tecnología y forma de comunicación (HTTP/JSON, encabezado
   X-Usuario-Id, EF Core).
3. Una sección con las 4 capas de Clean Architecture, qué contiene cada una
   y la regla de dependencias (qué capa puede referenciar a cuál).
   Usa Mermaid (C4Context y C4Container) con una breve explicación de cada diagrama.

Restricciones:

- No incluyas Docker, colas, caché, correo, JWT ni servicios fuera del alcance.
- Mismo encabezado (versión, fecha, estado) que los demás documentos de specs.
- No modifiques otros archivos.

Verificación:

- Confirma que la sintaxis Mermaid sea válida para que GitHub la renderice.
- Señala las decisiones que tomaste y que no estaban definidas en las especificaciones.

**Resultado de la IA:**

- Creó `specs/technical/02-arquitectura-c4.md` con el mismo encabezado que los demás documentos, sin modificar otros archivos.
- Nivel 1 (Contexto): los tres roles y el Help Desk System. No hay sistemas externos, y el documento explica por qué.
- Nivel 2 (Contenedores): Swagger UI, API REST y SQL Server, con su tecnología y comunicación (HTTP/JSON con `X-Usuario-Id`; EF Core hacia la base de datos), más una tabla resumen.
- Clean Architecture: tabla con el contenido de cada capa y sus referencias permitidas, diagrama de dependencias y regla de dependencias.
- Sintaxis Mermaid: el agente la revisó manualmente contra la documentación, pero no la ejecutó en un renderizador. Advirtió que C4 es experimental en Mermaid y recomendó confirmarlo en GitHub.

**Revisión humana**

Revisión del contenido:

| Elemento                           | Decisión | Justificación                                           |
| ---------------------------------- | -------- | ------------------------------------------------------- |
| Nivel 1 sin sistemas externos      | ✅       | Correo y JWT están fuera del alcance del MVP.           |
| Nivel 2: contenedores y protocolos | ✅       | Refleja el encabezado `X-Usuario-Id` del supuesto S-02. |
| Regla de dependencias entre capas  | ✅       | Coincide con RNF-07: Domain no depende de ninguna capa. |

Revisión de decisiones del agente no definidas en las especificaciones:

| Decisión del agente                                                                   | Decisión | Justificación                                                                                                                                                              |
| ------------------------------------------------------------------------------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Diagrama `flowchart` adicional para las capas                                         | ✅       | C4 no tiene notación para capas internas; el diagrama complementa la tabla y hace visible la regla de dependencias.                                                        |
| Swagger UI como contenedor separado                                                   | ✅       | Aunque lo sirve la propia API, separarlo deja claro cómo interactúan los usuarios con el sistema en el MVP sin frontend. Se documenta que solo está activo en Development. |
| Api → Infrastructure solo para registrar dependencias en `Program.cs`                 | ✅       | Es la única excepción aceptable en Clean Architecture: la Api arma la aplicación, pero no usa Infrastructure directamente.                                                 |
| Conversión de excepciones a ProblemDetails y lectura de `X-Usuario-Id` en la capa Api | ✅       | Son responsabilidades HTTP; mantenerlas fuera de Domain y Application conserva el negocio independiente del transporte.                                                    |
| Interfaces de persistencia en Application, implementadas en Infrastructure            | ✅       | Inversión de dependencias: Application no depende de EF Core, lo que permite probarla sin base de datos.                                                                   |
| Pruebas unitarias en Domain y Application; integración en Api                         | ✅       | Coherente con RNF-10.                                                                                                                                                      |
| Conexión a SQL Server indicada solo como "EF Core"                                    | ✅       | El protocolo de red no aporta a la comprensión del diseño en el nivel 2.                                                                                                   |

**Incidencias:**
El agente no validó los diagramas en un renderizador; solo revisó la sintaxis manualmente.

**Aprendizaje:**
Que el agente diga "la sintaxis es válida" no equivale a comprobarlo. La verificación debe hacerse en el entorno donde se leerá el documento, en este caso GitHub.

**Commit relacionado:**

- `0ab5aa4` docs(specs): arquitectura C4

---

## E-008 · Decisiones de arquitectura (ADRs)

**Herramienta:** Claude Code (VS Code)

**Prompt:**

Contexto:
Help Desk System, API REST .NET 10, Clean Architecture
(Domain, Application, Infrastructure, Api), SQL Server, EF Core.
Lee la documentacion que se encuentra dentro de specs/ .

Tarea:
Crea 4 ADR en specs/technical/adr/ con el formato de Michael Nygard
(Título, Estado, Fecha, Contexto, Decisión, Consecuencias positivas y negativas,
Alternativas consideradas):

- ADR-001-clean-architecture.md
- ADR-002-sqlserver-efcore.md
- ADR-003-fluentvalidation.md
- ADR-004-problemdetails-errores.md
  Crea también specs/technical/adr/README.md con un índice de los ADR (número, título, estado y enlace).

Restricciones:

- Cada ADR debe justificarse con requisitos concretos (RF, RN o RNF por su ID).
- Al menos 2 alternativas consideradas por ADR, con el motivo de descarte.
- Máximo 1 página por ADR. Estado: "Aceptado". Fecha: 27/10/2026.
- Los ADR deben ser coherentes con decisiones ya tomadas en el modelo de dominio y la arquitectura (SEQUENCE para el código del ticket, enums como texto, 409 desde Domain y 403 desde Application, interfaces de persistencia en Application).
- ADR-003 debe explicar cómo se reparte la validación entre FluentValidation(Application) y las invariantes del Domain, sin duplicar reglas innecesariamente.
- ADR-004 debe usar las capacidades nativas de ASP.NET Core para ProblemDetails, sin librerías externas si no son necesarias.
- No modifiques otros archivos.

Verificación:

- Lista qué requisitos justifica cada ADR.
- Señala las decisiones que tomaste y que no estaban definidas en las especificaciones.

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
| ADR-002                                                                                                            | ✏️       | Se agrega la experiencia de la desarrolladora con SQL Server como justificación. (Aplicado el 28/10/2026 en la Enmienda 2 de ADR-002.)                                        |

**Incidencias:**

- El prompt de los ADR se ejecutó dos veces: en la segunda version exigía coherencia con el modelo de dominio y un reparto claro de la validación.
- El agente detectó una contradicción entre el ADR-003 y `01-modelo-dominio.md` y la reportó en lugar de corregirla, respetando la restricción del prompt. Se alineó en un commit aparte.

**Aprendizaje:**

- Las restricciones del prompt ("no modifiques otros archivos") hicieron que el agente reportara la inconsistencia en lugar de corregirla por su cuenta, lo que permitió decidirla conscientemente.
- El agente aplicó la corrección de forma literal (solo a RN-01). Al revisar el diff se detectó que el mismo principio aplicaba a RN-09: hay que comunicar el criterio, no solo el caso concreto.

**Commits relacionados:**

- `afa2435` docs(adr): decisiones de arquitectura 001-004
- `eb0f1a1` docs(specs): alinea modelo de dominio con ADR-003
- `0b06fcb` docs(evidencias): bitácora E-009

**Nota de corrección (28/10/2026):** el commit `0b06fcb` se tituló "bitácora E-009", pero registra esta entrada; debía llamarse "bitácora E-008". No se reescribe el historial de Git (hallazgo H-06 de `docs/auditoria-documentacion.md`).

---

## E-009 · Contrato de API

**Herramienta:** Claude Code (VS Code)

**Prompt:**
Para la creación del archivo Contrato API se dividió en seis partes con la plantilla Contexto / Tarea / Restricciones; cada una terminaba con una pausa para revisión humana:

1. 5.1 Inventario y huecos (solo lectura).
2. 5.2 Convenciones comunes y ejemplos de ProblemDetails.
3. 5.3 Endpoints del Grupo A (registro, consulta y datos del ticket).
4. 5.4 Endpoints del Grupo B (asignación y ciclo de estados).
5. 5.5 Verificaciones.
6. 5.6 Cierre de H-03 y enlaces.

Cada parte, desde la 5.2 en adelante, incluía como primer paso registrar en la auditoría §9 las decisiones tomadas sobre los huecos de la parte anterior. Todas mantenían las mismas restricciones: no inventar endpoints, campos ni reglas; señalar los huecos sin resolverlos; no tocar código fuente; no hacer commit sin revisión.

**Resultado de la IA:**

- `specs/technical/03-contrato-api.md` (+1142 líneas), con:
- 13 operaciones bajo `/api/v1`.
- Convenciones comunes: rutas, X-Usuario-Id, ProblemDetails, orden de errores, paginación, fechas, enums, valores null e ids ilustrativos.
- Un elenco fijo de 6 tickets de ejemplo con un instante de referencia.
- Ejemplos de ProblemDetails para un 400 de validación y un 409.
- Sección de verificación.
- 33 decisiones nuevas (42 a 74) registradas en `docs/auditoria-documentacion.md` §9, dos términos agregados al glosario §5 y H-03 marcado como Resuelto (55/55 hallazgos).
- Ajustes en otros documentos:
- RNF-01: Swagger UI y el documento OpenAPI quedan exentos de X-Usuario-Id.
- RNF-06: la ruta de ejemplo pasa a usar el prefijo /api/v1.
- Modelo de dominio: `CambiarPrioridad(nueva)` → `CambiarPrioridad(prioridad)`.
- Matriz de permisos: la fila "Asignar o reasignar" se separa en dos.
- README.md y CLAUDE.md enlazan el contrato.
- Verificaciones finales limpias:
- 45/45 CA cubiertos.
- Matriz de permisos con 13 filas, idéntica al contrato.
- 12/12 valores de codigo del catálogo usados y ninguno fuera de él.
- 119 enlaces internos sin errores.

Operaciones del contrato:

| §   | Verbo y ruta                                |
| --- | ------------------------------------------- |
| 2.1 | POST /api/v1/tickets                        |
| 2.2 | GET /api/v1/tickets/{id}                    |
| 2.3 | GET /api/v1/tickets                         |
| 2.4 | POST /api/v1/tickets/{id}/reclasificar      |
| 2.5 | POST /api/v1/tickets/{id}/cambiar-prioridad |
| 2.6 | GET /api/v1/reportes/tickets-vencidos       |
| 3.1 | POST /api/v1/tickets/{id}/asignar           |
| 3.2 | POST /api/v1/tickets/{id}/reasignar         |
| 3.3 | GET /api/v1/tecnicos                        |
| 3.4 | POST /api/v1/tickets/{id}/iniciar-atencion  |
| 3.5 | POST /api/v1/tickets/{id}/resolver          |
| 3.6 | POST /api/v1/tickets/{id}/cerrar            |
| 3.7 | POST /api/v1/tickets/{id}/reabrir           |

**Revisión humana:**

| Elemento propuesto                                                       | Decisión (✅/✏️/❌) | Justificación                                                                                                                                                           |
| ------------------------------------------------------------------------ | ------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Asignar y reasignar como dos operaciones (13 en lugar de 12)             | ✏️                  | La decisión 8 tiene prioridad sobre el conteo de la auditoría. Se corrigió el número en la auditoría y se verificó en RN-08 el 409 de los casos cruzados (decisión 42). |
| Prefijo /api/v1, rutas en kebab-case sin tildes                          | ✅                  | Venía de la restricción del prompt, pero no estaba en ninguna fuente; se registró como decisión y se corrigió RNF-06.                                                   |
| GET /api/v1/tecnicos (solo activos) y /api/v1/reportes/tickets-vencidos  | ✅                  | Son las rutas más simples, no inventan parámetros y separan los reportes del recurso ticket.                                                                            |
| Campos del cuerpo derivados de los parámetros del modelo                 | ✏️                  | Se aprobaron y se agregó que el solicitante sale siempre de X-Usuario-Id, nunca del cuerpo (decisión 54).                                                               |
| Campos del detalle y del listado                                         | ✅                  | Coinciden con CA-02.1, CA-02.4 y las decisiones 21, 22, 27 y 49.                                                                                                        |
| Estructura del reporte (porTecnico, porPrioridad, cantidad, total)       | ✏️                  | Se aprobaron los nombres y se agregó: sin grupos con cantidad 0, orden definido y total igual a la suma de cada agrupación (decisiones 56 y 57).                        |
| "Sin asignar" como tecnico: null                                         | ✅                  | Es un dato, no un texto de presentación.                                                                                                                                |
| Enums como texto, sin distinguir mayúsculas                              | ✏️                  | Se agregó el rechazo de valores numéricos con 400, porque .NET los acepta por defecto (decisión 58). La IA no había detectado este hueco.                               |
| Desempate del listado por id descendente                                 | ✅                  | Hace la paginación estable (decisión 59).                                                                                                                               |
| Fechas ISO 8601 UTC con Z                                                | ✏️                  | Se aprobó y se agregó una nota de implementación: EF Core lee datetime2 como Unspecified y se serializaría sin Z.                                                       |
| Mensajes de errors                                                       | ✏️                  | En español y con redacción libre; se agregó que las claves van en camelCase (decisión 61).                                                                              |
| traceId en ProblemDetails; detail e instance fuera del contrato          | ✅                  | Permite correlacionar un 500 con el log (decisión 62).                                                                                                                  |
| Elenco fijo de tickets ilustrativos                                      | ✅                  | Hace coherentes todos los ejemplos; se agregó que cada ejemplo parte del estado de la tabla.                                                                            |
| Location como ruta relativa                                              | ✅                  | RFC 9110 la permite y no depende del host (decisión 64).                                                                                                                |
| {id} no válido → 404 TICKET_NO_ENCONTRADO                                | ✏️                  | Se aprobó y se agregó una nota: una restricción de ruta da un 404 sin ProblemDetails, y X-Usuario-Id debe validarse en un middleware (decisión 65).                     |
| Campos no previstos en el cuerpo se ignoran                              | ✅                  | Es el comportamiento por defecto y es coherente con la decisión 54 (decisión 66).                                                                                       |
| Mismo valor al reclasificar o cambiar prioridad → 200 sin cambios        | ✏️                  | Se registró solo después de comprobar que el modelo no lanza excepción, y se limitó a tickets modificables: Resuelto o Cerrado → 409 (decisiones 67 y 70).              |
| Orden de los 400 en asignar y reasignar, y tecnicoId no positivo         | ✅                  | VALIDACION_FALLIDA → TECNICO_NO_VALIDO → MISMO_TECNICO (decisiones 71 y 72).                                                                                            |
| Orden de técnicos activos y de comentarios                               | ✅                  | Por nombre para el selector; por fecha ascendente para el historial (decisiones 68 y 73).                                                                               |
| fechaActualizacion en cerrar y reabrir                                   | ✏️                  | Se generalizó: toda operación que modifica el ticket la actualiza, salvo la no-op; se comprobó que no contradice la decisión 16 (decisión 74).                          |
| CA-05.1 a CA-05.3 en la fila "Origen"                                    | ✅                  | Hace explícita una cobertura que ya existía; CA-05.3 también se citó en resolver y reabrir.                                                                             |
| Matriz de permisos con filas separadas para asignar y reasignar          | ✅                  | Alinea la matriz con el contrato uno a uno, sin cambiar ningún permiso.                                                                                                 |
| Actualización del resumen ejecutivo de la auditoría (fuera de lo pedido) | ✅                  | Sin ella, el resumen contradecía el cierre de H-03; la IA lo informó y ofreció revertirlo.                                                                              |
| Línea Co-Authored-By en el commit (no pedida)                            | ✅                  | Mantiene el formato de los commits anteriores.                                                                                                                          |

**Incidencias:**

- El prompt original era demasiado amplio para ejecutarse de una vez sin perder control sobre las decisiones; se dividió en seis partes con pausas de revisión.
- La IA detectó contradicciones reales entre las fuentes:
- 12 frente a 13 operaciones.
- El prefijo /api/v1, que no estaba documentado.
- Un ejemplo de RNF-06 sin prefijo.
- La inconsistencia `nueva`/`prioridad` en el modelo.
- La IA no detectó algunos huecos que aparecieron en la revisión: la serialización de enums (texto o número, y el rechazo de enteros), las claves de errors en camelCase y los riesgos de implementación (DateTimeKind.Unspecified en EF Core, el 404 sin ProblemDetails de las restricciones de ruta y el orden 401 → 404 entre middleware y filtros).
- La IA hizo dos cambios fuera del alcance: el resumen ejecutivo y Co-Authored-By. Avisó de ambos y se aceptaron.
- La primera verificación tuvo dos observaciones de trazabilidad (CA-05.x sin cita y la matriz con una fila combinada); se corrigieron antes de cerrar H-03.

**Aprendizaje:**

- Dividir un prompt grande en partes con la plantilla Contexto / Tarea / Restricciones, con una pausa de revisión al final de cada una, hizo visibles 33 decisiones que de otro modo la IA habría tomado implícitamente.
- La restricción «si encuentras un hueco, no lo resuelvas: señálalo y pregúntame» funcionó. La IA se detuvo cada vez y propuso opciones sin aplicarlas.
- Encadenar en cada prompt «registrar las decisiones de la parte anterior + siguiente tarea» ahorró vueltas sin perder la trazabilidad en la auditoría §9.
- Pedir una comprobación previa antes de registrar una decisión (por ejemplo, revisar si el modelo lanza una excepción con el mismo valor) evitó decidir sobre un supuesto falso.
- La IA es fuerte en coherencia documental (cruces, conteos, enlaces y verificación con scripts), pero pasa por alto huecos que dependen del comportamiento por defecto del framework. Esos requieren una revisión con conocimiento de .NET.
- Un elenco fijo de datos de ejemplo con un instante de referencia hace reproducibles los ejemplos que dependen del tiempo, como el estado del SLA.

**Commit relacionado:**
`84bafe0` (84bafe038870e14ca3cb3710432bee6d7e3a718e) en
`feat/specs-tecnicas` · `docs: agregar contrato de endpoints y cerrar H-03`

---

## E-010 · Matriz de trazabilidad (specs/trazabilidad.md)

**Herramienta:** Claude Code (agente en terminal, VS Code) + verificación manual con PowerShell.

**Prompt:**

Contexto: Help Desk System. Lee specs/ completo.

Tarea: Crea specs/trazabilidad.md con una tabla por cada criterio de aceptación:
RF | HU | CA | RN relacionadas | Endpoint | Test | Commit | Estado.

Restricciones:

- Las columnas Test y Commit quedan vacías (se llenarán durante la implementación).
- Estado inicial: "Especificado".
- No inventes nombres de tests.
- Usa exactamente los mismos IDs que aparecen en los specs.
- Si un CA no tiene endpoint (p. ej. es de UI o de validación interna), escribe "N/A" y explica por qué en una sección "Observaciones".

Verificación:

Confirma que todos los CA de 01-requisitos-funcionales.md aparecen y que ningún endpoint de 03-contrato-api.md queda sin CA.

Prompt de corrección posterior:

> Las "decisiones N" citadas en specs/trazabilidad.md están definidas en la sección 9
> de docs/auditoria-documentacion.md. Enlaza cada mención a esa sección con la ruta
> relativa correcta y agrega una línea al inicio indicando dónde están las decisiones.
> No cambies nada más.

**Resultado de la IA:**

- Creó `specs/trazabilidad.md` con 45 filas (una por CA), agrupadas en una tabla por RF (RF-01 a RF-06).
- Test y Commit vacíos; Estado "Especificado" en todas las filas; IDs idénticos a los specs (HU-01 a HU-14, CA-01.1 a CA-06.3, RN-01 a RN-13).
- Rutas sin el prefijo `/api/v1`, aclarado al inicio del documento.
- CA que cubren varios endpoints o HU los listan todos (p. ej. CA-04.6 con las cuatro transiciones).
- Sección "Observaciones": justifica los 5 CA sin RN (CA-01.5, CA-02.2, CA-02.8, CA-03.7, CA-06.3) y los CA de SLA (CA-05.1 a CA-05.3), que se verifican por `fechaVencimiento` y `estadoSla`.
- Sección "Verificación" con una tabla inversa endpoint → CA (13 operaciones).
- Reportó 45/45 CA, 13/13 endpoints con CA, 13/13 RN usadas y enlaces internos sin errores, comprobado con un script propio.
- Interpretó "una tabla por cada CA" como una fila por CA dentro de una tabla por RF, y lo advirtió explícitamente.

**Revisión humana:**

| Elemento propuesto                                   | Decisión (✅/✏️/❌) | Justificación                                                                                                        |
| ---------------------------------------------------- | ------------------- | -------------------------------------------------------------------------------------------------------------------- |
| Estructura: una tabla por RF, una fila por CA        | ✅                  | 45 tablas de una sola fila serían ilegibles; la interpretación conserva la granularidad por CA.                      |
| Columnas Test y Commit vacías, Estado "Especificado" | ✅                  | Cumple las restricciones del prompt; no se inventaron nombres de tests.                                              |
| Cobertura de CA (45/45)                              | ✅                  | Verificado de forma independiente con `Select-String` + `Compare-Object`: 45 en spec, 45 en matriz, sin diferencias. |
| Cobertura de endpoints (13/13)                       | ✅                  | Verificado de forma independiente; ningún endpoint de la matriz es inventado (sin filas `=>`).                       |
| Rutas sin prefijo `/api/v1`                          | ✅                  | Documentado al inicio; mejora la legibilidad. Se tuvo en cuenta al ajustar el script de verificación.                |
| CA sin RN marcados con "—" y justificados            | ✅                  | Cada uno se explica por su origen real (supuestos, RNF, matriz de permisos, decisiones).                             |
| CA-05.1 a CA-05.3 sin operación propia               | ✅                  | Correcto: el SLA es lógica de dominio observable en las respuestas. Requerirá tests de dominio además de los de API. |
| CA-01.5 (401) trazado solo a `POST /tickets`         | ✅                  | Aceptado para no inflar la matriz; se cubrirá con un test de autenticación transversal.                              |
| Referencias a "decisión N" sin enlace                | ✏️                  | No eran rastreables: las decisiones están en la sección 9 de `docs/auditoria-documentacion.md`. Se pidió enlazarlas. |
| Estado del documento "Borrador para revisión"        | ✏️                  | Cambiado a "Vigente" tras la revisión.                                                                               |

**Incidencias:**

1. Al pegar el script de verificación en la terminal de VS Code se perdieron los saltos de línea y PowerShell interpretó `-Unique$matriz` como un parámetro. Se resolvió separando las instrucciones con `;`.
2. La ruta del contrato estaba mal en el comando (`specs/functional/` en lugar de `specs/technical/03-contrato-api.md`); se localizó con `Get-ChildItem specs -Recurse -Filter *.md`.
3. La primera comparación de endpoints dio 24 frente a 13: el patrón capturaba también los ejemplos de peticiones del contrato (`/tickets/14`, `?page=1&pageSize=20`). Se filtraron las rutas con IDs numéricos o query string, obteniendo 13/13.
4. Se hizo commit de la matriz antes de aplicar las correcciones de la revisión; las correcciones se registraron en un segundo commit.

**Aprendizaje:**

- La verificación que reporta el agente no basta: hay que repetirla de forma independiente con comandos propios.
- Una comparación sin diferencias no prueba nada si ambos lados están vacíos; siempre hay que mostrar los conteos.
- Los ejemplos incluidos en los specs afectan a las verificaciones por expresiones regulares; los patrones deben distinguir plantillas (`{id}`) de ejemplos concretos.
- Las referencias cruzadas (como "decisión N") deben enlazarse a su fuente para que la trazabilidad sea real.
- Revisar antes de hacer commit evita commits de corrección.

**Commit relacionado:**

- `459cce7 ` docs(specs): agrega matriz de trazabilidad (45 CA, 13 endpoints)
- `e2646c1 ` docs(specs): enlaza decisiones de la auditoría y marca trazabilidad como vigente

---

## E-011 · Auditoría de la documentación (docs/auditoria-documentacion.md)

**Herramienta:** Claude Code (VS Code)

**Prompt:**
La auditoría se hizo antes del contrato de endpoints, en cuatro fases de solo lectura y dos partes de corrección, con una pausa de revisión humana al final de cada una:

1. Fase 1: inventario de los 15 documentos y validación de enlaces (§1).
2. Fase 2: contradicciones entre documentos (§2).
3. Fase 3: definiciones incompletas, ambigüedades y glosario propuesto (§3 a §5).
4. Fase 4: matriz de trazabilidad, suficiencia de información por operación y preguntas abiertas (§6 a §8).
5. Corrección parte 1: aplicar las decisiones 1 a 20 sobre las preguntas de §8 y responder las preguntas P-1 a P-5 (§11, decisiones 21 a 27).
6. Corrección parte 2: limpieza transversal, respuestas a las preguntas Q-1 a Q-10 (§12, decisiones 28 a 38), aprobación del catálogo de `codigo` (decisiones 39 a 41) y revalidación final (§13).

**Resultado de la IA:**

- `docs/auditoria-documentacion.md` con 55 hallazgos (2 críticos, 6 altos, 26 medios y 21 bajos) y veredicto inicial **NO LISTA** para el contrato de endpoints.
- 20 preguntas abiertas en §8, con opciones presentadas como sugerencias y no como recomendaciones.
- Corrección de las especificaciones funcionales, el modelo de dominio, la arquitectura C4, los ADR (mediante enmiendas), la bitácora, `README.md`, `CLAUDE.md` y `.gitignore`, según las 41 decisiones de §9.
- HU-10 a HU-14 agregadas (H-53): 14 HU y 45 CA en total.
- §10 con el estado de los 55 hallazgos: 55 resueltos y 0 pendientes. H-41 y H-43 se reclasificaron de Bajo a Alto.
- §13 con la revalidación final: 94 enlaces internos y 0 rotos, sin contradicciones nuevas y las 13 operaciones con información suficiente para el contrato. Veredicto final **LISTA**.

**Revisión humana:**

| Elemento propuesto                                                          | Decisión (✅/✏️/❌) | Justificación                                                                                                            |
| --------------------------------------------------------------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| Pregunta 1: detalle completo y listado resumido (opción B)                  | ✏️                  | Se eligió B, con técnico y solicitante como `{ id, nombre }` (decisión 1); se completó con P-1, P-2 y el `tipo` del comentario (decisiones 21, 22 y 27). |
| Pregunta 2: `200` con el ticket actualizado (opción A)                      | ✅                  | Crear responde `201` con `Location`; las acciones, `200` con el detalle; las consultas, `200` (decisión 2).               |
| Pregunta 3: solo el solicitante crea tickets (opción A)                     | ✅                  | Técnico o supervisor → `403` (decisión 3).                                                                               |
| Pregunta 4: técnico inexistente al asignar                                  | ✅                  | `400`, como dice CA-03.3; `404` solo para el recurso de la ruta (decisión 4).                                            |
| Pregunta 5: dos excepciones de dominio (`409` y `500`)                      | ✅                  | `TransicionInvalidaException` → `409` e `InvarianteVioladaException` → `500` (decisión 5).                              |
| Pregunta 6: precedencia `401` → `404` → `403` → `400` → `409` (opción A)    | ✅                  | Decisión 6.                                                                                                               |
| Pregunta 7: actor no autorizado → `403` (opción A)                          | ✅                  | Decisión 7.                                                                                                               |
| Pregunta 8: una operación por acción (opción A)                             | ✅                  | Decisión 8.                                                                                                               |
| Pregunta 9: `page` = 1 y `pageSize` = 20 (opción B)                         | ✏️                  | Se eligió B y se subió el máximo de `pageSize` de 50 a 100, con la respuesta `items`, `page`, `pageSize` y `totalItems` (decisiones 9 y 26). |
| Pregunta 10: filtros                                                        | ✅                  | `soloVencidos` no incluye `Incumplido`; técnico que filtra por otro `tecnicoId` → `403`; filtro inválido → `400` (decisión 10). |
| Pregunta 11: dos agrupaciones separadas en el reporte (opción A)            | ✅                  | Decisión 11.                                                                                                              |
| Pregunta 12: consulta de técnicos activos para el supervisor (opción A)     | ✅                  | Decisión 12; P-4: lista de `{ id, nombre }` sin paginar (decisión 24).                                                   |
| Pregunta 13: se aceptan huecos en el código (opción A)                      | ✅                  | Código "único y creciente" (decisión 13).                                                                                |
| Pregunta 14: reasignar al mismo técnico → `400` (opción A)                  | ✅                  | Decisión 14.                                                                                                              |
| Pregunta 15: `400` en los CA de error faltantes                             | ✅                  | Confirmado para reabrir sin motivo, textos de más de 2000 caracteres y prioridad inexistente (decisión 15).              |
| Pregunta 16: auditoría de cambios                                           | ✏️                  | Ninguna opción tal cual: el ticket guarda `FechaCreacion` y `FechaActualizacion` y el historial queda fuera del MVP (decisión 16); al crear, ambas fechas son iguales (P-5, decisión 25). |
| Pregunta 17: concurrencia fuera del MVP (opción B)                          | ✅                  | Limitación conocida (decisión 17).                                                                                       |
| Pregunta 18: el MVP no elimina tickets                                      | ✅                  | Decisión 18.                                                                                                              |
| Pregunta 19: datos semilla                                                  | ✅                  | 5 solicitantes, 4 técnicos (uno inactivo) y 1 supervisor (decisión 19).                                                  |
| Pregunta 20: el commit `0b06fcb` debía llamarse E-008 (opción B)            | ✅                  | Nota en E-008 sin reescribir el historial (decisión 20).                                                                 |
| P-1 a P-5: preguntas surgidas en la corrección parte 1                      | ✅                  | Respondidas con las decisiones 21 a 25; P-3: reclasificar responde `200` con el detalle (decisión 23).                   |
| H-20: la Api obtiene el usuario a través de Application                     | ✅                  | Confirmado (decisión 28).                                                                                                |
| Q-1: Problem Details por defecto más el campo `codigo` (opción C)           | ✅                  | Decisión 29.                                                                                                              |
| Q-2: `X-Usuario-Id` entero positivo de `int` (opción A)                     | ✅                  | Cualquier otro valor → `401` (decisión 30).                                                                              |
| Q-3: recortar los textos antes de validar (opción A)                        | ✅                  | Decisión 31.                                                                                                              |
| Q-4: las migraciones de EF Core como fuente del modelo físico (opción B)    | ✅                  | Los 10 usuarios semilla se documentan con nombres y correos ficticios (decisión 32).                                     |
| Q-5: índice único en `Usuario.Email` (opción A)                             | ✅                  | Decisión 33.                                                                                                              |
| Q-6: constante de paginación en Application (opción A)                      | ✅                  | Decisión 34.                                                                                                              |
| Q-7: código desde `HD-000001` con límite `HD-999999` (opción A)             | ✅                  | Limitación conocida del MVP (decisión 35).                                                                               |
| Q-8: criterios de prueba                                                    | ✅                  | Prueba de arquitectura obligatoria, claves `CA` y `RN` en `[Trait]` y LocalDB (decisión 36).                             |
| Q-9: justificación de ADR-002                                               | ✏️                  | La redactó la desarrolladora: más de 10 años de experiencia con .NET y SQL Server (decisión 37).                         |
| Q-10: eliminar las líneas de la SPA del `.gitignore` (opción A)             | ✅                  | Decisión 38.                                                                                                              |
| Catálogo de `codigo` de ADR-004                                             | ✅                  | Aprobado tal como se propuso, con los códigos separados (decisión 39).                                                   |
| Propiedad `Codigo` en las excepciones de dominio                            | ✅                  | Propuesta de la IA que complementa el catálogo: `TransicionInvalidaException` lleva `TRANSICION_INVALIDA` o `TICKET_NO_MODIFICABLE`, y en los `500` la Api responde siempre `ERROR_INTERNO` y registra el detalle en el log (decisiones 40 y 41). |
| Enlaces y glosario en `02-reglas-negocio.md` durante la corrección parte 1  | ❌                  | Correspondían a la parte 2; se revirtieron.                                                                              |
| Script de validación de enlaces que revisaba el código entre comillas invertidas | ❌              | Daba falsos positivos; se ajustó para ignorar ese código.                                                                |
| Notas no pedidas (una prueba de precedencia en RNF-04 y una frase extra en el resumen ejecutivo) | ❌ | Estaban fuera de lo pedido; se quitaron.                                                                                 |
| Cita a "RNF-09"                                                             | ❌                  | La regla correcta es RN-09; se corrigió la cita.                                                                         |

**Incidencias:**

- El veredicto inicial fue **NO LISTA**: la auditoría encontró 55 hallazgos, 2 de ellos críticos.
- Aplicar las decisiones generó preguntas nuevas: P-1 a P-5 en la parte 1 (§11) y Q-1 a Q-10 en la parte 2 (§12), que requirieron rondas adicionales de decisión.
- H-41 y H-43 se reclasificaron de Bajo a Alto en la parte 2, porque el contrato necesita su definición; H-31 subió a Crítico en la fase 4.
- La decisión 9 cambió un valor ya documentado: el máximo de `pageSize` pasó de 50 a 100 en RN-05, CA-02.4, RNF-05 y ADR-003.
- El commit `0b06fcb` tenía el nombre de entrada equivocado (E-009 en lugar de E-008); se corrigió con una nota, sin reescribir el historial (H-06).
- El cuerpo de los ADR conserva menciones que sus enmiendas corrigen ("consecutivo", "LocalDB o instancia local", el rango 1–50); se dejaron como registro, según §13.2.
- En la corrección parte 1, la IA aplicó en `02-reglas-negocio.md` enlaces y glosario que correspondían a la parte 2; se revirtieron.
- El script de validación de enlaces detectaba falsos positivos dentro del código entre comillas invertidas; se ajustó para ignorarlo.
- La IA agregó notas que no se habían pedido (una prueba de precedencia en RNF-04 y una frase extra en el resumen ejecutivo); se quitaron.
- Una cita decía "RNF-09" cuando debía decir "RN-09"; se corrigió.

**Aprendizaje:**

- Auditar en fases de solo lectura antes de corregir separó el diagnóstico de las decisiones; la IA presentó opciones como sugerencias y no las aplicó.
- Corregir genera preguntas nuevas: hay que prever más de una ronda de decisión antes de declarar la documentación lista.
- Los ADR y los registros históricos se corrigen con enmiendas y notas, no reescribiéndolos, para conservar la trazabilidad.
- Una revalidación final con conteos (enlaces, hallazgos y operaciones) da evidencia objetiva para cambiar el veredicto.

**Commit relacionado:**

- `fb3a977` docs: agregar auditoría de documentación (fases 1-4, veredicto NO LISTA)
- `801b2ff` docs: aplicar decisiones de la auditoría (parte 1)
- `9060679` docs: limpieza transversal y revalidación de la auditoría (parte 2)
- `4dc7c16` docs: cerrar auditoría de documentación, veredicto LISTA
- `38ec0bd` docs: actualizar CLAUDE.md, README y .gitignore según la auditoría

---

## E-012 · Pull request de especificaciones técnicas (PR #1)

**Herramienta:** Claude Code (VS Code) + MCP de GitHub (`create_pull_request`)

**Prompt:**

> **Contexto**
> Estoy desarrollando un Help Desk System (API en .NET 10) como proyecto integrador. Terminé la fase de especificación técnica en la rama `feat/specs-tecnicas`, donde ya hice commit y push de todos los documentos. Tengo configurado el MCP de GitHub.
>
> **Tarea**
> Usando el MCP de GitHub, crea un pull request de `feat/specs-tecnicas` hacia `main` en mi repositorio.
>
> - Título: "Especificaciones técnicas del Help Desk System"
> - Descripción:
>   - Lista los documentos de especificación, agrupados por carpeta, indicando cuáles son nuevos o modificados en este PR y cuáles ya existían en `main`:
>     - **specs**: `trazabilidad.md`
>     - **specs/functional**: `01-requisitos-funcionales.md`, `02-reglas-negocio.md`, `03-requisitos-no-funcionales.md`
>     - **specs/technical**: `01-modelo-dominio.md`, `02-arquitectura-c4.md`, `03-contrato-api.md`
>     - **specs/technical/adr**: `ADR-001-clean-architecture.md`, `ADR-002-sqlserver-efcore.md`, `ADR-003-fluentvalidation.md`, `ADR-004-problemdetails-errores.md`
>   - Usa la ruta completa real de cada archivo según la rama.
>   - Menciona que la implementación se construirá sobre el modelo de dominio y el contrato de API.
>
> **Restricciones**
> - No fusiones el PR; solo créalo.
> - Antes de crearlo, muéstrame el repositorio, la rama de origen, la rama de destino, el título y la descripción, y espera mi confirmación.
> - Verifica que todos los archivos listados existan en la rama `feat/specs-tecnicas`; si falta alguno, avísame en lugar de crear el PR.
> - No modifiques ningún archivo ni hagas commits nuevos.
>
> **Verificación**
> - Enlace al PR creado.
> - Lista de commits incluidos en el PR.
> - Lista de archivos modificados en el PR, con su ruta completa.

Mensaje posterior, tras modificar la bitácora antes de confirmar:

> Antes de crear el PR, hice commit y push de cambios en docs/evidencias/bitacora.md. Vuelve a revisar la rama origin/feat/specs-tecnicas, actualiza el número de commits y la lista de archivos si cambiaron, y muéstrame los datos finales. Si todo coincide, crea el PR.

**Resultado de la IA:**

- Verificó con `git fetch` y `git diff --name-status origin/main...origin/feat/specs-tecnicas` que los 11 archivos listados existían en la rama.
- Distinguió los archivos nuevos de los modificados: 9 nuevos en este PR y 2 que ya existían en `main` y se modifican (`specs/functional/01-requisitos-funcionales.md` y `specs/functional/02-reglas-negocio.md`).
- Detectó 8 archivos del PR que no estaban en la lista: `specs/functional/00-vision.md`, `specs/technical/adr/README.md`, `docs/auditoria-documentacion.md`, `docs/evidencias/bitacora.md`, `README.md`, `CLAUDE.md`, `.gitignore` y `.mcp.json`. Los agregó en una sección aparte de la descripción.
- Mostró el repositorio, las ramas, el título y la descripción, y esperó la confirmación antes de crear el PR.
- Tras el commit `2965cab` de la bitácora, volvió a revisar la rama: los commits pasaron de 17 a 18 y los archivos siguieron siendo 19, por lo que la descripción no cambió.
- Creó el PR #1 sin fusionarlo y entregó el enlace, los 18 commits (`f3eba20` a `2965cab`) y los 19 archivos con su ruta completa.

**Revisión humana:**

| Elemento propuesto                                                    | Decisión (✅/✏️/❌) | Justificación                                                                  |
| --------------------------------------------------------------------- | ------------------- | ------------------------------------------------------------------------------ |
| Repositorio `AngelicaTomala/help-desk-system`, `feat/specs-tecnicas` → `main` | ✅          | Confirmado al pedir la creación del PR con los datos finales.                  |
| Título "Especificaciones técnicas del Help Desk System"               | ✅                  | Es el título indicado en el prompt.                                            |
| Lista de documentos agrupada por carpeta, con la ruta completa        | ✅                  | Confirmada al pedir la creación del PR con los datos finales.                  |
| Marcado de archivos nuevos y modificados (ya existentes en `main`)    | ✅                  | Confirmado al pedir la creación del PR con los datos finales.                  |
| Sección "Otros archivos incluidos en el PR" con los 8 no listados     | ✅                  | Confirmada al pedir la creación del PR con los datos finales.                  |
| Cambio en `.mcp.json` (solo de formato) incluido en el PR             | ✅                  | Revisado: no contiene secretos; es configuración del proyecto.                 |
| Línea "Generated with Claude Code" al final de la descripción         | ✅                  | Se mantiene por transparencia sobre el uso del agente.                         |

**Incidencias:**

1. La bitácora se modificó antes de confirmar el PR; se hizo commit (`2965cab`) y push, y la IA volvió a revisar la rama antes de crearlo.
2. Al registrar la E-011, la IA reportó dos veces conteos incorrectos de la tabla de revisión (6 y luego 5 filas ✏️). Al pedir verificación, recontó y confirmó 30 ✅, 4 ✏️ y 4 ❌. El archivo era correcto, pero los resúmenes no.

**Aprendizaje:**

- Un PR solo muestra las diferencias con la rama destino, así que conviene pedir que se distingan los archivos nuevos, los modificados y los no listados.
- Los resúmenes del agente no sustituyen la verificación del archivo.

**Commit relacionado:**

- PR #1: https://github.com/AngelicaTomala/help-desk-system/pull/1
- `2965cab` docs(bitacora): actualización de la bitácora
