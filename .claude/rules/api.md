---
paths:
  - "src/HelpDesk.Api/**"
---
# Reglas de la capa Api

@../../specs/technical/03-contrato-api.md

- Los controladores solo invocan casos de uso de Application; sin condiciones de negocio ni `DbContext` (02-arquitectura-c4.md §3, RNF-08).
- Valida `X-Usuario-Id` en un middleware, antes de resolver el recurso, y obtén el usuario a través de Application (03-contrato-api.md §1.1, decisión 28).
- No uses restricciones de ruta como `{id:int:min(1)}`: un `{id}` no positivo responde `404` `TICKET_NO_ENCONTRADO` con Problem Details (decisión 65).
- Incluye `codigo` y `traceId` en todo Problem Details de negocio; las respuestas de la decisión 94 llevan `traceId`, sin `codigo`. `detail` nunca expone información interna (decisiones 29, 62 y 94).
- Copia al campo `codigo` el `Codigo` de `TransicionInvalidaException`; en los `500` responde siempre `ERROR_INTERNO` y registra la excepción en el log (decisiones 40 y 41, ADR-004).
- Responde los `400` de validación con `ValidationProblemDetails` y claves de `errors` en camelCase (decisión 61).
- Serializa JSON en camelCase y enums como texto; acepta enums sin distinguir mayúsculas y rechaza los numéricos con `400` (decisiones 47, 48 y 58).
- Ignora los campos no previstos del cuerpo (decisión 66).
- Recibe los parámetros de la URL como texto y pásalos a Application sin convertirlos, con lo necesario para que el validador detecte un parámetro repetido o presente con valor vacío; no respondas `400` por ellos antes del handler, que los valida después del `403` (decisiones 101, 103 y 104).
- Ignora los parámetros de la URL desconocidos; sus nombres no distinguen mayúsculas y minúsculas (decisión 106).
- Anota en el documento OpenAPI los valores y el formato de los parámetros de la URL (RNF-09, decisión 101).
- Usa una ruta relativa en el `Location` de crear ticket (decisión 64).
- Swagger UI y el documento OpenAPI no exigen `X-Usuario-Id` (decisión 52).
