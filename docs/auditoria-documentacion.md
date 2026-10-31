# Auditoría de la documentación

| Campo   | Valor                                  |
| ------- | -------------------------------------- |
| Versión | 1.4                                    |
| Fecha   | 30/10/2026                             |
| Estado  | Auditoría y corrección completas       |
| Rama    | `feat/specs-tecnicas` (auditoría en `fb3a977`; corrección parte 1 en `801b2ff`; parte 2 en `9060679`) |

## Resumen ejecutivo

Se auditaron los 15 documentos del repositorio en cuatro fases (secciones 1 a 8) y se corrigieron con las decisiones de la sección 9 (108 en total). El estado de cada hallazgo está en la sección 10 y la revalidación en la sección 13.

| Severidad | Total | Resueltos | Pendientes |
| --------- | ----- | --------- | ---------- |
| Crítico   | 2     | 2         | 0          |
| Alto      | 8     | 8         | 0          |
| Medio     | 26    | 26        | 0          |
| Bajo      | 19    | 19        | 0          |
| **Total** | **55** | **55**   | **0**      |

H-41 y H-43 se reclasificaron de Bajo a Alto en la parte 2 porque el contrato necesita su definición. Los totales originales por severidad eran: Crítico 2, Alto 6, Medio 26, Bajo 21.

### Veredicto: **LISTA** para generar el contrato de endpoints

No quedan hallazgos críticos ni bloqueantes. Las 13 operaciones (las 11 originales, con asignar y reasignar separadas según la decisión 42, más la consulta de técnicos activos) tienen request, response, validaciones, códigos de error y de éxito, y permisos definidos (sección 13.3). H-03 quedó resuelto con el [contrato de endpoints](../specs/technical/03-contrato-api.md).

El catálogo de `codigo` de [ADR-004](../specs/technical/adr/ADR-004-problemdetails-errores.md#enmienda-3--28102026) está aprobado (decisión 39), y las excepciones de dominio exponen su `Codigo` (decisión 40, [Enmienda 4](../specs/technical/adr/ADR-004-problemdetails-errores.md#enmienda-4--28102026)).

Escala de severidad:

- **Crítico:** impide avanzar al contrato de endpoints o deja una regla sin fuente válida.
- **Alto:** información incorrecta o ausente en un documento que el equipo o el agente usan como referencia.
- **Medio:** trazabilidad incompleta, documento huérfano o referencia a algo inexistente.
- **Bajo:** inconsistencia menor de nombres, formato o navegación.

---

## 1. Inventario y enlaces

### 1.1 Inventario

Se revisaron todos los archivos `.md` del repositorio, excluida la carpeta `node_modules/` (dependencias de Playwright, no son documentación del proyecto). También se revisó `.gitignore` por las rutas que menciona.

| #  | Archivo                                              | Qué cubre                                                                                          |
| -- | ---------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| 1  | `README.md`                                          | Título del repositorio y una frase de descripción.                                                 |
| 2  | `CLAUDE.md`                                          | Instrucciones para el agente: estado, stack, comandos, reglas transversales, trazabilidad y Git.  |
| 3  | `CLAUDE.local.md` *(no versionado, `.gitignore` l.492)* | Preferencias locales: puerto 3001, pruebas de JWT a futuro, ejecutar pruebas tras cada cambio.   |
| 4  | `specs/functional/00-vision.md`                      | Problema, objetivo, roles, alcance del MVP (RF-01 a RF-06), supuestos S-01 a S-04, stack y glosario. |
| 5  | `specs/functional/01-requisitos-funcionales.md`      | RF-01 a RF-06 con 9 HU y 36 CA (Dado/Cuando/Entonces).                                             |
| 6  | `specs/functional/02-reglas-negocio.md`              | RN-01 a RN-13, tabla y diagrama de estados (RN-08), tabla de SLA y matriz RN → RF.                 |
| 7  | `specs/functional/03-requisitos-no-funcionales.md`   | RNF-01 a RNF-10 con criterio de verificación y resumen por categoría.                              |
| 8  | `specs/technical/01-modelo-dominio.md`               | Entidades, enums, relaciones, ubicación de RN por capa, diagramas de clases y estados, 13 hallazgos con decisión. |
| 9  | `specs/technical/02-arquitectura-c4.md`              | C4 nivel 1 y 2, contenedores y capas de Clean Architecture con su regla de dependencias.           |
| 10 | `specs/technical/adr/README.md`                      | Índice de ADR-001 a ADR-004 con estado, fecha y enlace.                                            |
| 11 | `specs/technical/adr/ADR-001-clean-architecture.md`  | Decisión de usar cuatro proyectos de Clean Architecture.                                           |
| 12 | `specs/technical/adr/ADR-002-sqlserver-efcore.md`    | Decisión de usar SQL Server con EF Core (SEQUENCE, índices, `datetime2`, migraciones).             |
| 13 | `specs/technical/adr/ADR-003-fluentvalidation.md`    | Reparto de la validación entre FluentValidation e invariantes de Domain.                           |
| 14 | `specs/technical/adr/ADR-004-problemdetails-errores.md` | Mapeo de excepciones a Problem Details nativo de ASP.NET Core.                                  |
| 15 | `docs/evidencias/bitacora.md`                        | Entradas E-001 a E-008 de interacciones con la IA y decisiones humanas.                            |

Documentos que se pidió buscar y **no existen** como archivo propio: casos de uso (solo existen como HU en `01-requisitos-funcionales.md`), modelo de datos físico y contrato de endpoints (ver H-03 y H-12).

### 1.2 Validación de enlaces

| Verificación                       | Resultado                                                                                                                      |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Enlaces Markdown `[texto](ruta)`   | 4, todos en `specs/technical/adr/README.md` (l.7–10). Los 4 destinos existen.                                                  |
| Anclas (`#heading`)                | No hay ninguna ancla en la documentación.                                                                                      |
| Referencias en texto plano a archivos | Revisadas en los 15 documentos (ver 1.3). Todas apuntan a archivos existentes salvo las de H-09 y H-10. Ninguna es un enlace navegable (H-13).          |
| Referencias a secciones            | `02-arquitectura-c4.md` §4 → `00-vision.md` "sección 4" existe; ADR-001 l.14 → `02-arquitectura-c4.md` "sección 3" existe.   |
| Referencias a IDs                  | Una referencia a un ID inexistente: RNF-11 (H-07).                                                                             |

### 1.3 Matriz de referencias entrantes

| Documento                         | Lo referencian                                                       | ¿Huérfano?            |
| --------------------------------- | -------------------------------------------------------------------- | --------------------- |
| `README.md`                       | —                                                                    | Raíz (no aplica)      |
| `CLAUDE.md`                       | `bitacora.md` (E-003, l.69)                                          | No                    |
| `00-vision.md`                    | `CLAUDE.md` l.9, `03-RNF` l.14, `02-arquitectura-c4` l.9 y l.101, `01-modelo-dominio` l.9 | No |
| `01-requisitos-funcionales.md`    | `CLAUDE.md` l.10, `02-reglas-negocio` l.3, `01-modelo-dominio` l.9   | No                    |
| `02-reglas-negocio.md`            | `CLAUDE.md` l.11, `01-requisitos-funcionales` l.8, `01-modelo-dominio` l.9 | No              |
| `03-requisitos-no-funcionales.md` | `02-arquitectura-c4` l.9, `01-modelo-dominio` l.9                    | No (pero no en `CLAUDE.md`) |
| `01-modelo-dominio.md`            | `02-arquitectura-c4` l.9                                             | No (pero no en `CLAUDE.md`) |
| `02-arquitectura-c4.md`           | `ADR-001` l.14                                                       | No (pero no en `CLAUDE.md`) |
| `adr/README.md`                   | —                                                                    | **Sí** (H-04)         |
| `ADR-001` a `ADR-004`             | `adr/README.md` y referencias cruzadas entre ADR                     | No                    |
| `bitacora.md`                     | `CLAUDE.md` l.52                                                     | No                    |

### 1.4 Hallazgos

No se encontraron hallazgos de severidad **Crítico**.

| ID   | Severidad | Archivo y ubicación                                                                 | Descripción                                                                                                                                                                                                                       | Propuesta de corrección                                                                                                                         |
| ---- | --------- | ----------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| H-01 | Alto      | `CLAUDE.md` · §Estado del proyecto, l.7–11                                          | Dice que `specs/technical/` "aún no tiene contenido" y que la fuente de verdad es solo `specs/functional/`. Hoy existen `01-modelo-dominio.md`, `02-arquitectura-c4.md` y 4 ADR, y no lista `03-requisitos-no-funcionales.md`. | Actualizar la sección: listar los 4 documentos funcionales, los 2 técnicos y el índice de ADR, y definir qué documento prevalece ante conflicto. |
| H-02 | Alto      | `CLAUDE.md` · §Git, l.57                                                            | Afirma que "la carpeta todavía no está inicializada como repositorio git"; el repositorio existe con remoto `origin` y 12 commits.                                                                                                | Eliminar la línea o reemplazarla por el flujo actual (rama `feat/*`, PR a `main`).                                                             |
| H-03 | Alto      | `03-requisitos-no-funcionales.md` · RNF-06, l.25 · `CLAUDE.md` l.51 · `00-vision.md` §7, l.76 | RNF-06 cita el endpoint `GET /tickets` y dos documentos exigen trazar cada RF hasta su endpoint, pero ningún documento define rutas, verbos ni contratos.                                                                         | Crear el contrato de endpoints (siguiente entregable) y referenciarlo desde `CLAUDE.md`.                                                       |
| H-04 | Medio     | `specs/technical/adr/README.md`                                                     | Documento huérfano: ningún archivo lo enlaza ni lo menciona.                                                                                                                                                                     | Enlazarlo desde `README.md` y `CLAUDE.md`.                                                                                                     |
| H-05 | Medio     | `README.md` · l.1–2 · `bitacora.md` E-008, l.498                                     | El README no describe stack, estructura, requisitos previos ni enlaza a `specs/` o `docs/`. La bitácora indica que el uso de LocalDB "se documentará en el README", y no está.                                                    | Ampliar el README: propósito, stack, estructura de carpetas, índice de documentos y requisitos de ejecución (LocalDB).                         |
| H-06 | Medio     | `bitacora.md` · última entrada E-008, l.387                                         | El commit `0b06fcb` se titula "docs(evidencias): bitácora E-009", pero la bitácora no contiene ninguna entrada E-009.                                                                                                            | Confirmar si falta la entrada E-009 o si el mensaje del commit debía decir E-008, y registrarlo en la bitácora.                                 |
| H-07 | Medio     | `bitacora.md` · E-007, revisión humana, l.364                                       | La justificación cita "RNF-10 y RNF-11 (cobertura en Domain y Application)". RNF-11 no existe: `03-requisitos-no-funcionales.md` define RNF-01 a RNF-10 (l.20–29).                                                               | Corregir la referencia a RNF-10 o anotar que RNF-11 fue eliminado.                                                                             |
| H-08 | Medio     | `CLAUDE.md` · §Trazabilidad, l.52                                                   | Menciona "la plantilla `E-NNN`", pero ningún archivo la define; las entradas de la bitácora usan secciones distintas entre sí.                                                                                                   | Añadir la plantilla al inicio de `bitacora.md` o en un archivo propio y enlazarla.                                                             |
| H-09 | Medio     | `bitacora.md` · E-001 l.25, E-002 l.59, E-003 l.89                                   | "Commit relacionado: (pendiente)". Según E-004 (l.128 y l.130), el contenido de E-002 y E-003 entró en el commit `61b123f`.                                                                                                       | Completar los hashes o indicar que no hubo commit asociado.                                                                                    |
| H-10 | Bajo      | `bitacora.md` · E-001, l.17                                                         | Lista las carpetas `tests/` y `evidencias/`; en el repositorio existen `test/` (como dice `CLAUDE.md` l.7) y `docs/evidencias/`.                                                                                                | Unificar el nombre de la carpeta de pruebas y corregir la ruta de evidencias.                                                                  |
| H-11 | Bajo      | `bitacora.md` · E-004, prompt, l.100                                                | El prompt transcrito menciona `evidencia/bitacora.md`, ruta inexistente (la real es `docs/evidencias/bitacora.md`).                                                                                                              | Mantener la transcripción literal y añadir una nota con la ruta correcta.                                                                      |
| H-12 | Medio     | `01-modelo-dominio.md` · §1–3 · `ADR-002` §Decisión, l.16–20                         | No hay modelo de datos físico: nombres de tablas y columnas, longitudes en BD, nombre de la SEQUENCE, comportamiento de las FK ni datos semilla concretos (usuarios y roles a cargar, S-01).                                       | Documentar el esquema físico y el set de datos semilla antes de las migraciones.                                                              |
| H-13 | Bajo      | `01-requisitos-funcionales.md` l.8 · `02-arquitectura-c4.md` l.9 y l.101 · `ADR-001` l.14 · `01-modelo-dominio.md` l.9 | Todas las referencias entre documentos son texto plano, no enlaces. En `ADR-001` l.14, `02-arquitectura-c4.md` es relativo a `specs/technical/`, no a la carpeta `adr/` donde está el ADR. | Convertirlas en enlaces relativos navegables (p. ej., `[02-arquitectura-c4.md](../02-arquitectura-c4.md#3-clean-architecture-organización-interna-de-la-api)`). |
| H-14 | Bajo      | `CLAUDE.md` · l.7 · carpetas `src/` y `test/`                                       | Las carpetas existen en local pero están vacías y Git no versiona carpetas vacías: en un clon del repositorio no existen.                                                                                                         | Añadir un `.gitkeep` en cada una o aclarar en `CLAUDE.md` que se crearán con la solución.                                                     |
| H-15 | Bajo      | `bitacora.md` · E-008, l.503 · `ADR-002`                                             | La bitácora marca como "(pendiente)" agregar a ADR-002 la experiencia de la desarrolladora con SQL Server; `ADR-002` no la contiene.                                                                                             | Añadir la justificación a ADR-002 o cerrar el pendiente en la bitácora.                                                                        |
| H-16 | Bajo      | `.gitignore` · l.487–488                                                            | Ignora `src/wwwroot/app/` como "Build de la SPA (web/ → npm run build)". No existe carpeta `web/` y el frontend está fuera de alcance (`00-vision.md` §4, l.50).                                                                 | Eliminar esas líneas o documentar por qué se conservan.                                                                                        |

### 1.5 Resumen

| Severidad | Cantidad |
| --------- | -------- |
| Crítico   | 0        |
| Alto      | 3        |
| Medio     | 7        |
| Bajo      | 6        |
| **Total** | **16**   |

---

## 2. Contradicciones

Se compararon los 15 documentos del inventario en tres ejes: conceptos definidos en varios lugares, reglas de negocio frente al modelo, y decisiones técnicas. **Sin contradicciones** en: versión de .NET (10 en todos), base de datos (SQL Server), ORM (EF Core), autenticación (encabezado `X-Usuario-Id`, JWT fuera del MVP), Docker (excluido en todos), roles, valores de prioridad, categoría y estado del SLA, y longitudes de RN-01 y RN-09.

### 2.1 Hallazgos

| ID   | Severidad | Archivo y ubicación | Descripción | Propuesta de corrección |
| ---- | --------- | ------------------- | ----------- | ----------------------- |
| H-17 | Alto | **A:** `01-requisitos-funcionales.md` · CA-03.3, l.69 · **B:** `ADR-004` · tabla de mapeo, l.21 | A: asignar un técnico **que no existe** responde `400`. B: "Recurso no encontrado (Application) → `404`", sin excepción para este caso. Un handler que busque al técnico y no lo encuentre produciría `404`, no `400`. | Precisar en ADR-004 que el `404` aplica solo al recurso de la ruta (el ticket) y que una referencia inválida en el cuerpo (técnico) es `400`. |
| H-18 | Alto | **A:** `ADR-001` · §Decisión, l.16 · **B:** `ADR-003` · §Decisión, l.26–27 · **C:** `ADR-004` · l.22–23 | A: "una excepción de dominio que termina en `409`". B: la violación de invariantes lanza "una excepción de dominio" que se trata como `500`. C: solo `TransicionInvalidaException` y conflictos de estado van a `409`. El término "excepción de dominio" tiene dos destinos HTTP distintos. | Nombrar dos tipos (p. ej., conflicto de estado → `409`; invariante violada → `500`) y usarlos en los tres ADR y en `01-modelo-dominio.md` §4. |
| H-19 | Medio | **A:** `02-arquitectura-c4.md` · §2, l.36 · **B:** `01-modelo-dominio.md` · §4, l.103 y l.110–119 · `ADR-001` l.17 | A: la API valida identidad y permiso (`403`) "antes de ejecutar el caso de uso". B: el `403` lo deciden los handlers de Application, es decir, dentro del caso de uso. | Corregir el texto de A: la Api valida la identidad (`401`) y el caso de uso valida el permiso (`403`). |
| H-20 | Medio | **A:** `ADR-004` · l.19 · `02-arquitectura-c4.md` §3, l.76 · **B:** `02-arquitectura-c4.md` §3, l.76 y l.96 · `RNF-07` l.26 | A: el `401` por "usuario inexistente" se decide en la capa Api. B: la Api no usa Infrastructure ni el `DbContext` salvo para registrar dependencias. No se documenta cómo la Api comprueba que el usuario existe sin acceder a datos. | Documentar el mecanismo (p. ej., un servicio de Application que la Api invoca) sin romper la regla de dependencias. |
| H-21 | Medio | **A:** `CLAUDE.md` · l.38 · `02-reglas-negocio.md` · l.3 · **B:** `01-modelo-dominio.md` · §4, l.110–111 · `ADR-001` l.17 | A: las RN van en Domain y solo pasan a Application "si dependen de datos externos". B: RN-04 y RN-05 están solo en Application, y el criterio usado es "consultar la base de datos **o conocer al actor**". RN-05 (paginación) no depende de datos externos. | Unificar el criterio de A con el de `01-modelo-dominio.md` §4 (l.103). |
| H-22 | Medio | **A:** `RNF-07` · `03-requisitos-no-funcionales.md` l.26 · **B:** `02-arquitectura-c4.md` · §3, l.76 y l.86 | A: "Infrastructure y Api referencian las capas internas". B: Api referencia también a Infrastructure (capa externa) para registrar dependencias. RNF-07 no contempla esa excepción. | Añadir a RNF-07 la referencia Api → Infrastructure limitada a `Program.cs`, como en B. |
| H-23 | Medio | **A:** `CLAUDE.md` · l.46 · **B:** `01-requisitos-funcionales.md` CA-02.4, l.51 · `02-reglas-negocio.md` RN-05, l.19 | A: listados con "`pageSize` (máximo 50)". B: además `page` ≥ 1 y `pageSize` ≥ 1, con `400` fuera de rango. | Actualizar A con el rango completo y el `400`. |
| H-24 | Medio | **A:** `CLAUDE.md` · l.39 · **B:** `RNF-01` · `03-requisitos-no-funcionales.md` l.20 | A: `401` solo "si el usuario no existe". B: `401` también si falta el encabezado o el Id no es válido. | Actualizar A con los tres casos de B. |
| H-25 | Bajo | **A:** `CLAUDE.md` · l.43 · **B:** `02-reglas-negocio.md` RN-09, l.59 | A omite el máximo de 2000 caracteres y que se conserva el técnico al reabrir. | Completar A o remitir a RN-09 sin resumirla. |
| H-26 | Bajo | **A:** `ADR-003` · §Decisión, l.25 · **B:** `01-modelo-dominio.md` · §1, l.48 y l.74; §4, l.111 | A: todos los límites, incluido el rango de paginación 1–50, son constantes públicas de Domain. B: solo define constantes de `Ticket` y `Comentario`, y RN-05 (paginación) está únicamente en Application. | Decidir dónde vive el límite de paginación y alinear ambos documentos. |
| H-27 | Bajo | **A:** `02-reglas-negocio.md` · tabla RN-08, l.36 y l.39–40 · `01-modelo-dominio.md` §6, l.226–230 · **B:** `02-reglas-negocio.md` · diagrama, l.49 y l.52–53 | La misma máquina de estados está dibujada dos veces. A dice "Técnico asignado" y "Solicitante dueño"; B dice "Técnico" y "Solicitante". | Dejar un solo diagrama (p. ej., el de `02-reglas-negocio.md`) con las etiquetas de la tabla y referenciarlo desde el modelo. |
| H-28 | Bajo | **A:** `02-reglas-negocio.md` · l.37–40 · `01-requisitos-funcionales.md` CA-03.2 l.68, CA-04.1 l.87 · `CLAUDE.md` l.42 · **B:** `01-modelo-dominio.md` · §2, l.85 | El estado se escribe "En progreso" en A y `EnProgreso` en B. El documento no aclara cuál será el valor en la API (RNF-09 exige enums como texto). | Indicar que el valor de contrato es `EnProgreso` y que "En progreso" es solo texto, como se hizo con `Critica` (decisión 6 de `01-modelo-dominio.md` §7). |
| H-29 | Bajo | **A:** `01-modelo-dominio.md` · §1, l.23 · **B:** `03-requisitos-no-funcionales.md` RNF-06, l.25 · `ADR-002` l.17 | A: `Email` es único. B enumera los índices y el único índice único es `Codigo`. | Añadir el índice único de `Usuario.Email` a B o quitar la unicidad de A. |
| H-30 | Bajo | **A:** `00-vision.md` l.3–10 · `03-requisitos-no-funcionales.md` l.3–7 · **B:** `01-requisitos-funcionales.md` y `02-reglas-negocio.md` (sin encabezado) | A tiene encabezado de versión, fecha y estado; B no. Además, `00-vision.md` sigue en versión 1.0 del 26/10/2026 y `03-requisitos-no-funcionales.md` en 1.0, aunque ambos cambiaron el 27/10/2026 (commit `f55293e`). | Añadir el encabezado a B y actualizar versión y fecha en los documentos modificados. |

### 2.2 Resumen

| Severidad | Cantidad |
| --------- | -------- |
| Crítico   | 0        |
| Alto      | 2        |
| Medio     | 6        |
| Bajo      | 6        |
| **Total** | **14**   |

---

## 3. Definiciones incompletas

Revisado y **completo**: lista cerrada de valores de los seis enums (`01-modelo-dominio.md` §2), transiciones permitidas (`02-reglas-negocio.md` RN-08), cardinalidades (`01-modelo-dominio.md` §3), longitudes de título, descripción y comentarios (RN-01, RN-09), y tipo y obligatoriedad de cada atributo de las tres entidades (`01-modelo-dominio.md` §1). Ya reportados antes y no repetidos aquí: contrato de endpoints (H-03), modelo de datos físico y datos semilla (H-12).

### 3.1 Hallazgos

| ID   | Severidad | Archivo y ubicación | Descripción | Propuesta de corrección |
| ---- | --------- | ------------------- | ----------- | ----------------------- |
| H-31 | Crítico *(reclasificado en la fase 4; antes Alto)* | `01-requisitos-funcionales.md` · CA-02.1, l.48 | La consulta por Id solo enumera cinco datos (estado, prioridad, técnico asignado, vencimiento, estado del SLA). No dice si se devuelven código, título, descripción, categoría, solicitante, demás fechas o comentarios, ni si el técnico se representa por Id o por nombre. Tampoco se definen los campos de cada elemento del listado (CA-02.4). | Definir en el contrato de endpoints los campos de la respuesta de detalle y de listado. |
| H-32 | Alto | `01-requisitos-funcionales.md` · HU-01 l.18, CA-01.1 l.24 · `00-vision.md` §3, l.31–33 | Solo se define que el **solicitante** crea tickets. No se indica si un técnico o un supervisor pueden crearlos ni qué se responde si lo intentan. | Decidirlo y añadir un CA (p. ej., `403` para otros roles) o ampliar CA-01.1. |
| H-33 | Medio | `02-reglas-negocio.md` · RN-04 l.18, RN-06 l.25, RN-08 l.32–40, RN-12 l.87, RN-13 l.12 · `01-requisitos-funcionales.md` CA-04.7 l.93 | No hay una matriz rol × operación: los permisos están repartidos en cinco reglas. Casos sin respuesta definida: el técnico asignado intenta cerrar o reabrir (RN-08 solo nombra al solicitante y al supervisor; CA-04.7 solo cubre al técnico **no** asignado) o el supervisor intenta iniciar atención o resolver. | Añadir una matriz rol × operación con el código de respuesta para cada combinación no permitida. |
| H-34 | Medio | `03-requisitos-no-funcionales.md` · RNF-04, l.23 · `ADR-004` · l.16–23 | No se define la precedencia cuando aplican varios errores a la vez (p. ej., un solicitante que reabre un ticket ajeno ya `Cerrado`: ¿`403` o `409`?; un ticket inexistente con usuario sin permiso: ¿`404` o `403`?). | Definir el orden de evaluación (identidad → existencia → permiso → validación → estado, u otro) en ADR-004 o RNF-04. |
| H-35 | Medio | `01-requisitos-funcionales.md` · CA-02.4, l.51 · `02-reglas-negocio.md` RN-05, l.19 | No se definen los valores por defecto de `page` y `pageSize` cuando se omiten, ni el nombre y la forma de "el total de registros" en la respuesta. | Definirlos en CA-02.4 o en el contrato de endpoints. |
| H-36 | Medio | `01-requisitos-funcionales.md` · CA-02.3, l.50 · HU-02, l.42 | Los filtros `estado`, `prioridad`, `tecnicoId` y `soloVencidos` no tienen tipo, valores admitidos ni respuesta ante un valor inválido. No se indica cómo se combinan con RN-04 (p. ej., un técnico que filtra por el `tecnicoId` de otro). | Especificar tipo, valores, respuesta a valores inválidos e interacción con RN-04. |
| H-37 | Medio | `02-reglas-negocio.md` · RN-09 l.59, RN-12 l.87 · `01-requisitos-funcionales.md` CA-04.3 l.89, CA-04.5 l.91, CA-05.4 l.110 | Reglas sin CA de error: reabrir sin motivo (CA-04.3 solo cubre resolver), solución o motivo de más de 2000 caracteres, y cambiar a una prioridad inexistente. RNF-10 exige una prueba por CA. | Añadir los CA de error correspondientes. |
| H-38 | Medio | `01-modelo-dominio.md` · §1, l.31–46 · `00-vision.md` §1, l.20 | No se define auditoría de cambios: el ticket no registra quién ni cuándo asignó, reasignó, reclasificó, cambió la prioridad o cerró. La visión plantea como problema "saber quién atiende qué". | Decidir si el MVP registra historial o campos de auditoría y documentarlo, o declararlo fuera de alcance. |
| H-39 | Medio | `01-modelo-dominio.md` · §3, l.98 · `00-vision.md` §4 | El modelo define "borrado en cascada" de comentarios, pero ningún RF permite eliminar tickets y no se dice si el borrado sería lógico o físico. | Declarar que no se eliminan tickets en el MVP o definir el tipo de borrado. |
| H-40 | Medio | `01-modelo-dominio.md` · §1, l.31–46 · `03-requisitos-no-funcionales.md` RNF-04, l.23 | No se define el control de concurrencia (p. ej., dos supervisores reasignan a la vez, o el técnico resuelve mientras el supervisor reasigna). El modelo no tiene campo de versión. | Decidir una estrategia (p. ej., concurrencia optimista con `409`) o declararlo fuera de alcance. |
| H-41 | Alto *(reclasificado en la corrección parte 2; antes Bajo)* | `03-requisitos-no-funcionales.md` · RNF-03, l.22 · `ADR-004` · l.16–23 | RNF-03 verifica los campos `type` y `title`, pero no se definen sus valores para cada tipo de error. | Definir un catálogo de `type` y `title` por código. |
| H-42 | Bajo | `02-reglas-negocio.md` · RN-03, l.11 · `ADR-002` · l.18 | No se define el valor inicial de la secuencia ni qué ocurre al superar `HD-999999`. | Documentar el inicio y el límite, o declarar el límite aceptado para el MVP. |
| H-43 | Alto *(reclasificado en la corrección parte 2; antes Bajo)* | `03-requisitos-no-funcionales.md` · RNF-01, l.20 · `01-modelo-dominio.md` §1, l.21 | "No es un Id válido" no especifica qué es válido (numérico, positivo, rango de `int`). | Precisar el formato válido de `X-Usuario-Id`. |
| H-44 | Bajo | `02-reglas-negocio.md` · RN-01 l.9, RN-09 l.59 | "Obligatorio" no aclara si un texto formado solo por espacios es válido ni si las longitudes se miden antes o después de recortar espacios. | Precisarlo en RN-01 y RN-09. |

---

## 4. Ambigüedades

Términos vagos encontrados: "Opcional" (RNF-07), "etc." (RNF-09), "y equivalentes" (RNF-10), "o" entre dos alternativas técnicas (ADR-002) y "(pendiente)" en la bitácora (ya reportados en H-09 y H-15). No aparecen "y/o", "según corresponda", "TBD" ni "por definir".

### 4.1 Hallazgos

| ID   | Severidad | Archivo y ubicación | Descripción | Propuesta de corrección |
| ---- | --------- | ------------------- | ----------- | ----------------------- |
| H-45 | Medio | `02-reglas-negocio.md` · RN-03, l.11 · `ADR-002` · l.18 y l.27 | "Código único y consecutivo" admite dos lecturas: sin huecos, o simplemente creciente. Una SEQUENCE de SQL Server puede dejar huecos si una transacción falla, y ADR-002 afirma que garantiza "consecutivos sin duplicados". | Precisar en RN-03 si se admiten huecos. |
| H-46 | Medio | `01-requisitos-funcionales.md` · CA-06.1, l.126 · HU-08, l.122 | "Agrupada por técnico y por prioridad" puede ser dos agrupaciones separadas o una agrupación cruzada (técnico × prioridad). | Precisar la estructura del reporte en CA-06.1. |
| H-47 | Bajo | **A:** `01-requisitos-funcionales.md` HU-09 l.20, CA-04.4 l.90 · **B:** `02-reglas-negocio.md` RN-13 l.12, RN-08 l.39 | A dice "técnico o supervisor" y "el solicitante"; B precisa "técnico **asignado**" y "solicitante **dueño**". Leída sola, A admite a cualquier técnico o solicitante. | Usar en A los mismos calificativos que en B. |
| H-48 | Bajo | `01-requisitos-funcionales.md` · CA-03.2, l.68 · `02-reglas-negocio.md` RN-08, l.35 | "Reasigna a otro técnico" no aclara si reasignar al mismo técnico está permitido, es un error o no hace nada. | Definir el comportamiento. |
| H-49 | Bajo | `03-requisitos-no-funcionales.md` · RNF-07 l.26, RNF-09 l.28, RNF-10 l.29 · `ADR-002` l.14 | Términos vagos en criterios de verificación: "Opcional: prueba de arquitectura" (RNF-07), "`Critica`, etc." (RNF-09), "`--filter "CA=CA-01.1"` (y equivalentes)" sin definir la clave de `[Trait]` para RN (RNF-10), y "LocalDB o instancia local" sin indicar cuál se usa para las pruebas (ADR-002). | Reemplazar por criterios cerrados: decidir si la prueba de arquitectura es obligatoria, listar los enums y definir las claves de `[Trait]` y el entorno de pruebas. |
| H-50 | Medio | `README.md` l.2 · `00-vision.md` l.14, l.18, l.20, l.32, l.83 · `01-requisitos-funcionales.md` HU-03, l.44 | Cuatro términos para el mismo concepto: "ticket", "incidencia" (README), "solicitud" (visión, glosario y HU-03) y "caso" (visión). El glosario define *Ticket* como "Solicitud de soporte". | Adoptar "ticket" como término canónico (sección 5) y reservar "solicitud" solo para la definición. |
| H-51 | Bajo | `00-vision.md` §3, l.31–32 · `02-arquitectura-c4.md` l.21–22, l.42–43 · `README.md` l.2 | Sinónimos de roles: "colaborador" y "usuarios" por *solicitante*; "personal de soporte" por *técnico*. | Usar solo los nombres de rol; dejar las descripciones como explicación, no como nombre. |
| H-52 | Bajo | `01-requisitos-funcionales.md` HU-09 l.20, CA-01.6 l.29, CA-04.4 l.90 · `00-vision.md` l.31, l.33 | Varios verbos para una misma acción: "reclasificar", "corregir la categoría" y "cambia la categoría"; "cerrar", "confirmar cierre" y "confirma". La visión dice "ver reportes" (plural) y el MVP define un solo reporte (RF-06). | Usar un verbo canónico por acción (sección 5) y "reporte de vencidos" en singular. |

### Resumen de las secciones 3 y 4

Incluye las reclasificaciones posteriores: H-31 a Crítico (fase 4), y H-41 y H-43 a Alto (corrección parte 2).

| Severidad | Sección 3 | Sección 4 | Total |
| --------- | --------- | --------- | ----- |
| Crítico   | 1         | 0         | 1     |
| Alto      | 3         | 0         | 3     |
| Medio     | 8         | 3         | 11    |
| Bajo      | 2         | 5         | 7     |
| **Total** | **14**    | **8**     | **22** |

---

## 5. Glosario propuesto

Un término canónico por concepto. Las definiciones se toman de los documentos citados; no se agregan conceptos nuevos. Donde la fuente no basta, se remite al hallazgo correspondiente. La columna "Sinónimos encontrados" solo lista términos que aparecen en la documentación actual.

| Término canónico | Valor de contrato | Definición (fuente) | Sinónimos encontrados (evitar) |
| ---------------- | ----------------- | ------------------- | ------------------------------ |
| Ticket | — | Solicitud de soporte registrada en el sistema (`00-vision.md` §8). | incidencia, caso (H-50). "Solicitud" solo para el problema previo al sistema (`00-vision.md` §1) y en la definición |
| Código de ticket | — | `HD-` + 6 dígitos, único y creciente; se aceptan huecos (RN-03, decisión 13). | consecutivo |
| Usuario | — | Persona cargada como dato semilla con un rol (S-01, `01-modelo-dominio.md` §1). | — |
| Solicitante | `Solicitante` | Rol que reporta un problema y crea tickets (`00-vision.md` §3). | colaborador, usuarios (H-51) |
| Solicitante dueño | — | Solicitante que creó el ticket (`SolicitanteId`, RN-04, RN-08). | "el solicitante" sin calificar (H-47) |
| Técnico | `Tecnico` | Rol que atiende los tickets (`00-vision.md` §3). | personal de soporte (H-51) |
| Técnico asignado | — | Técnico referenciado en `TecnicoId` del ticket (RN-08, RN-13). | "técnico" sin calificar (H-47) |
| Supervisor | `Supervisor` | Coordinador del área de TI (`00-vision.md` §3). | — |
| Estado del ticket | `EstadoTicket` | Uno de `Abierto`, `Asignado`, `EnProgreso`, `Resuelto`, `Cerrado` (RN-08). "En progreso" solo como texto de lectura (H-28). | — |
| Asignar | — | Fijar el técnico de un ticket `Abierto`; pasa a `Asignado` (RN-07, RN-08). | — |
| Reasignar | — | Cambiar el técnico responsable de un ticket (`00-vision.md` §8); queda en `Asignado` (RN-07). | — |
| Iniciar atención | — | Transición `Asignado` → `EnProgreso` por el técnico asignado (RN-08). | — |
| Resolver | — | Transición `EnProgreso` → `Resuelto` con una solución (RN-08, RN-09). | marcar como resuelto |
| Solución | `Solucion` (tipo de comentario) | Texto obligatorio al resolver, máx. 2000 (RN-09). | comentario de solución (aceptable, lo usa RN-09) |
| Cerrar | — | Transición `Resuelto` → `Cerrado` por el solicitante dueño o el supervisor (RN-08). | confirmar cierre, confirma (H-52) |
| Reabrir | — | Transición `Resuelto` → `EnProgreso` por el solicitante dueño, con motivo (RN-08, RN-09). | — |
| Motivo de reapertura | `Reapertura` (tipo de comentario) | Texto obligatorio al reabrir, máx. 2000 (RN-09). | — |
| Reclasificar | — | Cambiar la categoría de un ticket no `Resuelto` ni `Cerrado` (RN-13). | corregir la categoría, cambiar la categoría (H-52) |
| Cambiar prioridad | `cambiar-prioridad` (ruta) | Cambiar la prioridad de un ticket no `Resuelto` ni `Cerrado`; recalcula el vencimiento (RN-12). Solo el supervisor. | — |
| Consultar técnicos activos | `tecnicos` (ruta) | Obtener la lista de técnicos activos como `{ id, nombre }`, sin paginar (CA-03.6, decisiones 12, 24 y 45). Solo el supervisor. | — |
| Categoría | `Categoria` | Uno de `Hardware`, `Software`, `Red`, `Accesos`, `Otros` (RN-02). | — |
| Prioridad | `Prioridad` | Uno de `Critica`, `Alta`, `Media`, `Baja` (RN-02); "Crítica" solo como texto de lectura. | — |
| SLA | — | Tiempo máximo comprometido para resolver un ticket según su prioridad (`00-vision.md` §8; horas en RN-10). | — |
| Vencimiento | `FechaVencimiento` | Fecha y hora límite del SLA (`00-vision.md` §8); creación + horas de SLA (RN-10). | fecha de vencimiento (aceptable) |
| Estado del SLA | `EstadoSla` | Valor calculado: `EnTiempo`, `Vencido`, `Cumplido`, `Incumplido` (RN-11). | — |
| Ticket vencido | — | Ticket con estado del SLA `Vencido` (RN-11, CA-06.1). Alcance del filtro `soloVencidos`: ver H-36. | — |
| Comentario | `Comentario` | Texto ligado a un ticket, de tipo `Solucion` o `Reapertura` (`01-modelo-dominio.md` §1). | — |
| Reporte de vencidos | — | Conteo de tickets vencidos en dos agrupaciones separadas, por técnico y por prioridad, más el total (RF-06, decisión 11). | reportes (H-52) |

---

## 6. Matriz de trazabilidad

_Estado original de la fase 4; tras las correcciones, los requisitos sin caso de uso quedaron cubiertos por HU-10 a HU-14 (H-53) y `Usuario` tiene la consulta de técnicos activos (H-54)._

Los casos de uso del proyecto son las historias de usuario (HU) de `01-requisitos-funcionales.md`; no hay otro documento de casos de uso. Ningún documento define recursos de la API (H-03): el único endpoint mencionado es `GET /tickets` (RNF-06, `03-requisitos-no-funcionales.md` l.25). Por eso la última columna indica **qué operación exige el requisito**, no una ruta.

| Requisito | CA | Caso de uso (HU) | Entidades | Reglas | Operación exigida · recurso documentado |
| --------- | -- | ---------------- | --------- | ------ | --------------------------------------- |
| RF-01 | CA-01.1 a CA-01.5 | HU-01 | Ticket, Usuario | RN-01, RN-02, RN-03, RN-10 | Crear ticket · no documentado |
| RF-01 | CA-01.6 a CA-01.9 | HU-09 | Ticket | RN-02, RN-13 | Reclasificar · no documentado |
| RF-02 | CA-02.1, CA-02.2, CA-02.6 | HU-03 (solicitante) · **sin HU para el técnico** | Ticket, Usuario | RN-04, RN-11 | Consultar por Id · no documentado |
| RF-02 | CA-02.3, CA-02.4, CA-02.5 | HU-02, HU-03 · **sin HU para el técnico** | Ticket | RN-04, RN-05, RN-11 | Listar · `GET /tickets` (solo en RNF-06) |
| RF-03 | CA-03.1, CA-03.3 a CA-03.5 | HU-04 | Ticket, Usuario | RN-06, RN-07, RN-08 | Asignar · no documentado |
| RF-03 | CA-03.2 | **Sin HU** (HU-04 solo dice "asignar") | Ticket, Usuario | RN-06, RN-07, RN-08 | Reasignar · no documentado |
| RF-04 | CA-04.1 | HU-05 | Ticket | RN-08 | Iniciar atención · no documentado |
| RF-04 | CA-04.2, CA-04.3 | HU-05 | Ticket, Comentario | RN-08, RN-09 | Resolver · no documentado |
| RF-04 | CA-04.4 | HU-06 (solicitante) · **sin HU para el supervisor** | Ticket | RN-08 | Cerrar · no documentado |
| RF-04 | CA-04.5 | HU-06 | Ticket, Comentario | RN-08, RN-09 | Reabrir · no documentado |
| RF-04 | CA-04.6, CA-04.7 | HU-05, HU-06 | Ticket | RN-08 | Todas las transiciones |
| RF-05 | CA-05.1 a CA-05.3 | HU-07 | Ticket | RN-10, RN-11 | Sin operación propia (datos en consultar y listar) |
| RF-05 | CA-05.4 a CA-05.6 | **Sin HU** (HU-07 no menciona cambiar la prioridad) | Ticket | RN-10, RN-12 | Cambiar prioridad · no documentado |
| RF-06 | CA-06.1 a CA-06.3 | HU-08 | Ticket, Usuario | RN-11 | Reporte de vencidos · no documentado |
| RNF-01 a RNF-10 | — | Transversales (no requieren HU) | — | — | Todas las operaciones |

**Casos de uso sin requisito:** ninguno. Las 9 HU pertenecen a un RF.

**Requisitos sin caso de uso:** cambio de prioridad (CA-05.4 a CA-05.6), reasignación (CA-03.2), cierre por el supervisor (CA-04.4) y consulta por el técnico (CA-02.5, CA-02.6). Ver H-53.

**Entidad sin operación:** `Usuario` no tiene ninguna operación documentada. El supervisor necesita el Id de un técnico para asignar (CA-03.1) y no hay forma documentada de obtenerlo. Ver H-54.

### 6.1 Hallazgos

| ID   | Severidad | Archivo y ubicación | Descripción | Propuesta de corrección |
| ---- | --------- | ------------------- | ----------- | ----------------------- |
| H-53 | Medio | `01-requisitos-funcionales.md` · HU-02 l.42, HU-03 l.44, HU-04 l.63, HU-06 l.83, HU-07 l.103 | Requisitos sin caso de uso: cambio de prioridad (CA-05.4 a CA-05.6), reasignación (CA-03.2), cierre por el supervisor (CA-04.4) y consulta por el técnico (CA-02.5, CA-02.6). | Añadir HU o ampliar las existentes para cubrir esos actores y acciones. |
| H-54 | Medio | `01-requisitos-funcionales.md` · CA-03.1, l.67 · `00-vision.md` S-01, l.60 | Ningún RF expone usuarios. El supervisor debe enviar el Id de un técnico activo para asignar, pero no hay operación ni documento que diga cómo obtenerlo. | Decidir entre una operación de consulta de técnicos o documentar los Id de los datos semilla. |
| H-55 | Crítico | `01-requisitos-funcionales.md` · CA-02.1 l.48, CA-02.3 l.50, CA-02.4 l.51, CA-03.1 l.67, CA-03.2 l.68, CA-04.1 l.87, CA-04.2 l.88, CA-04.4 l.90, CA-04.5 l.91, CA-05.4 l.110, CA-06.1 l.126 | Solo dos CA definen el código de éxito: crear (`201`, CA-01.1) y reclasificar (`200`, CA-01.6). Las otras 9 operaciones no indican el código de éxito ni si devuelven cuerpo. | Definir el código de éxito y el cuerpo de la respuesta de cada operación. |

---

## 7. Suficiencia de información por operación

_Estado original de la fase 4. La revalidación tras las correcciones está en la sección 13.3._

_Nota: esta tabla es la foto de la fase 4 (11 operaciones); el número vigente de operaciones (13) está en §13.3._

✅ definido · ⚠️ parcial · ❌ no definido. "Ruta y verbo" es ❌ en todas (H-03).

| Operación | Request | Response | Validaciones | Códigos de error | Código de éxito | Permisos | Qué falta exactamente |
| --------- | ------- | -------- | ------------ | ---------------- | --------------- | -------- | --------------------- |
| Crear ticket | ✅ título, descripción, categoría, prioridad (CA-01.1) | ❌ | ✅ RN-01, RN-02 | ⚠️ `400`, `401`; falta respuesta para otros roles | ✅ `201` | ⚠️ | Campos de respuesta (H-31); permiso de técnico y supervisor (H-32) |
| Consultar por Id | ✅ Id | ⚠️ 5 campos (CA-02.1) | — | ✅ `401`, `403`, `404` | ❌ | ✅ RN-04 | Campos completos (H-31); código de éxito (H-55); precedencia `403`/`404` (H-34) |
| Listar | ⚠️ filtros sin tipo ni valores | ❌ | ⚠️ paginación sí; filtros no | ✅ `400`, `401` | ❌ | ✅ RN-04 | Filtros (H-36); valores por defecto y forma del total (H-35); campos (H-31); código de éxito (H-55) |
| Reclasificar | ✅ categoría | ❌ | ✅ RN-02, RN-13 | ✅ `400`, `403`, `409` | ✅ `200` | ✅ RN-13 | Cuerpo de la respuesta (H-31) |
| Asignar / reasignar | ⚠️ Id del técnico (sin forma de obtenerlo) | ❌ | ✅ RN-06 | ⚠️ `400`, `403`, `409`; conflicto `400`/`404` | ❌ | ✅ RN-06 | Código de éxito (H-55); `404` o `400` (H-17); obtener Id del técnico (H-54); reasignar al mismo técnico (H-48) |
| Iniciar atención | ✅ sin datos | ❌ | — | ⚠️ `403`, `409` | ❌ | ⚠️ | Código de éxito (H-55); respuesta para supervisor (H-33) |
| Resolver | ✅ solución | ❌ | ⚠️ falta CA para > 2000 | ⚠️ `400`, `403`, `409` | ❌ | ⚠️ | Código de éxito (H-55); CA de error (H-37); permisos (H-33) |
| Cerrar | ✅ sin datos | ❌ | — | ⚠️ `409`; `403` solo para técnico no asignado | ❌ | ⚠️ | Código de éxito (H-55); respuesta para técnico asignado (H-33) |
| Reabrir | ✅ motivo | ❌ | ⚠️ sin CA para motivo vacío o > 2000 | ⚠️ `403`, `409` | ❌ | ⚠️ | Código de éxito (H-55); CA de error (H-37); permisos (H-33) |
| Cambiar prioridad | ✅ prioridad | ❌ | ⚠️ sin CA para prioridad inválida | ✅ `403`, `409` | ❌ | ✅ RN-12 | Código de éxito (H-55); CA de error (H-37); HU (H-53) |
| Reporte de vencidos | ✅ sin datos | ⚠️ agrupación ambigua | — | ✅ `403` | ❌ | ✅ CA-06.3 | Estructura del reporte (H-46); código de éxito (H-55) |

Comunes a todas las operaciones: el catálogo de `type` y `title` de Problem Details (H-41), el formato válido de `X-Usuario-Id` (H-43), la precedencia de errores (H-34) y la diferencia entre `409` y `500` en excepciones de dominio (H-18).

---

## 8. Preguntas abiertas

Las preguntas 1 a 8 bloquean el contrato de endpoints. Las opciones son sugerencias para facilitar la decisión, no recomendaciones.

1. **Campos de respuesta (H-31).** ¿Qué devuelven el detalle y el listado?
   - A) El mismo objeto completo en ambos, con comentarios.
   - B) Detalle completo con comentarios; listado resumido (código, título, estado, prioridad, técnico, vencimiento, estado del SLA).
   - ¿El técnico y el solicitante se representan por Id, por Id y nombre, o como objeto?
2. **Códigos de éxito (H-55).** ¿Qué responden asignar, las transiciones y cambiar la prioridad?
   - A) `200` con el ticket actualizado.
   - B) `204` sin cuerpo.
   - ¿Consultar, listar y el reporte responden `200`?
3. **Creación de tickets (H-32).** ¿Pueden crear tickets un técnico o un supervisor?
   - A) No: `403`.
   - B) Sí, y quedan como solicitante del ticket.
4. **Técnico inexistente al asignar (H-17).** ¿`400` (como dice CA-03.3) o `404`?
5. **Excepciones de dominio (H-18).** ¿Se confirman dos tipos: conflicto de estado → `409` e invariante violada → `500`? ¿Con qué nombres?
6. **Precedencia de errores (H-34).** ¿En qué orden se evalúan?
   - A) `401` → `404` → `403` → `400` → `409`.
   - B) `401` → `400` → `404` → `403` → `409`.
7. **Permisos no definidos (H-33).** ¿Qué se responde si el técnico asignado intenta cerrar o reabrir, o si el supervisor intenta iniciar atención o resolver?
   - A) `403` (el rol no puede hacer esa acción).
   - B) `409` (la acción no está permitida).
8. **Forma de las operaciones de estado (H-03).** ¿Cómo se exponen asignar, iniciar, resolver, cerrar, reabrir, reclasificar y cambiar prioridad?
   - A) Una operación por acción.
   - B) Una operación genérica de cambio de estado con la acción en el cuerpo.
9. **Paginación (H-35).** ¿Valores por defecto de `page` y `pageSize`, y nombre del campo del total?
   - A) `page` = 1 y `pageSize` = 10.
   - B) `page` = 1 y `pageSize` = 20.
   - C) Ambos obligatorios.
10. **Filtros (H-36).** ¿`soloVencidos` incluye solo `Vencido` o también `Incumplido`? Si un técnico filtra por otro `tecnicoId`: A) lista vacía; B) `403`. ¿Un valor de filtro inválido responde `400`?
11. **Reporte de vencidos (H-46).** A) Dos agrupaciones separadas (por técnico y por prioridad) más el total. B) Una agrupación cruzada técnico × prioridad más el total.
12. **Obtener el Id del técnico (H-54).** A) Operación de consulta de técnicos activos para el supervisor. B) Documentar los Id de los datos semilla en el README.
13. **Código consecutivo (H-45).** A) Se aceptan huecos (SEQUENCE). B) Sin huecos (requiere otro mecanismo).
14. **Reasignar al mismo técnico (H-48).** A) `400`. B) `409`. C) Se acepta sin cambios.
15. **CA de error faltantes (H-37).** ¿Se confirma `400` para reabrir sin motivo, para solución o motivo de más de 2000 caracteres y para una prioridad inexistente?
16. **Auditoría (H-38).** A) Fuera del MVP. B) Campos de quién y cuándo en el ticket. C) Historial de cambios.
17. **Concurrencia (H-40).** A) Concurrencia optimista con `409`. B) Fuera del MVP.
18. **Borrado (H-39).** ¿Se confirma que el MVP no elimina tickets?
19. **Datos semilla (H-12).** ¿Cuántos usuarios de cada rol y si se incluye un técnico inactivo para probar CA-03.3?
20. **Bitácora E-009 (H-06).** A) Falta escribir la entrada E-009. B) El mensaje del commit `0b06fcb` debía decir E-008.

---

## 9. Decisiones tomadas

Decisiones de la desarrolladora del 28/10/2026 sobre las preguntas de la sección 8. Son la fuente de verdad para la corrección (parte 1 de 2).

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 1  | Detalle completo y listado resumido; técnico y solicitante como `{ id, nombre }`. | H-31 (parcial) | `01-requisitos-funcionales.md` convenciones, CA-02.1, CA-02.4 |
| 2  | Crear → `201` con el ticket y `Location`; asignar, transiciones y cambiar prioridad → `200` con el ticket actualizado (detalle); consultas → `200`. | H-55 | `01-requisitos-funcionales.md` convenciones, CA-01.1, CA-02.1, CA-02.4, CA-03.1, CA-03.2, CA-03.6, CA-04.1, CA-04.2, CA-04.4, CA-04.5, CA-05.4, CA-06.1 |
| 3  | Solo el solicitante crea tickets; técnico o supervisor → `403`. | H-32 | RN-03, matriz de permisos (`02-reglas-negocio.md`), CA-01.10 |
| 4  | Técnico inexistente al asignar → `400`; `404` solo para el recurso de la ruta. | H-17 | Enmienda de ADR-004, RNF-04 |
| 5  | `TransicionInvalidaException` → `409`; `InvarianteVioladaException` → `500`. | H-18 | Enmiendas de ADR-001, ADR-003 y ADR-004; `01-modelo-dominio.md` §1 y §4; `02-arquitectura-c4.md` §3 |
| 6  | Precedencia de errores: `401` → `404` → `403` → `400` → `409`. | H-34 | RNF-04, enmienda de ADR-004, convenciones de `01-requisitos-funcionales.md` |
| 7  | Actor no autorizado para una acción → `403`. | H-33 | RN-08 (nota bajo la tabla), matriz de permisos, CA-04.7, enmienda de ADR-004 |
| 8  | Una operación por acción. | H-03 (parcial) | Convenciones de `01-requisitos-funcionales.md` |
| 9  | `page` por defecto 1; `pageSize` por defecto 20, máximo 100; respuesta con `items`, `page`, `pageSize`, `totalItems`. | H-35 | RN-05, CA-02.4, RNF-05, `01-modelo-dominio.md` §4, enmienda de ADR-003 |
| 10 | `soloVencidos` no incluye `Incumplido`; técnico que filtra por otro `tecnicoId` → `403`; filtro inválido → `400`. | H-36 | CA-02.3, CA-02.7, CA-02.8, RNF-05, matriz de permisos, enmienda de ADR-003 |
| 11 | Reporte con dos agrupaciones separadas (por técnico y por prioridad). | H-46 | CA-06.1 |
| 12 | Operación de consulta de técnicos activos, solo para el supervisor. | H-54 | `00-vision.md` §3, HU-14, CA-03.6, CA-03.7, matriz de permisos |
| 13 | Código "único y creciente"; se aceptan huecos. | H-45 | RN-03, `01-modelo-dominio.md` §1 y §4, enmienda de ADR-002 |
| 14 | Reasignar al mismo técnico → `400`. | H-48 | RN-06, CA-03.8, `01-modelo-dominio.md` §4, enmienda de ADR-003 |
| 15 | `400` para reabrir sin motivo, textos de más de 2000 caracteres y prioridad inexistente. | H-37 | CA-04.8, CA-04.9, CA-05.7, RNF-05, enmienda de ADR-003 |
| 16 | El ticket guarda `FechaCreacion` y `FechaActualizacion`; historial fuera del MVP. | H-38 | `01-modelo-dominio.md` §1 y §5, `00-vision.md` §4, enmienda de ADR-002 |
| 17 | Concurrencia fuera del MVP, como limitación conocida. | H-40 | `00-vision.md` §4, `03-requisitos-no-funcionales.md` (Fuera de alcance) |
| 18 | El MVP no elimina tickets. | H-39 | `00-vision.md` §4, `01-modelo-dominio.md` §3 |
| 19 | Datos semilla: 5 solicitantes, 4 técnicos (uno inactivo) y 1 supervisor. | H-12 (parcial) | `00-vision.md` S-01 |
| 20 | El commit `0b06fcb` debía llamarse E-008; nota sin reescribir el historial. | H-06 | `bitacora.md` E-008 |

Decisiones complementarias (28/10/2026), respuestas a las preguntas de la sección 11:

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 21 | (P-1) Cada elemento del listado incluye el `id` y el código del ticket. | H-31 | CA-02.4 |
| 22 | (P-2) Cada comentario del detalle tiene `id`, `texto`, `autor` (`{ id, nombre }`) y `fechaCreacion`. | H-31 | CA-02.1 |
| 23 | (P-3) Reclasificar responde `200` con el ticket actualizado (detalle). | H-31, H-55 | CA-01.6, convenciones de `01-requisitos-funcionales.md` |
| 24 | (P-4) La consulta de técnicos activos no se pagina; devuelve una lista de `{ id, nombre }`. | H-54 | CA-03.6 |
| 25 | (P-5) Al crear un ticket, `FechaActualizacion` es igual a `FechaCreacion`. | H-38 | `01-modelo-dominio.md` §1 |
| 26 | Se confirma el máximo de `pageSize` en 100. | H-35 | Sin cambios (ya aplicado con la decisión 9) |
| 27 | Cada comentario del detalle incluye además su `tipo` (`Solucion` o `Reapertura`), coherente con `TipoComentario` del modelo de dominio. | H-31 | CA-02.1 |

Decisiones sobre las preguntas de la sección 12 (28/10/2026):

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 28 | Se confirma que la Api obtiene el usuario a través de Application. | H-20 | Enmienda 3 de ADR-004 |
| 29 | (Q-1) Problem Details con `type` y `title` por defecto de ASP.NET Core, más el campo de extensión `codigo` con el caso de negocio (p. ej., `TRANSICION_INVALIDA`). | H-41 | RNF-03, Enmienda 3 de ADR-004 (con el catálogo de `codigo` propuesto), `CLAUDE.md` |
| 30 | (Q-2) `X-Usuario-Id` es un entero positivo dentro del rango de `int`; cualquier otro valor responde `401`. | H-43 | RNF-01, Enmienda 3 de ADR-004, `CLAUDE.md` |
| 31 | (Q-3) Los textos se recortan antes de validar; si quedan vacíos responden `400`. | H-44 | RN-01, RN-09, RNF-05, Enmienda 2 de ADR-003, `CLAUDE.md` |
| 32 | (Q-4) Las migraciones de EF Core son la fuente del modelo físico; los 10 usuarios semilla se documentan en una tabla con nombres y correos ficticios. | H-12 | `01-modelo-dominio.md` §8, Enmienda 2 de ADR-002, `00-vision.md` S-01, `CLAUDE.md`, `README.md` |
| 33 | (Q-5) `Usuario.Email` tiene índice único. | H-29 | RNF-06, `01-modelo-dominio.md` §1, Enmienda 2 de ADR-002 |
| 34 | (Q-6) La constante de paginación vive en Application. | H-26 | `01-modelo-dominio.md` §4, Enmienda 2 de ADR-003 |
| 35 | (Q-7) El código inicia en `HD-000001`; el límite `HD-999999` es una limitación conocida del MVP. | H-42 | RN-03, `00-vision.md` §4, `01-modelo-dominio.md` §1, Enmienda 2 de ADR-002, `CLAUDE.md` |
| 36 | (Q-8) La prueba de arquitectura es obligatoria; las claves de `[Trait]` son `CA` y `RN`; se usa LocalDB. | H-49 | RNF-07, RNF-10, Enmienda 2 de ADR-002, `CLAUDE.md`, `README.md` |
| 37 | (Q-9) Justificación de ADR-002: más de 10 años de experiencia desarrollando software con .NET y con SQL Server. | H-15 | Enmienda 2 de ADR-002, `bitacora.md` E-008 |
| 38 | (Q-10) Se eliminan las líneas de la SPA del `.gitignore`. | H-16 | `.gitignore` |
| 39 | Se aprueba el catálogo de `codigo` tal como se propuso, con los códigos separados. | H-41 | Enmienda 4 de ADR-004 |
| 40 | Las excepciones de dominio exponen una propiedad `Codigo`. `TransicionInvalidaException` se lanza con `TRANSICION_INVALIDA` o `TICKET_NO_MODIFICABLE` según el caso, y la Api copia ese valor al campo `codigo` de Problem Details. | H-18, H-41 | `01-modelo-dominio.md` §4, Enmienda 4 de ADR-004 |
| 41 | En los errores `500` la Api ignora el `Codigo` de la excepción y responde siempre `ERROR_INTERNO`, sin exponer detalles internos; el detalle de la excepción se registra en el log. | H-41 | `01-modelo-dominio.md` §4, Enmienda 4 de ADR-004 |

Decisiones para el contrato de endpoints (28/10/2026, parte 5.1):

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 42 | Asignar y reasignar son dos operaciones distintas (decisión 8); el contrato tiene 13 operaciones. Según la tabla de RN-08, asignar un ticket `Asignado` y reasignar uno `Abierto` no son combinaciones permitidas: responden `409` `TRANSICION_INVALIDA`. | H-03 | Esta auditoría (§13.3 y resumen ejecutivo); `01-modelo-dominio.md` §1, §4 y §5; RN-07 (`02-reglas-negocio.md`); Enmienda 5 de ADR-004 |
| 43 | Todas las rutas usan el prefijo `/api/v1`. | H-03 | RNF-06 (ejemplo corregido) |
| 44 | Las rutas de varias palabras usan kebab-case, sin tildes. | H-03 | — (se aplica en el contrato) |
| 45 | Técnicos activos: `GET /api/v1/tecnicos`, que devuelve solo los técnicos activos. | H-03 | Glosario §5 |
| 46 | Reporte de vencidos: `GET /api/v1/reportes/tickets-vencidos`. | H-03 | — (se aplica en el contrato) |
| 47 | Los campos JSON van en camelCase. | H-03 | — (se aplica en el contrato) |
| 48 | Los enums se serializan como texto, con el nombre del valor en el modelo de dominio. | H-03 | — (se aplica en el contrato) |
| 49 | En las respuestas, `tecnico` es `null` cuando el ticket no tiene técnico; las fechas que aún no aplican también son `null`. | H-03 | — (se aplica en el contrato) |
| 50 | En el reporte, el grupo "Sin asignar" se representa con `tecnico: null`. | H-03 | — (se aplica en el contrato) |
| 51 | El solicitante puede usar el filtro `tecnicoId`; se aplica dentro de su visibilidad (RN-04) y puede devolver una lista vacía. | H-03 | — (se aplica en el contrato) |
| 52 | Swagger UI y el documento OpenAPI no exigen `X-Usuario-Id`. | H-03 | RNF-01 |
| 53 | Filtros: `soloVencidos` es booleano y `tecnicoId` es entero positivo; un valor inválido responde `400` `VALIDACION_FALLIDA` (CA-02.8, decisión 10). | H-03 | — (se aplica en el contrato) |

Decisiones para el contrato de endpoints (28/10/2026, parte 5.2):

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 54 | El solicitante del ticket se toma de `X-Usuario-Id`; nunca viene en el cuerpo. | H-03 | — (se aplica en el contrato) |
| 55 | Cambiar prioridad recibe el campo `prioridad`. | H-03 | Tabla de métodos de `01-modelo-dominio.md` §1 (`CambiarPrioridad(prioridad)`) |
| 56 | Reporte de vencidos: campos `porTecnico`, `porPrioridad`, `cantidad` y `total`. Solo se incluyen grupos con `cantidad` mayor que 0. `total` es igual a la suma de cada agrupación. | H-03 | — (se aplica en el contrato) |
| 57 | Orden del reporte: `porPrioridad` por severidad (`Critica`, `Alta`, `Media`, `Baja`); `porTecnico` por `cantidad` descendente y luego por nombre, con `tecnico: null` al final. | H-03 | — (se aplica en el contrato) |
| 58 | Enums de entrada (cuerpo y query): no distinguen mayúsculas y minúsculas; las respuestas usan siempre la grafía canónica; los valores numéricos se rechazan con `400` `VALIDACION_FALLIDA`. | H-03 | — (se aplica en el contrato) |
| 59 | El listado de tickets se ordena por `fechaCreacion` descendente (RN-05, CA-02.4) y, en caso de empate, por `id` descendente. | H-03 | `03-contrato-api.md` (Convenciones comunes) |
| 60 | Las fechas en JSON usan ISO 8601 en UTC con sufijo `Z` (por ejemplo, `2026-10-28T14:30:00Z`); los segundos fraccionarios son opcionales. | H-03 | `03-contrato-api.md` §1.6 |
| 61 | Los mensajes de `errors` van en español y su redacción queda a la implementación; el contrato fija solo las claves y el `codigo`. Las claves de `errors` usan los nombres de campo en camelCase. | H-03 | `03-contrato-api.md` §1.3 |
| 62 | Los Problem Details incluyen `traceId`. `detail` e `instance` no forman parte del contrato, y `detail` nunca expone información interna. | H-03 | `03-contrato-api.md` §1.3 y §4 |
| 63 | Los ejemplos del contrato usan un elenco fijo de tickets ilustrativos, definido en §1.10. | H-03 | `03-contrato-api.md` §1.10 |
| 64 | El encabezado `Location` de crear ticket es una ruta relativa (`/api/v1/tickets/{id}`). | H-03 | `03-contrato-api.md` §2.1 |
| 65 | Un `{id}` de ruta que no sea un entero positivo responde `404` `TICKET_NO_ENCONTRADO`, con Problem Details completo. | H-03 | `03-contrato-api.md` §1.1 |
| 66 | Los campos no previstos en el cuerpo se ignoran (por ejemplo, `solicitanteId`, según la decisión 54). | H-03 | `03-contrato-api.md` §1.7 |
| 67 | Reclasificar con la misma categoría o cambiar a la misma prioridad responde `200` sin cambios: no se modifican `fechaActualizacion` ni `fechaVencimiento`. Solo aplica a tickets modificables (ver decisión 70). | H-03 | `03-contrato-api.md` §2.4 y §2.5 |
| 68 | Los comentarios del detalle se ordenan por `fechaCreacion` ascendente y luego por `id` ascendente. | H-03 | `03-contrato-api.md` §2.2 |
| 69 | Un filtro `tecnicoId` que no corresponde a un técnico existente responde `200` con una lista vacía. | H-03 | `03-contrato-api.md` §2.3 |
| 70 | La decisión 67 solo aplica a tickets modificables: reclasificar o cambiar la prioridad de un ticket `Resuelto` o `Cerrado` responde `409` `TICKET_NO_MODIFICABLE`, aunque el valor sea igual al actual. | H-03 | `03-contrato-api.md` §2.4 y §2.5 |
| 71 | En asignar y reasignar, un `tecnicoId` ausente o no positivo responde `400` `VALIDACION_FALLIDA` (`errors.tecnicoId`); `TECNICO_NO_VALIDO` queda para un Id bien formado que no existe, no tiene rol Técnico o está inactivo. | H-03 | `03-contrato-api.md` §3.1 y §3.2 |
| 72 | El orden de los `400` en asignar y reasignar es: `VALIDACION_FALLIDA`, luego `TECNICO_NO_VALIDO`, luego `MISMO_TECNICO`. | H-03 | `03-contrato-api.md` §3.1 y §3.2 |
| 73 | La lista de técnicos activos se ordena por nombre ascendente y luego por `id`. | H-03 | `03-contrato-api.md` §3.3 |
| 74 | Toda operación que modifica el ticket actualiza `fechaActualizacion`, incluidas las que agregan comentarios; la operación sin cambios de la decisión 67 no la modifica. Complementa la decisión 16 sin contradecirla. | H-03 | `03-contrato-api.md` §1.6 y operaciones que modifican el ticket |

Decisiones para la implementación de Domain (29/10/2026):

Los hallazgos D-01 a D-05 salen de la revisión de solo lectura de la capa Domain, y D-06 y D-07 de las revisiones de regresión, todas del 29/10/2026; no forman parte de H-01 a H-55.

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 75 | Asignar y reasignar son dos métodos de `Ticket`: `Asignar(tecnicoId, ahora)` solo desde `Abierto` y `Reasignar(tecnicoId, ahora)` solo desde `Asignado` o `EnProgreso`. Cualquier otro estado lanza `TransicionInvalidaException` con `TRANSICION_INVALIDA` (decisión 42, tabla de RN-08). | D-01 | `01-modelo-dominio.md` §1, §4 (tabla de excepciones, RN-07 y RN-08) y §5; RN-07 (`02-reglas-negocio.md`); Enmienda 5 de ADR-004 |
| 76 | Todo método que modifica el ticket recibe el parámetro `ahora` (`DateTime` UTC), que Application obtiene de `TimeProvider`. Afecta a `Asignar`, `Reasignar`, `IniciarAtencion`, `Reabrir`, `CambiarPrioridad` y `Reclasificar`. La operación sin cambios de la decisión 67 no modifica `FechaActualizacion`. | D-02 | `01-modelo-dominio.md` §1 (tabla de métodos y nota) y §5 |
| 77 | `Resolver(solucion, autorId, ahora)` y `Reabrir(motivo, autorId, ahora)` reciben el autor del comentario como parámetro. Domain no valida quién es el autor; el permiso lo valida Application (`403`). | D-03 | `01-modelo-dominio.md` §1, §4 (RN-09) y §5 |
| 78 | Firma `Ticket.Crear(codigo, titulo, descripcion, categoria, prioridad, solicitanteId, ahora)`. El código lo genera `IGeneradorCodigoTicket` antes de crear el ticket; Domain no valida su formato. | D-04 | `01-modelo-dominio.md` §1 y §5 |
| 79 | Application recorta los textos antes de validar y de llamar a Domain; Domain valida el texto que recibe (no vacío y dentro de las longitudes) y guarda ese valor. | D-05 | `01-modelo-dominio.md` §1 (tabla de métodos y nota) y §4 (RN-01 y RN-09); Enmienda 3 de ADR-003 |
| 80 | `InvarianteVioladaException` lleva el `Codigo` `ERROR_INTERNO` (complementa las decisiones 40 y 41). | Observación de la revisión de Domain | `01-modelo-dominio.md` §4 (tabla de excepciones); Enmienda 5 de ADR-004 |
| 81 | Asignar y reasignar tienen un handler cada uno: `AsignarTicketHandler` y `ReasignarTicketHandler`. Ambos validan el actor supervisor (`403`) y el técnico (`TECNICO_NO_VALIDO`); `MISMO_TECNICO` solo aplica a `ReasignarTicketHandler` (contrato §3.2). | Pregunta de la revisión de Domain (fila RN-06) | `01-modelo-dominio.md` §4 (RN-06) |
| 82 | CA-03.4 cubre los casos cruzados de la decisión 42: asignar un ticket que no está `Abierto`, o reasignar uno que no está `Asignado` ni `En progreso`, responde `409` y el ticket no cambia. | Pregunta de la revisión de Domain (CA-03.4) | CA-03.4 (`01-requisitos-funcionales.md`); Enmienda 5 de ADR-004 |
| 83 | Domain exige que el texto llegue ya recortado. `Ticket.Crear`, `Ticket.Resolver` y `Ticket.Reabrir` lanzan `InvarianteVioladaException` si el texto está en blanco (vacío o solo espacios) o si tiene espacios al inicio o al final. Las longitudes de RN-01 y RN-09 se miden sobre ese texto. "Espacio" es cualquier carácter de espacio en blanco, con la misma definición que `string.Trim()` y `string.IsNullOrWhiteSpace()` de .NET (incluye tabuladores y saltos de línea), tanto en Domain como en Application. Los espacios y saltos de línea en medio del texto están permitidos; la regla solo aplica a los extremos y al texto completamente en blanco. Complementa la decisión 79. | D-06 | `01-modelo-dominio.md` §1 (filas de `Crear`, `Resolver` y `Reabrir`, y nota) y §4 (RN-01 y RN-09); Enmienda 4 de ADR-003 |
| 84 | `Ticket.Crear`, `Ticket.CambiarPrioridad` y `Ticket.Reclasificar` lanzan `InvarianteVioladaException` (`Codigo` `ERROR_INTERNO`, decisión 80) si reciben un valor de `Categoria` o `Prioridad` que no está definido en el enum. `PoliticaSla.HorasPara` lanza la misma excepción con una prioridad no definida. La comprobación del valor del enum se hace antes que la guarda de estado: un valor no definido sobre un ticket `Resuelto` o `Cerrado` lanza `InvarianteVioladaException`, no `TICKET_NO_MODIFICABLE`, en el mismo orden que el `400` antes del `409` de la decisión 6. Por un caso de uso no ocurre, porque la decisión 58 lo rechaza antes con `400`. | D-07 | `01-modelo-dominio.md` §1 (filas de `Crear`, `CambiarPrioridad` y `Reclasificar`) y §4 (tabla de excepciones, RN-02 y RN-10); `02-arquitectura-c4.md` §3; Enmienda 5 de ADR-003 |

Decisiones de la implementación de Domain (30/10/2026):

Salen del plan de implementación de la capa Domain; no forman parte de H-01 a H-55 ni de D-01 a D-07.

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 85 | En `Ticket.Resolver` y `Ticket.Reabrir`, el texto se valida antes que el estado. Un texto inválido sobre un estado no permitido lanza `InvarianteVioladaException` (`ERROR_INTERNO`), no `TRANSICION_INVALIDA`, con el mismo criterio que las decisiones 6 y 84. Lo cubren las pruebas `Resolver_TextoEnBlancoEnEstadoNoPermitido_LanzaInvarianteViolada` y `Reabrir_TextoEnBlancoEnEstadoNoPermitido_LanzaInvarianteViolada`. | Plan de implementación de Domain | `01-modelo-dominio.md` §1 (filas de `Resolver` y `Reabrir`) y §4 (RN-09); Enmienda 6 de ADR-003 |
| 86 | `Ticket.Reasignar` no rechaza reasignar al mismo técnico, porque `MISMO_TECNICO` se valida en `ReasignarTicketHandler` (decisión 81). Domain aplica la reasignación y fija `FechaActualizacion`. Lo documenta la prueba `Reasignar_MismoTecnico_NoLanzaExcepcion`. | Plan de implementación de Domain | `01-modelo-dominio.md` §1 (fila de `Reasignar`) y §4 (RN-06); Enmienda 6 de ADR-003 |

Decisiones para la implementación de Infrastructure (30/10/2026):

Las decisiones 87 y 88 salen de los hallazgos I-01 e I-02 de la revisión de solo lectura de la capa Infrastructure del 30/10/2026, la 89 de la revisión de ese informe, y la 90 y la 91 de la implementación de la capa; no forman parte de H-01 a H-55 ni de D-01 a D-07.

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 87 | Las claves foráneas `Ticket.SolicitanteId`, `Ticket.TecnicoId` y `Comentario.AutorId` usan `Restrict` al borrar. Solo `Comentario.TicketId` usa cascada (composición de `01-modelo-dominio.md` §3). El MVP no borra usuarios (no hay ningún RF para hacerlo; son datos semilla, S-01) ni tickets (decisión 18). | I-01 | `01-modelo-dominio.md` §3; Enmienda 3 de ADR-002 |
| 88 | Las columnas que guardan enums como texto son `nvarchar(20)`: `Ticket.Estado`, `Ticket.Prioridad`, `Ticket.Categoria`, `Usuario.Rol` y `Comentario.Tipo`. | I-02 | `01-modelo-dominio.md` §2; Enmienda 3 de ADR-002 |
| 89 | Los 10 usuarios semilla de `01-modelo-dominio.md` §8 tienen los Id 1 a 10, en el orden de la tabla, coherentes con los ejemplos de `03-contrato-api.md` §1.10. | Revisión del informe de Infrastructure (Id de los datos semilla) | `01-modelo-dominio.md` §8; Enmienda 3 de ADR-002 |
| 90 | La SEQUENCE `CodigoTicket` tiene `MAXVALUE` 999999 y no es cíclica, y el formato del código (`FormatoCodigoTicket`) rechaza los números fuera de 1 a 999999. Al superar el máximo, la creación del ticket debe fallar con `500` `ERROR_INTERNO` (decisión 41) sin crear el ticket. Completa la decisión 35. La prueba `GenerarAsync_SecuenciaEnMaximo_DevuelveHD999999YLuegoFalla` comprueba que la SEQUENCE entrega `HD-999999` y que la llamada siguiente falla; el `500` `ERROR_INTERNO` y que no se cree el ticket se comprobarán al implementar RF-01, cuando existan el handler de crear y la Api. | Implementación de Infrastructure | `01-modelo-dominio.md` §1 (fila de `Codigo`) y §4 (RN-03); Enmienda 4 de ADR-002 |
| 91 | Las pruebas de integración usan una base propia en LocalDB, distinta de `HelpDesk` (por convención, `HelpDesk_Pruebas`), que se recrea con la migración al empezar cada ejecución y se conserva al terminar. Su cadena de conexión se lee de los user-secrets del proyecto de pruebas, con la clave `ConnectionStrings:HelpDeskPruebas`, o de la variable de entorno `ConnectionStrings__HelpDeskPruebas` (RNF-02). Si la cadena apunta a la base de desarrollo `HelpDesk` o no nombra ninguna base, las pruebas fallan antes de conectarse. | Implementación de Infrastructure | Enmienda 4 de ADR-002 |

Decisiones para la implementación de RF-01 (30/10/2026):

Salen de los hallazgos R-01 a R-08 de la revisión de solo lectura de RF-01 del 30/10/2026; no forman parte de H-01 a H-55, de D-01 a D-07 ni de I-01 e I-02.

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 92 | El handler aplica el orden de la decisión 6: primero comprueba la existencia del recurso de la ruta (`404`), después el permiso (`403`), después invoca el validador (`400`) y al final llama a Domain (`409`). El validador sigue siendo una clase de FluentValidation, invocada por el handler. | R-01 | Enmienda 7 de ADR-003; `.claude/rules/application.md` |
| 93 | Un cuerpo ausente, vacío, `null`, que no es un objeto o con JSON mal formado responde `400` `VALIDACION_FALLIDA` con la clave `cuerpo` en `errors`. Una petición sin `Content-Type` se trata como un cuerpo ausente o ilegible (con un `Content-Type` presente que no es JSON aplica la decisión 94). Un campo con un tipo JSON incorrecto responde `400` `VALIDACION_FALLIDA` con la clave de ese campo, y solo se informa ese error, porque la lectura del JSON se detiene en el primer error de tipo; es una excepción a "devuelve todos los errores a la vez" de ADR-003. Excepción al orden de la decisión 6: todo error de lectura del cuerpo, incluido un campo con tipo JSON incorrecto, ocurre antes del handler, así que su `400` se responde antes que el `404` y el `403`. | R-02 | `03-contrato-api.md` §1.3 y §1.4; Enmienda 6 de ADR-004; Enmienda 7 de ADR-003 |
| 94 | Las rutas inexistentes (`404`), los métodos no admitidos (`405`) y los tipos de contenido no admitidos (`415`) responden con Problem Details (RNF-03), con `traceId` y sin el campo `codigo`, porque no son casos de negocio y quedan fuera del catálogo. El `415` se responde cuando el `Content-Type` está presente y no es JSON; si falta, aplica la decisión 93. | R-03 | `03-contrato-api.md` §1.3; Enmienda 6 de ADR-004; RNF-03 (`03-requisitos-no-funcionales.md`); `.claude/rules/api.md`; `.claude/rules/tests.md` |
| 95 | Un Id válido en `X-Usuario-Id` o en `{id}` está formado solo por dígitos ASCII, sin signo, sin espacios y sin ceros a la izquierda, dentro del rango de `int` y mayor que 0. Un encabezado `X-Usuario-Id` repetido responde `401` `USUARIO_NO_IDENTIFICADO`. | R-04 | `03-contrato-api.md` §1.1 y §1.2 |
| 96 | Los DTO de petición reciben `categoria` y `prioridad` como texto. El validador los convierte sin distinguir mayúsculas y rechaza los textos numéricos (decisión 58). El esquema OpenAPI de esos campos se anota con los valores del enum (RNF-09). El `{id}` llega a Application como `int`, ya validado por la Api. | R-05 | Enmienda 7 de ADR-003 |
| 97 | Application define `ITicketRepositorio` (escritura del agregado), `IUsuarioRepositorio` (lectura de usuarios), una unidad de trabajo e `IConsultasTicket` (lectura del detalle del ticket con los nombres de solicitante, técnico y autores). El middleware de `X-Usuario-Id` obtiene el usuario a través de Application (decisión 28) y lo guarda en un servicio con alcance por petición, `IUsuarioActual`, que usan los handlers. | R-06 | Enmienda 3 de ADR-001 |
| 98 | Application señala sus errores con excepciones que llevan `Codigo`: una para el `404`, otra para el `403`, que admite cualquier `codigo` `403` del catálogo (`ROL_NO_AUTORIZADO`, `TICKET_AJENO` y `FILTRO_NO_AUTORIZADO`), y otra para los `400`, que admite cualquier `codigo` `400` del catálogo (`VALIDACION_FALLIDA` con `errors`, `TECNICO_NO_VALIDO` y `MISMO_TECNICO`). Las constantes de los códigos de Application viven en Application. La Api las traduce a Problem Details. | R-07 | Enmienda 6 de ADR-004 |
| 99 | Las pruebas de extremo a extremo usan la base de pruebas de la decisión 91 en una colección serializada. Antes de cada prueba se borran los tickets y comentarios y se reinicia la SEQUENCE con `RESTART WITH 1`. La factory de pruebas sobrescribe `ConnectionStrings:HelpDesk` con la cadena de pruebas, conserva la guarda contra la base `HelpDesk` y usa `FakeTimeProvider`. | R-08 | Enmienda 5 de ADR-002; `.claude/rules/tests.md` |

Decisiones de la implementación de RF-01 (31/10/2026):

Salen de los planes de implementación de RF-01; no forman parte de H-01 a H-55, de D-01 a D-07, de I-01 e I-02 ni de R-01 a R-08.

| #  | Decisión | Hallazgos que resuelve | Dónde se aplicó |
| -- | -------- | ---------------------- | --------------- |
| 111 | `IUsuarioActual` (decisión 97) expone solo `Id` y `Rol`. Leerlo sin un usuario fijado, o fijarlo dos veces en la misma petición, lanza `InvalidOperationException` (`500`, decisión 41): es un error de programación, no un caso de negocio. | Plan de implementación de RF-01 | Enmienda 5 de ADR-001 |
| 112 | `IConsultasTicket` (decisión 97) recibe la hora de la operación (`ahora`) y calcula `estadoSla` con `Ticket.CalcularEstadoSla(ahora)`, sin duplicar RN-11. | Plan de implementación de RF-01 | Enmienda 5 de ADR-001 |
| 113 | Las claves de `errors` se convierten a camelCase (decisión 61) en Application, al crear la excepción de validación (decisión 98). | Plan de implementación de RF-01 | Enmienda 8 de ADR-004 |
| 114 | Los valores de enum con espacios (por ejemplo, `" Red "`) se rechazan con `400` `VALIDACION_FALLIDA`: el recorte de la decisión 31 se aplica solo a los textos libres. | Plan de implementación de RF-01 | `03-contrato-api.md` §1.8 |
| 115 | Reclasificar guarda siempre, también en la operación sin cambios de la decisión 67. EF Core no envía ningún `UPDATE` si nada cambió, así que `fechaActualizacion` no se modifica. | Plan de implementación de RF-01 | — (código: `ReclasificarTicketHandler` en `abc4a3d`) |
| 116 | El middleware de `X-Usuario-Id` actúa solo sobre acciones de controlador, después del enrutamiento: una ruta inexistente responde `404` y un método no admitido `405` aunque falte el encabezado (decisión 94). | Plan de implementación de RF-01 | `03-contrato-api.md` §1.2 (Exentos) |
| 117 | HTTP quita los espacios al inicio y al final del valor de un encabezado (RFC 9110) antes de que la Api lo lea, y la regla de la decisión 95 se aplica al valor ya sin ellos. Por eso `" 1"` en `X-Usuario-Id` equivale a `"1"`. Las pruebas con `TestServer` comprueban el valor tal como llega a la Api, porque `TestServer` no quita esos espacios como Kestrel; en ellas, `" 1"` responde `401`. | Plan de implementación de RF-01 | `03-contrato-api.md` §1.2 (ejemplo corregido) |
| 118 | Las pruebas de extremo a extremo borran los tickets y comentarios y reinician la SEQUENCE antes **y después** de cada prueba, para no dejar datos que choquen con el índice único del código en las pruebas de integración. Completa la decisión 99. | Plan de implementación de RF-01 | Enmienda 6 de ADR-002; `.claude/rules/tests.md` |
| 119 | El documento OpenAPI declara el `415` (decisión 94) en las operaciones con cuerpo. El esquema de Problem Details incluye `codigo`, con los 12 valores del catálogo de ADR-004 y no obligatorio, y `traceId`. El `{id}` de la ruta no lleva patrón, para que se pueda probar el `404` de la decisión 65. | Plan de implementación de RF-01 | — (código: transformadores de `src/HelpDesk.Api/Documentacion/` en `e6030c9`) |

Casos de uso agregados (H-53): HU-10 cambiar prioridad, HU-11 reasignar, HU-12 cierre por el supervisor, HU-13 consulta por el técnico y HU-14 consulta de técnicos activos. Totales: 14 HU y 45 CA (RNF-10 actualizado).

Cambio respecto de lo documentado antes: la decisión 9 sube el máximo de `pageSize` de 50 a 100 (antes en RN-05, CA-02.4, RNF-05 y ADR-003).

---

## 10. Estado de los hallazgos

Estado tras la corrección parte 1, la parte 2 y las decisiones 28 a 38 (28/10/2026).

| ID   | Estado | Nota |
| ---- | ------ | ---- |
| H-01 | Resuelto | `CLAUDE.md` §Estado del proyecto lista y enlaza los documentos funcionales, técnicos, el índice de ADR y la auditoría. |
| H-02 | Resuelto | Eliminada la línea "no inicializada"; §Git indica que los cambios llegan a `main` por pull request (E-004). |
| H-03 | Resuelto | Contrato de endpoints en [03-contrato-api.md](../specs/technical/03-contrato-api.md): 13 operaciones, con las tres verificaciones de su §5 limpias (28/10/2026). |
| H-04 | Resuelto | `adr/README.md` enlazado desde `README.md`, `CLAUDE.md` y `02-arquitectura-c4.md`. |
| H-05 | Resuelto | `README.md` con propósito, stack, requisitos de ejecución (ADR-002, S-01), estructura e índice de documentos. |
| H-06 | Resuelto | Decisión 20: nota de corrección en E-008. |
| H-07 | Resuelto | E-007: se corrige la cita a RNF-10 con nota de corrección. |
| H-08 | Resuelto | Plantilla E-NNN al inicio de `bitacora.md`, con los campos de `CLAUDE.md` y el formato de E-001 a E-008. |
| H-09 | Resuelto | E-001: `21ae3a5` y `61b123f`; E-002 y E-003: `61b123f`, según E-004. |
| H-10 | Resuelto | Nota en E-001: las carpetas reales son `test/` y `docs/evidencias/`. |
| H-11 | Resuelto | Nota en E-004 con la ruta correcta; la transcripción del prompt se mantiene literal. |
| H-12 | Resuelto | Decisiones 19 y 32: las migraciones de EF Core son la fuente del modelo físico; los 10 usuarios semilla están en `01-modelo-dominio.md` §8. |
| H-13 | Resuelto | Referencias entre documentos convertidas en enlaces relativos; en los ADR, mediante enmiendas. 81 enlaces revalidados (§13.1). |
| H-14 | Resuelto | `CLAUDE.md` aclara que `src/` y `test/` no se versionan hasta tener contenido. |
| H-15 | Resuelto | Decisión 37: Enmienda 2 de ADR-002; pendiente cerrado en E-008. |
| H-16 | Resuelto | Decisión 38: eliminado el bloque de la SPA del `.gitignore` (`src/wwwroot/app/` y `*.tsbuildinfo`). |
| H-17 | Resuelto | Decisión 4: enmienda de ADR-004 y RNF-04. |
| H-18 | Resuelto | Decisión 5: enmiendas de ADR-001, ADR-003 y ADR-004; modelo y C4 nombran ambos tipos. |
| H-19 | Resuelto | `02-arquitectura-c4.md` §2: la Api valida el `401`; el `403` lo valida el caso de uso en Application. |
| H-20 | Resuelto | `02-arquitectura-c4.md` §2 y §3, Enmienda 2 de ADR-001 y ADR-004: la Api obtiene el usuario a través de Application. Confirmado por la decisión 28. |
| H-21 | Resuelto | `CLAUDE.md` y `02-reglas-negocio.md` usan el criterio de `01-modelo-dominio.md` §4. |
| H-22 | Resuelto | RNF-07 incluye la referencia Api → Infrastructure limitada a `Program.cs`. |
| H-23 | Resuelto | `CLAUDE.md`: paginación con valores por defecto, máximo 100, `400` y forma de la respuesta (decisión 9). |
| H-24 | Resuelto | `CLAUDE.md`: los tres casos de `401` de RNF-01. |
| H-25 | Resuelto | `CLAUDE.md` remite a RN-09 en lugar de resumirla. |
| H-26 | Resuelto | Decisión 34: en Application (`01-modelo-dominio.md` §4, Enmienda 2 de ADR-003). |
| H-27 | Resuelto | Los dos diagramas de estados usan ahora las mismas etiquetas ("Técnico asignado", "Solicitante dueño"). Se conservan ambos. |
| H-28 | Resuelto | Nota en RN-08 y en `CLAUDE.md`: el valor en la API es `EnProgreso`. |
| H-29 | Resuelto | Decisión 33: índice único en RNF-06, el modelo y la Enmienda 2 de ADR-002. |
| H-30 | Resuelto | Encabezado agregado a `01-requisitos-funcionales.md` y `02-reglas-negocio.md`; versión y fecha actualizadas en 00, 01, 02, 03, modelo (1.3) y C4. |
| H-31 | Resuelto | Decisiones 1, 21, 22, 23 y 27: campos del detalle (con comentarios) en CA-02.1, del listado en CA-02.4 y cuerpo de reclasificar en CA-01.6. |
| H-32 | Resuelto | Decisión 3: RN-03, CA-01.10 y matriz de permisos. |
| H-33 | Resuelto | Decisión 7: matriz de permisos en `02-reglas-negocio.md`, nota en RN-08 y CA-04.7 ampliado. |
| H-34 | Resuelto | Decisión 6: RNF-04 y enmienda de ADR-004. |
| H-35 | Resuelto | Decisión 9: RN-05, CA-02.4 y RNF-05. |
| H-36 | Resuelto | Decisión 10: CA-02.3, CA-02.7 y CA-02.8. |
| H-37 | Resuelto | Decisión 15: CA-04.8, CA-04.9 y CA-05.7. |
| H-38 | Resuelto | Decisiones 16 y 25: `FechaActualizacion` en el modelo (igual a `FechaCreacion` al crear); historial fuera del MVP. |
| H-39 | Resuelto | Decisión 18: `00-vision.md` §4 y nota en el modelo §3. |
| H-40 | Resuelto | Decisión 17: limitación conocida en `00-vision.md` §4 y RNF. |
| H-41 | Resuelto | Decisión 29: valores por defecto más `codigo` (RNF-03, Enmienda 3 de ADR-004). Catálogo de `codigo` aprobado (decisión 39) y propiedad `Codigo` en las excepciones de dominio (decisión 40, Enmienda 4 de ADR-004). |
| H-42 | Resuelto | Decisión 35: RN-03, `00-vision.md` §4, modelo y Enmienda 2 de ADR-002. |
| H-43 | Resuelto | Decisión 30: RNF-01 y Enmienda 3 de ADR-004. |
| H-44 | Resuelto | Decisión 31: RN-01, RN-09, RNF-05 y Enmienda 2 de ADR-003. |
| H-45 | Resuelto | Decisión 13: RN-03, modelo, enmienda de ADR-002 y `CLAUDE.md`. |
| H-46 | Resuelto | Decisión 11: CA-06.1. |
| H-47 | Resuelto | HU-09 dice "técnico asignado"; CA-04.4 y CA-04.5 dicen "solicitante dueño"; diagrama de RN-08 con las mismas etiquetas. |
| H-48 | Resuelto | Decisión 14: RN-06, CA-03.8 y modelo §4. |
| H-49 | Resuelto | "etc." corregido en la parte 2; decisión 36: prueba de arquitectura obligatoria (RNF-07), claves `CA` y `RN` (RNF-10) y LocalDB (Enmienda 2 de ADR-002). |
| H-50 | Resuelto | "ticket" en README, visión (§3), HU-03; "solicitud" queda solo para el problema previo al sistema (§1 de la visión) y la definición. |
| H-51 | Resuelto | README reescrito con los nombres de rol; "colaborador" y "personal de soporte" quedan solo como descripción del rol. |
| H-52 | Resuelto | "reclasificar" (HU-09, CA-01.6, visión), "cerrar" (HU-06, CA-04.4, visión), "resolver" (CA-04.2, visión) y "reporte de tickets vencidos" en singular (visión, C4). |
| H-53 | Resuelto | HU-10 a HU-14 en `01-requisitos-funcionales.md`. |
| H-54 | Resuelto | Decisiones 12 y 24: HU-14, CA-03.6 (lista sin paginar de `{ id, nombre }`), CA-03.7 y matriz de permisos. |
| H-55 | Resuelto | Decisiones 2 y 23: código de éxito y cuerpo en todos los CA de operaciones, incluido reclasificar. |

**Resueltos: 55 · Pendientes: 0.**

## 11. Preguntas abiertas de la corrección (parte 1)

Surgieron al aplicar las decisiones. **Respondidas el 28/10/2026** (decisiones 21 a 25, sección 9).

- **P-1 (H-31).** El listado resumido de la decisión 1 no incluye el `id` del ticket, y CA-02.1 consulta por Id. ¿Se agrega `id` a cada elemento del listado?
- **P-2 (H-31).** ¿Qué campos tiene cada comentario en el detalle? ¿El autor se devuelve también como `{ id, nombre }`?
- **P-3 (H-31, H-55).** Reclasificar responde `200` (CA-01.6), pero la decisión 2 no dice su cuerpo. ¿Devuelve el ticket actualizado (detalle), como las demás acciones?
- **P-4 (H-54).** ¿La consulta de técnicos activos se pagina como los listados (decisión 9) o devuelve la lista completa?
- **P-5 (H-38).** Al crear un ticket, ¿`FechaActualizacion` vale lo mismo que `FechaCreacion` o queda vacía hasta la primera modificación?

## 12. Preguntas abiertas (parte 2)

**Respondidas el 28/10/2026** (decisiones 28 a 38, sección 9).

**Bloquean el contrato de endpoints:**

- **Q-1 (H-41) · Catálogo de `type` y `title` de los errores.** RNF-03 verifica ambos campos, pero no tienen valores definidos.
  - A) Valores por defecto de ASP.NET Core: `type` con la URI de la sección de RFC 9110 de cada código y `title` estándar ("Bad Request", "Not Found", …).
  - B) Catálogo propio: un `type` y un `title` en español por cada caso de error (p. ej., transición inválida, técnico inactivo).
  - C) Valores por defecto de A, más un campo de extensión con el código del caso de negocio.
- **Q-2 (H-43) · Formato válido de `X-Usuario-Id`.** RNF-01 responde `401` si "no es un Id válido", sin definir qué es válido.
  - A) Entero positivo dentro del rango de `int` (el Id del modelo es `int`).
  - B) Cualquier entero; un valor ≤ 0 se trata como usuario inexistente (`401`).

**No bloquean el contrato:**

- **Q-3 (H-44) · Textos formados solo por espacios.** A) Se recortan los espacios antes de validar la longitud (un texto de solo espacios responde `400`). B) Se valida el texto tal cual.
- **Q-4 (H-12) · Modelo de datos físico y datos semilla.** A) Documentar tablas, columnas, nombre de la SEQUENCE y los 10 usuarios semilla en un documento técnico antes de las migraciones. B) Que las migraciones de EF Core sean la fuente; documentar solo los usuarios semilla.
- **Q-5 (H-29) · Unicidad de `Usuario.Email`.** A) Índice único (agregarlo a RNF-06 o a una enmienda de ADR-002). B) Quitar la unicidad del modelo.
- **Q-6 (H-26) · Constante de paginación.** A) En Application, junto a `ListarTicketsValidator`. B) En Domain, como las demás constantes (ADR-003).
- **Q-7 (H-42) · Código del ticket.** A) La secuencia inicia en 1 y el límite `HD-999999` se acepta para el MVP. B) Otro valor inicial o formato.
- **Q-8 (H-49) · Criterios de prueba.** ¿La prueba de arquitectura de RNF-07 es obligatoria? ¿Las claves de `[Trait]` son `CA` y `RN`? ¿Las pruebas de integración usan LocalDB o una instancia local?
- **Q-9 (H-15) · ADR-002.** Texto de la justificación por experiencia con SQL Server que quedó pendiente en E-008.
- **Q-10 (H-16) · `.gitignore`.** A) Eliminar las líneas de la SPA (`src/wwwroot/app/`). B) Conservarlas con un comentario que explique por qué.

## 13. Revalidación final (28/10/2026)

### 13.1 Enlaces

Se validaron con un script todos los enlaces Markdown internos de los documentos (excluidos `node_modules/` y el código entre comillas invertidas): **94 enlaces, 0 rotos**, incluidas las anclas, calculadas con el algoritmo de GitHub. Ningún documento queda huérfano: todos se enlazan desde `README.md`.

### 13.2 Contradicciones

Se revisaron de nuevo los ejes de la sección 2 sobre los documentos corregidos. **Sin contradicciones nuevas.** En el cuerpo de los ADR quedan menciones que corrigen sus enmiendas:

- "consecutivo" en ADR-002 (Enmienda 1);
- "LocalDB o instancia local" en ADR-002 (Enmienda 2);
- el rango 1–50 y las constantes de paginación en Domain en ADR-003 (Enmiendas 1 y 2).

Las menciones de la bitácora (E-002, E-005) son registro histórico.

### 13.3 Suficiencia por operación

✅ definido. "Ruta y verbo" se define en el contrato (H-03); la decisión 8 fija una operación por acción. En todas las operaciones el encabezado `X-Usuario-Id` es un entero positivo de `int` (decisión 30) y los errores usan Problem Details con `codigo` (decisión 29).

| Operación | Request | Response | Validaciones | Errores (`codigo`) | Éxito | Permisos |
| --------- | ------- | -------- | ------------ | ------------------ | ----- | -------- |
| Crear ticket | ✅ | ✅ detalle + `Location` | ✅ | ✅ `400`, `401`, `403` | ✅ `201` | ✅ |
| Consultar por Id | ✅ | ✅ detalle | — | ✅ `401`, `403`, `404` | ✅ `200` | ✅ |
| Listar | ✅ filtros y paginación | ✅ `items`, `page`, `pageSize`, `totalItems` | ✅ | ✅ `400`, `401`, `403` | ✅ `200` | ✅ |
| Reclasificar | ✅ | ✅ detalle | ✅ | ✅ `400`, `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Asignar | ✅ | ✅ detalle | ✅ | ✅ `400`, `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Reasignar | ✅ | ✅ detalle | ✅ | ✅ `400`, `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Consultar técnicos activos | ✅ | ✅ lista de `{ id, nombre }` sin paginar | — | ✅ `401`, `403` | ✅ `200` | ✅ |
| Iniciar atención | ✅ | ✅ detalle | — | ✅ `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Resolver | ✅ | ✅ detalle | ✅ | ✅ `400`, `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Cerrar | ✅ | ✅ detalle | — | ✅ `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Reabrir | ✅ | ✅ detalle | ✅ | ✅ `400`, `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Cambiar prioridad | ✅ | ✅ detalle | ✅ | ✅ `400`, `401`, `403`, `404`, `409` | ✅ `200` | ✅ |
| Reporte de vencidos | ✅ | ✅ dos agrupaciones y total; nombres de campo a definir en el contrato | — | ✅ `401`, `403` | ✅ `200` | ✅ |

Las 13 operaciones tienen información suficiente para su contrato. El catálogo de `codigo` está aprobado (decisión 39) y cada `409` se distingue por el `Codigo` de `TransicionInvalidaException` (decisión 40).