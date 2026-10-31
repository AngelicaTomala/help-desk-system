---
paths:
  - "src/HelpDesk.Application/**"
---
# Reglas de la capa Application

- Crea un validador de FluentValidation por comando o consulta e invócalo desde el handler, en el orden de la decisión 6: existencia del recurso de la ruta (`404`), permiso (`403`), validador (`400`) y Domain (`409`); no uses la autovalidación de `FluentValidation.AspNetCore` (ADR-003, decisión 92).
- Reutiliza en los validadores las constantes públicas de Domain; no repitas los números (ADR-003).
- Declara aquí las constantes de paginación, junto a `ListarTicketsValidator` (decisión 34).
- En el listado, comprueba el `403` antes del validador: para un técnico, cualquier `tecnicoId` distinto de su propio Id, incluido uno mal formado, vacío o repetido con algún valor distinto, responde `FILTRO_NO_AUTORIZADO` sin consultar la base; repetido con todos los valores iguales a su Id, llega al validador (`400`) (decisiones 100 y 103).
- `ListarTicketsValidator` recibe los parámetros de la URL como texto: un parámetro repetido o presente con valor vacío responde `400` con su clave; `tecnicoId`, `page` y `pageSize` usan el formato de la decisión 95 y después los rangos de RN-05; `soloVencidos` acepta solo `true` y `false`, sin distinguir mayúsculas, y `false` equivale a no enviarlo; los enums siguen la decisión 58 (decisiones 101 a 105).
- Recorta los textos antes de validar y pasa a Domain el texto ya recortado (decisiones 79 y 83, Enmienda 3 de ADR-003).
- Resuelve en los handlers lo que depende del actor (`403`) o de consultar datos (`400`, p. ej. `TECNICO_NO_VALIDO`) (01-modelo-dominio.md §4, ADR-003).
- Separa `AsignarTicketHandler` y `ReasignarTicketHandler`; `MISMO_TECNICO` solo en el segundo, después de `VALIDACION_FALLIDA` y `TECNICO_NO_VALIDO` (decisiones 72 y 81).
- Obtén `ahora` de `TimeProvider` y pásalo a Domain (RNF-08, decisión 76).
- Define aquí las interfaces de persistencia y de `IGeneradorCodigoTicket`; las implementa Infrastructure (02-arquitectura-c4.md §3).
