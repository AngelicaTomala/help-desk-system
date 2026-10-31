---
paths:
  - "test/**"
---
# Reglas de la capa Tests

- Marca cada prueba con `[Trait("CA", …)]` y/o `[Trait("RN", …)]`; los 45 CA y las 13 RN deben tener al menos una (RNF-10).
- Nombra las pruebas `Metodo_Escenario_Resultado` (convención del proyecto, E-015).
- Usa `FakeTimeProvider` como reloj falso; nunca esperas reales (RNF-08, ADR-001).
- Prueba Domain sin base de datos ni HTTP (ADR-001).
- Usa LocalDB en las pruebas de integración, no el proveedor InMemory, que no aplica índices únicos ni SEQUENCE (ADR-002).
- Para cada `409`, comprueba que el estado del ticket no cambió (RNF-04).
- Por cada endpoint: sin encabezado → `401`, usuario inexistente → `401` y usuario sin permiso para la operación → `403` (en consultar por Id y en listar, `TICKET_AJENO` y `FILTRO_NO_AUTORIZADO`, decisión 109); además, Id no numérico, `0`, negativo y fuera de `int` → `401` (RNF-01).
- Verifica `Content-Type` y los campos `type`, `title` y `status` de los errores, `codigo` en los errores de negocio (las respuestas de la decisión 94 llevan `traceId`, sin `codigo`), y que un `500` no incluye stack trace (RNF-03).
- Prueba cada validador con los valores frontera de RNF-05 (RNF-05).
- La prueba de rendimiento de RNF-06 es explícita y no se ejecuta con `dotnet test` normal: siembra 10 000 tickets en la base de pruebas y mide el listado (decisión 110).
- En las pruebas de extremo a extremo usa la base de pruebas de la decisión 91 en una colección serializada: antes de cada prueba borra los tickets y comentarios y reinicia la SEQUENCE con `RESTART WITH 1`. La factory sobrescribe `ConnectionStrings:HelpDesk` con la cadena de pruebas, conserva la guarda contra la base `HelpDesk` y usa `FakeTimeProvider` (decisión 99, Enmienda 5 de ADR-002).
