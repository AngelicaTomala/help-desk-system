---
paths:
  - "src/HelpDesk.Application/**"
---
# Reglas de la capa Application

- Crea un validador de FluentValidation por comando o consulta e invócalo explícitamente antes del handler; no uses la autovalidación de `FluentValidation.AspNetCore` (ADR-003).
- Reutiliza en los validadores las constantes públicas de Domain; no repitas los números (ADR-003).
- Declara aquí las constantes de paginación, junto a `ListarTicketsValidator` (decisión 34).
- Recorta los textos antes de validar y pasa a Domain el texto ya recortado (decisiones 79 y 83, Enmienda 3 de ADR-003).
- Resuelve en los handlers lo que depende del actor (`403`) o de consultar datos (`400`, p. ej. `TECNICO_NO_VALIDO`) (01-modelo-dominio.md §4, ADR-003).
- Separa `AsignarTicketHandler` y `ReasignarTicketHandler`; `MISMO_TECNICO` solo en el segundo, después de `VALIDACION_FALLIDA` y `TECNICO_NO_VALIDO` (decisiones 72 y 81).
- Obtén `ahora` de `TimeProvider` y pásalo a Domain (RNF-08, decisión 76).
- Define aquí las interfaces de persistencia y de `IGeneradorCodigoTicket`; las implementa Infrastructure (02-arquitectura-c4.md §3).
