# 02 · Reglas de negocio

Cada regla está referenciada desde uno o más requisitos en `01-requisitos-funcionales.md` y debe estar implementada en la capa **Domain** (o **Application** cuando depende de datos externos) y cubierta por al menos una prueba.

## Registro y datos del ticket

| ID    | Regla                                                                                                                                                | RF    |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------- | ----- |
| RN-01 | El título es obligatorio y tiene entre 5 y 150 caracteres. La descripción es obligatoria y tiene entre 10 y 2000 caracteres.                         | RF-01 |
| RN-02 | La categoría debe ser una de: `Hardware`, `Software`, `Red`, `Accesos`, `Otros`. La prioridad debe ser una de: `Critica`, `Alta`, `Media`, `Baja`.   | RF-01 |
| RN-03 | Todo ticket nuevo nace en estado `Abierto`, sin técnico asignado, con un código único y consecutivo con formato `HD-` + 6 dígitos (ej. `HD-000001`). | RF-01 |

## Visibilidad

| ID    | Regla                                                                                                                          | RF    |
| ----- | ------------------------------------------------------------------------------------------------------------------------------ | ----- |
| RN-04 | Un **solicitante** solo ve sus propios tickets. Un **técnico** solo ve los tickets asignados a él. Un **supervisor** ve todos. | RF-02 |
| RN-05 | Los listados se paginan con un máximo de 50 registros por página y se ordenan por fecha de creación descendente.               | RF-02 |

## Asignación

| ID    | Regla                                                                                                                                                 | RF    |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ----- |
| RN-06 | Solo un **supervisor** puede asignar o reasignar tickets, y solo a usuarios con rol **Técnico** que estén activos.                                    | RF-03 |
| RN-07 | Solo se puede asignar o reasignar un ticket en estado `Abierto`, `Asignado` o `En progreso`. Tras asignar o reasignar, el ticket queda en `Asignado`. | RF-03 |

## Ciclo de vida

### RN-08 · Transiciones permitidas

| Estado actual | Acción           | Estado nuevo | Quién puede ejecutarla         |
| ------------- | ---------------- | ------------ | ------------------------------ |
| Abierto       | Asignar          | Asignado     | Supervisor                     |
| Asignado      | Reasignar        | Asignado     | Supervisor                     |
| Asignado      | Iniciar atención | En progreso  | Técnico asignado               |
| En progreso   | Reasignar        | Asignado     | Supervisor                     |
| En progreso   | Resolver         | Resuelto     | Técnico asignado               |
| Resuelto      | Cerrar           | Cerrado      | Solicitante dueño o Supervisor |
| Resuelto      | Reabrir          | En progreso  | Solicitante dueño              |

Cualquier otra combinación es inválida. `Cerrado` es un estado final: no admite ninguna transición.

```mermaid
stateDiagram-v2
    [*] --> Abierto
    Abierto --> Asignado: Asignar (Supervisor)
    Asignado --> Asignado: Reasignar (Supervisor)
    Asignado --> EnProgreso: Iniciar atención (Técnico)
    EnProgreso --> Asignado: Reasignar (Supervisor)
    EnProgreso --> Resuelto: Resolver (Técnico)
    Resuelto --> Cerrado: Cerrar (Solicitante / Supervisor)
    Resuelto --> EnProgreso: Reabrir (Solicitante)
    Cerrado --> [*]
```

| ID    | Regla                                                                                                                                                                        | RF    |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- |
| RN-09 | Para **resolver** se exige un comentario de solución; para **reabrir** se exige un motivo. Al reabrir se borra la fecha de resolución y el ticket conserva el mismo técnico. | RF-04 |

## SLA

### RN-10 · Tiempo de SLA por prioridad

| Prioridad | Horas de SLA |
| --------- | ------------ |
| Crítica   | 2            |
| Alta      | 4            |
| Media     | 8            |
| Baja      | 24           |

`FechaVencimiento = FechaCreacion + HorasSla(Prioridad)`, en horas calendario y en UTC (supuestos S-03 y S-04).

### RN-11 · Estado del SLA (valor calculado, no se almacena)

| Condición                                      | Estado del SLA |
| ---------------------------------------------- | -------------- |
| No resuelto y ahora ≤ vencimiento              | `EnTiempo`     |
| No resuelto y ahora > vencimiento              | `Vencido`      |
| Resuelto con fecha de resolución ≤ vencimiento | `Cumplido`     |
| Resuelto con fecha de resolución > vencimiento | `Incumplido`   |

"Resuelto" incluye los estados `Resuelto` y `Cerrado`.

| ID    | Regla                                                                                                                                                                              | RF    |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- |
| RN-12 | Solo el supervisor puede cambiar la prioridad, y solo si el ticket no está `Resuelto` ni `Cerrado`. Al cambiarla, el vencimiento se recalcula desde la fecha de creación original. | RF-05 |

## Matriz regla → requisito

| Regla | RF-01 | RF-02 | RF-03 | RF-04 | RF-05 | RF-06 |
| ----- | ----- | ----- | ----- | ----- | ----- | ----- |
| RN-01 | ✔     |       |       |       |       |       |
| RN-02 | ✔     |       |       |       |       |       |
| RN-03 | ✔     |       |       |       |       |       |
| RN-04 |       | ✔     |       |       |       |       |
| RN-05 |       | ✔     |       |       |       |       |
| RN-06 |       |       | ✔     |       |       |       |
| RN-07 |       |       | ✔     |       |       |       |
| RN-08 |       |       | ✔     | ✔     |       |       |
| RN-09 |       |       |       | ✔     |       |       |
| RN-10 | ✔     |       |       |       | ✔     |       |
| RN-11 |       | ✔     |       |       | ✔     | ✔     |
| RN-12 |       |       |       |       | ✔     |       |
