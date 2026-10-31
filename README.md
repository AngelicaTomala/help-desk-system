# help-desk-system

API REST para registrar, asignar, priorizar y consultar tickets de soporte de TI, con control de SLA según la prioridad. Proyecto integrador del curso "Agentic Software Development .NET 10".

## Stack

- .NET 10 · ASP.NET Core Web API · OpenAPI / Swagger UI (no hay frontend; la API se prueba desde Swagger UI)
- Clean Architecture: Domain, Application, Infrastructure, Api
- SQL Server + Entity Framework Core
- FluentValidation · xUnit

## Requisitos para ejecutar

- SDK de .NET 10.
- SQL Server LocalDB, sin Docker: lo usan el desarrollo y las pruebas de integración ([ADR-002](specs/technical/adr/ADR-002-sqlserver-efcore.md)).
- Usuarios cargados como datos semilla: 5 solicitantes, 4 técnicos (uno inactivo) y 1 supervisor ([tabla](specs/technical/01-modelo-dominio.md#8-datos-semilla)). La identidad se envía en el encabezado `X-Usuario-Id`.

La solución es `HelpDesk.slnx`; los comandos de compilación y pruebas están en [CLAUDE.md](CLAUDE.md#comandos).

## Puesta en marcha

Los comandos son para Windows PowerShell 5.1 y se ejecutan desde la raíz del repositorio. Las cadenas de conexión no se guardan en `appsettings*.json` ni en el código: se leen de user-secrets o de variables de entorno ([RNF-02](specs/functional/03-requisitos-no-funcionales.md)).

### 1. Requisitos

| Herramienta | Versión | Comprobación |
| ----------- | ------- | ------------ |
| SDK de .NET 10 | 10.0.x (probado con 10.0.301) | `dotnet --version` |
| SQL Server LocalDB, instancia `MSSQLLocalDB` | La que trae SQL Server Express LocalDB | `sqllocaldb info MSSQLLocalDB` |
| Herramienta global `dotnet-ef` | 10.0.14, la misma versión que los paquetes de EF Core | `dotnet tool list -g` |

Para instalar `dotnet-ef` en esa versión:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.14
```

### 2. Cadena de conexión de la Api

La Api lee la clave `ConnectionStrings:HelpDesk` de los user-secrets del proyecto `src/HelpDesk.Api`, que ya tiene `UserSecretsId`. También la usan los comandos de `dotnet ef`, porque toman la Api como proyecto de inicio.

```powershell
dotnet user-secrets set "ConnectionStrings:HelpDesk" 'Server=(localdb)\MSSQLLocalDB;Database=HelpDesk;Trusted_Connection=True;TrustServerCertificate=True' --project src/HelpDesk.Api
dotnet user-secrets list --project src/HelpDesk.Api
```

### 3. Cadena de conexión de las pruebas de integración

Las pruebas de integración leen la clave `ConnectionStrings:HelpDeskPruebas` de los user-secrets del proyecto `test/HelpDesk.Tests` o, si no está ahí, de la variable de entorno `ConnectionStrings__HelpDeskPruebas` ([decisión 91](docs/auditoria-documentacion.md#9-decisiones-tomadas), [ADR-002, Enmienda 4](specs/technical/adr/ADR-002-sqlserver-efcore.md#enmienda-4--30102026)).

- Debe apuntar a una base **distinta de `HelpDesk`**; por convención, `HelpDesk_Pruebas`. Si apunta a `HelpDesk` o no nombra ninguna base, las pruebas fallan antes de conectarse.
- Las pruebas **borran y recrean esa base** con la migración `Inicial` al empezar cada ejecución, y la conservan al terminar para poder revisarla. No guardes en ella datos que quieras conservar.

Con user-secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:HelpDeskPruebas" 'Server=(localdb)\MSSQLLocalDB;Database=HelpDesk_Pruebas;Trusted_Connection=True;TrustServerCertificate=True' --project test/HelpDesk.Tests
dotnet user-secrets list --project test/HelpDesk.Tests
```

Con una variable de entorno (solo dura lo que la sesión de PowerShell):

```powershell
$env:ConnectionStrings__HelpDeskPruebas = 'Server=(localdb)\MSSQLLocalDB;Database=HelpDesk_Pruebas;Trusted_Connection=True;TrustServerCertificate=True'
```

### 4. Crear la base de desarrollo

La base `HelpDesk` se crea aplicando la migración `Inicial`, que también carga los 10 usuarios semilla:

```powershell
dotnet ef database update --project src/HelpDesk.Infrastructure --startup-project src/HelpDesk.Api
dotnet ef migrations list --project src/HelpDesk.Infrastructure --startup-project src/HelpDesk.Api
```

`migrations list` muestra `20261031022819_Inicial`; si aparece con `(Pending)`, la migración aún no se aplicó.

### 5. Ejecutar la Api y abrir Swagger UI

```powershell
dotnet run --project src/HelpDesk.Api
```

El perfil `http` de `launchSettings.json` escucha en `http://localhost:3001` con el entorno `Development` y no abre el navegador. Swagger UI está en <http://localhost:3001/swagger> y el documento OpenAPI en <http://localhost:3001/openapi/v1.json>; los dos solo existen en `Development`.

Según el supuesto S-02 y el [contrato de la API](specs/technical/03-contrato-api.md#12-encabezado-x-usuario-id), cada petición a la API debe llevar el encabezado `X-Usuario-Id` con el Id de un usuario semilla ([decisión 89](docs/auditoria-documentacion.md#9-decisiones-tomadas)); Swagger UI y el documento OpenAPI no lo exigen.

| Id | Usuario | Rol |
| -- | ------- | --- |
| 1 a 5 | Ana, Bruno, Carla, Diego y Elena | Solicitante |
| 6 a 8 | Fabián, Gabriela y Hugo | Técnico |
| 9 | Irene | Técnico (inactiva) |
| 10 | Jorge | Supervisor |

> **Estado actual:** todavía no existen el middleware que valida `X-Usuario-Id` ni los controladores, así que Swagger UI aún no muestra operaciones.

### 6. Ejecutar las pruebas

```powershell
dotnet test                                                  # todas
dotnet test --filter "FullyQualifiedName~Integracion"        # solo las de integración (LocalDB)
dotnet test --filter "CA=CA-05.1"                            # las de un CA
dotnet test --filter "RN=RN-03"                              # las de una RN
```

Los filtros por CA y RN usan los `[Trait("CA", …)]` y `[Trait("RN", …)]` de cada prueba ([RNF-10](specs/functional/03-requisitos-no-funcionales.md)). Las pruebas de integración necesitan la cadena del paso 3; las demás no usan base de datos.

### 7. Errores frecuentes

| Síntoma | Causa | Solución |
| ------- | ----- | -------- |
| La Api (o `dotnet ef`) falla al arrancar con `InvalidOperationException: Falta la cadena de conexión 'ConnectionStrings:HelpDesk'. Configúrala con user-secrets o con una variable de entorno (RNF-02).` | No hay cadena `ConnectionStrings:HelpDesk` | Configúrala como en el paso 2, o define `$env:ConnectionStrings__HelpDesk` en la sesión |
| Las pruebas de integración fallan con `Falta la cadena de conexión 'ConnectionStrings:HelpDeskPruebas'. Configúrala con user-secrets del proyecto de pruebas o con una variable de entorno (RNF-02).` | No hay cadena de pruebas ni en user-secrets ni en la variable de entorno | Configúrala como en el paso 3 |
| Las pruebas de integración fallan con `La cadena 'ConnectionStrings:HelpDeskPruebas' debe indicar una base de pruebas distinta de 'HelpDesk'.` | La cadena apunta a `HelpDesk` o no tiene `Database` | Usa `Database=HelpDesk_Pruebas` |

## Estructura

| Carpeta                                     | Contenido                                                  |
| ------------------------------------------- | ---------------------------------------------------------- |
| [specs/functional/](specs/functional/)      | Visión, requisitos funcionales, reglas de negocio y RNF    |
| [specs/technical/](specs/technical/)        | Modelo de dominio, arquitectura C4, contrato de la API y ADR |
| [docs/](docs/)                              | Auditoría de la documentación y bitácora de evidencias     |
| `src/`, `test/`                             | Proyectos de la solución (`src/`) y de pruebas (`test/`)   |

## Documentación

- [00 · Visión](specs/functional/00-vision.md)
- [01 · Requisitos funcionales](specs/functional/01-requisitos-funcionales.md)
- [02 · Reglas de negocio](specs/functional/02-reglas-negocio.md)
- [03 · Requisitos no funcionales](specs/functional/03-requisitos-no-funcionales.md)
- [01 · Modelo de dominio](specs/technical/01-modelo-dominio.md)
- [02 · Arquitectura C4](specs/technical/02-arquitectura-c4.md)
- [03 · Contrato de la API](specs/technical/03-contrato-api.md)
- [Índice de ADR](specs/technical/adr/README.md)
- [Auditoría de la documentación](docs/auditoria-documentacion.md)
- [Bitácora de evidencias](docs/evidencias/bitacora.md)
