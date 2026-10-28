# 03 · Requisitos no funcionales

| Campo   | Valor                  |
| ------- | ---------------------- |
| Versión | 1.0                    |
| Fecha   | 27/10/2026             |
| Estado  | Borrador para revisión |

Convención de IDs:

- **RNF-xx**: requisito no funcional.
- En el criterio de verificación, **Prueba** indica una prueba automatizada (xUnit) y **Revisión** indica una inspección concreta de código, configuración o documento.

Alcance: solo aplican a la API REST del MVP (sin frontend ni Docker) y respetan los supuestos S-02, S-03 y S-04 de `00-vision.md`.

## Requisitos

| ID     | Categoría         | Requisito                                                                                                                                                                                                                              | Criterio de verificación                                                                                                                                                                                                                                                                                                        | RF/RN relacionados                                                            |
| ------ | ----------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| RNF-01 | Seguridad         | Todos los endpoints exigen el encabezado `X-Usuario-Id` (S-02). Si falta, no es un Id válido o el usuario no existe, se responde `401`. Si el rol o el dueño no corresponden a la operación, se responde `403`.                        | **Prueba:** por cada endpoint, una prueba de integración sin encabezado → `401`, con usuario inexistente → `401` y con rol no autorizado → `403`. El 100 % de los endpoints tiene las tres pruebas.                                                                                                                             | S-02, RN-03, RN-04, RN-06, RN-08, RN-12, RN-13, CA-01.5, CA-01.8, CA-01.10, CA-02.6, CA-02.7, CA-03.5, CA-03.7, CA-04.7, CA-05.6, CA-06.3 |
| RNF-02 | Seguridad         | El acceso a datos usa EF Core con consultas parametrizadas y la cadena de conexión no se guarda en el repositorio (se lee de user-secrets o variables de entorno).                                                                     | **Revisión:** búsqueda en `src/` de `FromSqlRaw`/`ExecuteSqlRaw` con concatenación o interpolación = 0 resultados; `appsettings*.json` versionados no contienen usuario ni contraseña de SQL Server.                                                                                                                            | —                                                                             |
| RNF-03 | Manejo de errores | Todas las respuestas de error (4xx y 5xx) usan el formato Problem Details (RFC 9457, `application/problem+json`). Un error no controlado responde `500` sin stack trace ni mensajes internos fuera del entorno Development.            | **Prueba:** pruebas de integración verifican `Content-Type` y los campos `type`, `title` y `status` en respuestas `400`, `401`, `403`, `404` y `409`; una prueba que fuerza una excepción verifica `500` sin stack trace en el cuerpo.                                                                                          | Todos los RF                                                                  |
| RNF-04 | Manejo de errores | Los códigos HTTP se asignan de forma uniforme: `400` validación, `401` identidad, `403` permiso, `404` recurso de la ruta inexistente (un dato inválido del cuerpo, como un técnico inexistente, es `400`), `409` transición o estado no permitido. Cuando aplican varios errores se evalúan en este orden: `401` → `404` → `403` → `400` → `409`. Una operación rechazada no modifica datos. | **Prueba:** cada CA que espera un código de error tiene una prueba que verifica el código y, para `409`, que el estado del ticket no cambió tras la petición. | RN-07, RN-08, RN-12, RN-13, CA-01.7, CA-01.9, CA-02.2, CA-03.3, CA-03.4, CA-04.6, CA-05.5 |
| RNF-05 | Validación        | Los datos de entrada se validan con FluentValidation antes de llegar al dominio. La respuesta `400` incluye el detalle por campo (`errors`). Los parámetros de paginación fuera de rango (`page` < 1, `pageSize` < 1 o > 100) y los valores de filtro inválidos responden `400`. | **Prueba:** pruebas unitarias de cada validador con valores frontera: título de 4/5/150/151 caracteres, descripción de 9/10/2000/2001, comentario de solución y motivo de reapertura de 2000/2001, categoría y prioridad inexistentes (también al reclasificar y al cambiar la prioridad), `page` 0/1, `pageSize` 0/1/100/101. | RN-01, RN-02, RN-05, RN-06, RN-09, RN-13, CA-01.3, CA-01.4, CA-01.9, CA-02.4, CA-02.8, CA-03.8, CA-04.3, CA-04.8, CA-04.9, CA-05.7 |
| RNF-06 | Rendimiento       | El filtrado, el ordenamiento, el conteo y la paginación de los listados se ejecutan en SQL Server, no en memoria. Existen índices sobre `FechaCreacion`, `Estado`, `TecnicoId` y `SolicitanteId`, y un índice único sobre `Codigo`.    | **Revisión:** el SQL generado (log de EF Core o `ToQueryString()`) de `GET /tickets` contiene `WHERE`, `ORDER BY ... DESC` y `OFFSET/FETCH`; la migración define los índices. **Prueba manual:** con 10 000 tickets semilla, `GET /tickets?pageSize=50` responde en menos de 1 s en el entorno local (promedio de 10 llamadas). | RN-03, RN-04, RN-05, CA-02.3, CA-02.4                                         |
| RNF-07 | Mantenibilidad    | Se respeta la regla de dependencias de Clean Architecture: Domain no referencia otros proyectos ni paquetes de EF Core o ASP.NET Core; Application solo referencia Domain; Infrastructure y Api referencian las capas internas.        | **Revisión:** los `ProjectReference` y `PackageReference` de cada `.csproj` cumplen la regla. Opcional: prueba de arquitectura que falla si Domain depende de otra capa.                                                                                                                                                        | Todas las RN                                                                  |
| RNF-08 | Mantenibilidad    | Las reglas de negocio viven en Domain (o Application si dependen de datos externos), no en los controladores. La hora actual se obtiene de una abstracción inyectable (`TimeProvider`) y todas las fechas se manejan en UTC.           | **Revisión:** los controladores no contienen condiciones de negocio; búsqueda de `DateTime.Now` en `src/` = 0 resultados. **Prueba:** las pruebas de SLA usan un reloj falso para verificar `EnTiempo`/`Vencido`/`Cumplido`/`Incumplido` sin esperas reales.                                                                    | S-03, S-04, RN-08, RN-10, RN-11, RN-12                                        |
| RNF-09 | Documentación     | La API publica un documento OpenAPI y Swagger UI en Development. Cada operación declara sus códigos de respuesta posibles, sus esquemas y el encabezado `X-Usuario-Id`; los enums se muestran como texto (`Abierto`, `Critica`, etc.). | **Revisión:** en el `openapi.json` generado, cada operación lista todos los códigos que exigen sus CA y el parámetro de encabezado `X-Usuario-Id`; cada endpoint se puede ejecutar desde Swagger UI.                                                                                                                            | Todos los RF, S-02                                                            |
| RNF-10 | Pruebas           | Cada CA (45) y cada RN (13) tiene al menos una prueba xUnit identificada con su ID en el nombre o en un atributo `[Trait]`. Se incluyen casos límite, como valores frontera y la unicidad del código de ticket.                        | **Revisión:** la matriz de trazabilidad (ID → prueba) no tiene huecos. **Prueba:** `dotnet test` pasa al 100 %; `dotnet test --filter "CA=CA-01.1"` (y equivalentes) ejecuta al menos una prueba por ID.                                                                                                                        | Todos los RF y RN                                                             |

## Resumen por categoría

| Categoría         | RNF            |
| ----------------- | -------------- |
| Seguridad         | RNF-01, RNF-02 |
| Manejo de errores | RNF-03, RNF-04 |
| Validación        | RNF-05         |
| Rendimiento       | RNF-06         |
| Mantenibilidad    | RNF-07, RNF-08 |
| Documentación     | RNF-09         |
| Pruebas           | RNF-10         |
| **Total**         | **10**         |

## Fuera de alcance

- Autenticación con JWT, HTTPS con certificados propios y limitación de peticiones (rate limiting): quedan como mejoras posteriores al MVP.
- Pruebas de carga formales y métricas de despliegue (no hay Docker ni entorno productivo en el MVP).
- Control de concurrencia: limitación conocida del MVP (ver `00-vision.md` §4).
