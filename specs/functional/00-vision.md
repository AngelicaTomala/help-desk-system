# 00 · Visión del producto — Help Desk System

| Campo    | Valor                                |
| -------- | ------------------------------------ |
| Proyecto | Help Desk System                     |
| Curso    | Agentic Software Development .NET 10 |
| Autora   | Angélica Tomalá Torres               |
| Versión  | 1.1                                  |
| Fecha    | 28/10/2026                           |
| Estado   | Borrador para revisión               |

## 1. Problema

En el área de TI las solicitudes de soporte llegan por correo, llamadas o mensajes sueltos.

Esto provoca:

- Solicitudes que se pierden o se atienden dos veces.
- Falta de prioridad: un problema crítico espera igual que uno menor.
- Imposibilidad de saber quién atiende qué y en qué estado está cada solicitud.
- Ninguna medición del cumplimiento de tiempos de respuesta (SLA).

## 2. Objetivo

Construir una API REST que centralice el ciclo de vida de los tickets de soporte de TI: registro, asignación a un técnico, seguimiento por estados y control del SLA según prioridad.

## 3. Usuarios y roles

| Rol             | Descripción                               | Puede                                                                                                                                  |
| --------------- | ----------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| **Solicitante** | Colaborador que reporta un problema       | Crear tickets, consultar sus propios tickets, cerrar o reabrir un ticket resuelto                                                      |
| **Técnico**     | Personal de soporte que atiende los tickets | Consultar tickets asignados a él, reclasificar sus tickets asignados, iniciar atención y resolver                                    |
| **Supervisor**  | Coordinador del área de TI                | Consultar todos los tickets, consultar los técnicos activos, asignar y reasignar técnicos, reclasificar, cambiar prioridad, cerrar tickets y ver el reporte de tickets vencidos |

## 4. Alcance

### Incluido (MVP)

| ID    | Funcionalidad                            | Prioridad   |
| ----- | ---------------------------------------- | ----------- |
| RF-01 | Registrar y reclasificar el ticket       | Obligatorio |
| RF-02 | Consultar, filtrar y paginar tickets     | Obligatorio |
| RF-03 | Asignar y reasignar técnico              | Obligatorio |
| RF-04 | Cambiar estado (máquina de estados)      | Obligatorio |
| RF-05 | Calcular y controlar SLA según prioridad | Obligatorio |
| RF-06 | Reporte de tickets vencidos              | Deseable    |

### Fuera de alcance

- Interfaz gráfica (frontend). La API se prueba desde Swagger UI.
- Autenticación con JWT (queda como mejora deseable; ver supuesto S-02).
- Despliegue con contenedores (Docker).
- Notificaciones por correo, SLA en horario laboral, adjuntos y base de conocimiento.
- Desactivar usuarios y redistribuir los tickets de un técnico desactivado (los usuarios son datos semilla, [S-01](#5-supuestos)).
- Eliminar tickets: el MVP no elimina tickets.
- Historial de cambios del ticket: solo se guardan la fecha de creación y la fecha de última actualización.
- Control de concurrencia: dos modificaciones simultáneas sobre el mismo ticket no se detectan. Es una limitación conocida del MVP.

## 5. Supuestos

| ID   | Supuesto                                                                                                                                           |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| S-01 | Los usuarios (solicitantes, técnicos y supervisores) se cargan como datos iniciales; no hay registro de usuarios en el MVP. Los datos semilla son 5 solicitantes, 4 técnicos (uno de ellos inactivo) y 1 supervisor. |
| S-02 | Mientras no exista JWT, la identidad del usuario se recibe en el encabezado `X-Usuario-Id`. Es una limitación de seguridad conocida y documentada. |
| S-03 | El SLA se calcula en horas calendario (24/7), no en horario laboral.                                                                               |
| S-04 | Las fechas se almacenan en UTC.                                                                                                                    |

## 6. Stack tecnológico

- .NET 10 · ASP.NET Core (API REST) · OpenAPI / Swagger UI
- Clean Architecture: Domain, Application, Infrastructure, Api
- SQL Server + Entity Framework Core
- FluentValidation · xUnit
- Claude Code como agente de desarrollo

## 7. Criterio de éxito

- Los RF obligatorios funcionan y están cubiertos por pruebas.
- Cada RF es trazable hasta su historia, endpoint, prueba y commit.
- El uso de IA queda evidenciado con prompts, resultados y decisiones humanas.

## 8. Glosario

| Término     | Definición                                                            |
| ----------- | --------------------------------------------------------------------- |
| Ticket      | Solicitud de soporte registrada en el sistema                         |
| SLA         | Tiempo máximo comprometido para resolver un ticket según su prioridad |
| Vencimiento | Fecha y hora límite del SLA de un ticket                              |
| Reasignar   | Cambiar el técnico responsable de un ticket                           |

Términos canónicos completos y sinónimos a evitar: [glosario propuesto de la auditoría](../../docs/auditoria-documentacion.md#5-glosario-propuesto).
