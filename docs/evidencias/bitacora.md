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

**Insumos definidos por la desarrolladora:**

- Tema del proyecto: API REST para gestionar el ciclo de vida de los tickets de soporte de TI.
- Roles: Solicitante, Técnico y Supervisor.
- SLA por prioridad: Crítica 2 h, Alta 4 h, Media 8 h, Baja 24 h.
- Estados: Abierto → Asignado → En progreso → Resuelto → Cerrado, con Reabrir desde Resuelto.

**Resultado de la IA:**
Borradores de `00-vision.md`, `01-requisitos-funcionales.md`
(6 RF, 8 historias, 31 criterios de aceptación) y `02-reglas-negocio.md` (12 reglas y
diagrama de estados), ubicados en `specs/functional/`.

**Revisión humana:**

| Elemento propuesto                                     | Decisión | Justificación                                                                                                 |
| ------------------------------------------------------ | -------- | ------------------------------------------------------------------------------------------------------------- |
| SLA propuesto de 4 / 8 / 24 / 72 h (RN-10)             | ✏️       | Se ajustó a 2 / 4 / 8 / 24 h porque en un área de producción una falla crítica no puede esperar 4 horas.                 |
| Comentario obligatorio para resolver el ticket (RN-09) | ✅       | Se podrá llevar una trazabilidad de las actividades que realizó el técnico.                                   |
| Reasignar devuelve el ticket a `Asignado`              | ✅       | Se considera que debe quedar asignado porque el ticket pasa a un nuevo técnico.                               |
| Reabrir vuelve a `En progreso` con el mismo técnico    | ✅       | Estoy de acuerdo con que sea el mismo técnico porque conoce el problema y puede realizar una rápida revisión. |
| Cambio de prioridad recalcula el SLA (RN-12)           | ✅       | Si se cambia la prioridad, la fecha de vencimiento también debe recalcularse en base a la nueva prioridad.    |
| Categorías fijas (RN-02)                               | ✅       | Estoy de acuerdo que sean fijas porque solo se van a manejar 5 categorias.                                    |

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
