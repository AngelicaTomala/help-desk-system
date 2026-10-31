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
- Por cada endpoint: sin encabezado → `401`, usuario inexistente → `401` y rol no autorizado → `403`; además, Id no numérico, `0`, negativo y fuera de `int` → `401` (RNF-01).
- Verifica `Content-Type` y los campos `type`, `title`, `status` y `codigo` de los errores, y que un `500` no incluye stack trace (RNF-03).
- Prueba cada validador con los valores frontera de RNF-05 (RNF-05).
