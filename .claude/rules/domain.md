---
paths:
  - "src/HelpDesk.Domain/**"
---
# Reglas de la capa Domain

- No uses EF Core, ASP.NET Core ni FluentValidation; Domain valida con sus propias excepciones (02-arquitectura-c4.md §3, ADR-003).
- Expón los atributos de las entidades con setters privados; solo los métodos de comportamiento cambian el estado (convención del proyecto, E-015) (ADR-003: "un Ticket inválido nunca puede existir").
- Crea tickets solo con la fábrica estática `Ticket.Crear(codigo, titulo, descripcion, categoria, prioridad, solicitanteId, ahora)`; no valides el formato de `codigo` (decisión 78, 01-modelo-dominio.md §1 y §5).
- Recibe la hora como parámetro `ahora` (UTC) en todo método que crea o modifica; nunca leas `DateTime.Now` ni `DateTime.UtcNow` (decisión 76, RNF-08).
- Implementa `Asignar` (solo desde `Abierto`) y `Reasignar` (solo desde `Asignado` o `EnProgreso`) como métodos distintos (decisión 75).
- Recibe `autorId` en `Resolver` y `Reabrir` y no valides quién es el autor (decisión 77).
- Lanza `TransicionInvalidaException` con `Codigo` `TRANSICION_INVALIDA` o `TICKET_NO_MODIFICABLE` según el caso (decisión 40, 01-modelo-dominio.md §4).
- Lanza `InvarianteVioladaException` con `Codigo` `ERROR_INTERNO` (decisión 80).
- Declara una sola vez los límites como constantes públicas: `Ticket.TituloLongitudMinima/Maxima`, `Ticket.DescripcionLongitudMinima/Maxima` y `Comentario.TextoLongitudMaxima` (RN-01, RN-09, ADR-003).
- No recortes textos: rechaza con `InvarianteVioladaException` el texto en blanco o con espacios en los extremos según `string.Trim()`, y mide las longitudes sobre el texto recibido (decisiones 79 y 83).
- Rechaza un valor de `Categoria` o `Prioridad` no definido en el enum, antes de la guarda de estado; `PoliticaSla.HorasPara` también (decisión 84).
- En `Reclasificar` y `CambiarPrioridad`, aplica primero la guarda de estado; con el valor actual no cambies nada, tampoco `FechaActualizacion` (decisiones 67 y 70).
