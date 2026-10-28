# Auditoría de la documentación

| Campo   | Valor                                  |
| ------- | -------------------------------------- |
| Versión | 1.0                                    |
| Fecha   | 28/10/2026                             |
| Estado  | Final · Fase 4 de 4                    |
| Rama    | `feat/specs-tecnicas` (commit `0b06fcb`) |

## Resumen ejecutivo

Se auditaron los 15 documentos del repositorio en cuatro fases: inventario y enlaces, contradicciones, definiciones incompletas y ambigüedades, y trazabilidad hacia la API.

| Severidad | Cantidad | Hallazgos |
| --------- | -------- | --------- |
| Crítico   | 2        | H-31, H-55 |
| Alto      | 6        | H-01, H-02, H-03, H-17, H-18, H-32 |
| Medio     | 26       | H-04 a H-09, H-12, H-19 a H-24, H-33 a H-40, H-45, H-46, H-50, H-53, H-54 |
| Bajo      | 21       | H-10, H-11, H-13 a H-16, H-25 a H-30, H-41 a H-44, H-47 a H-49, H-51, H-52 |
| **Total** | **55**   | |

### Veredicto: **NO LISTA** para generar el contrato de endpoints

Motivos:

1. **Dos hallazgos críticos pendientes.** No están definidos los campos de las respuestas (H-31) ni los códigos de éxito de 9 de las 11 operaciones identificadas (H-55). El contrato tendría que inventarlos.
2. **Hallazgos altos que afectan al contrato:** la elección entre `404` y `400` para un técnico inexistente (H-17), el doble significado de "excepción de dominio" (`409` frente a `500`, H-18) y los permisos para crear tickets (H-32).
3. **Ninguna de las 11 operaciones** tiene información suficiente para su contrato (sección 7).

Para pasar a **LISTA** basta con responder las preguntas abiertas 1 a 8 (sección 8) y reflejar las respuestas en las especificaciones. Las demás preguntas pueden resolverse durante el diseño del contrato.

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
| H-41 | Bajo | `03-requisitos-no-funcionales.md` · RNF-03, l.22 · `ADR-004` · l.16–23 | RNF-03 verifica los campos `type` y `title`, pero no se definen sus valores para cada tipo de error. | Definir un catálogo de `type` y `title` por código. |
| H-42 | Bajo | `02-reglas-negocio.md` · RN-03, l.11 · `ADR-002` · l.18 | No se define el valor inicial de la secuencia ni qué ocurre al superar `HD-999999`. | Documentar el inicio y el límite, o declarar el límite aceptado para el MVP. |
| H-43 | Bajo | `03-requisitos-no-funcionales.md` · RNF-01, l.20 · `01-modelo-dominio.md` §1, l.21 | "No es un Id válido" no especifica qué es válido (numérico, positivo, rango de `int`). | Precisar el formato válido de `X-Usuario-Id`. |
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

---

## 5. Glosario propuesto

Un término canónico por concepto. Las definiciones se toman de los documentos citados; no se agregan conceptos nuevos. Donde la fuente no basta, se remite al hallazgo correspondiente. La columna "Sinónimos encontrados" solo lista términos que aparecen en la documentación actual.

| Término canónico | Valor de contrato | Definición (fuente) | Sinónimos encontrados (evitar) |
| ---------------- | ----------------- | ------------------- | ------------------------------ |
| Ticket | — | Solicitud de soporte registrada en el sistema (`00-vision.md` §8). | incidencia, caso, solicitud (H-50) |
| Código de ticket | — | `HD-` + 6 dígitos, único y consecutivo (RN-03). Interpretación de "consecutivo": ver H-45. | — |
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
| Solución | `Solucion` (tipo de comentario) | Texto obligatorio al resolver, máx. 2000 (RN-09). | comentario de solución |
| Cerrar | — | Transición `Resuelto` → `Cerrado` por el solicitante dueño o el supervisor (RN-08). | confirmar cierre, confirma (H-52) |
| Reabrir | — | Transición `Resuelto` → `EnProgreso` por el solicitante dueño, con motivo (RN-08, RN-09). | — |
| Motivo de reapertura | `Reapertura` (tipo de comentario) | Texto obligatorio al reabrir, máx. 2000 (RN-09). | — |
| Reclasificar | — | Cambiar la categoría de un ticket no `Resuelto` ni `Cerrado` (RN-13). | corregir la categoría, cambiar la categoría (H-52) |
| Categoría | `Categoria` | Uno de `Hardware`, `Software`, `Red`, `Accesos`, `Otros` (RN-02). | — |
| Prioridad | `Prioridad` | Uno de `Critica`, `Alta`, `Media`, `Baja` (RN-02); "Crítica" solo como texto de lectura. | — |
| SLA | — | Tiempo máximo comprometido para resolver un ticket según su prioridad (`00-vision.md` §8; horas en RN-10). | — |
| Vencimiento | `FechaVencimiento` | Fecha y hora límite del SLA (`00-vision.md` §8); creación + horas de SLA (RN-10). | fecha de vencimiento (aceptable) |
| Estado del SLA | `EstadoSla` | Valor calculado: `EnTiempo`, `Vencido`, `Cumplido`, `Incumplido` (RN-11). | — |
| Ticket vencido | — | Ticket con estado del SLA `Vencido` (RN-11, CA-06.1). Alcance del filtro `soloVencidos`: ver H-36. | — |
| Comentario | `Comentario` | Texto ligado a un ticket, de tipo `Solucion` o `Reapertura` (`01-modelo-dominio.md` §1). | — |
| Reporte de vencidos | — | Conteo de tickets vencidos agrupado por técnico y prioridad (RF-06). Estructura: ver H-46. | reportes (H-52) |

---

## 6. Matriz de trazabilidad

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

### Resumen de las secciones 3 y 4

| Severidad | Sección 3 | Sección 4 | Total |
| --------- | --------- | --------- | ----- |
| Crítico   | 1         | 0         | 1     |
| Alto      | 1         | 0         | 1     |
| Medio     | 8         | 3         | 11    |
| Bajo      | 4         | 5         | 9     |
| **Total** | **14**    | **8**     | **22** |
