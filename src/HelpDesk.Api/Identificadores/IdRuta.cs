using HelpDesk.Application.Excepciones;
using HelpDesk.Application.Identificadores;

namespace HelpDesk.Api.Identificadores;

/// <summary>
/// Lectura del <c>{id}</c> de la ruta. La acción lo recibe como <c>string</c>, sin restricción de ruta, y lo lee al
/// empezar: un <c>{id}</c> con otro formato que el de <see cref="FormatoId"/> responde <c>404</c>
/// <c>TICKET_NO_ENCONTRADO</c> con Problem Details (decisiones 65 y 95), después del <c>401</c> del middleware y del
/// <c>400</c> de lectura del cuerpo (§1.4, decisión 93).
/// </summary>
public static class IdRuta
{
    public static int LeerTicket(string? id) =>
        FormatoId.TryLeer(id, out var valor)
            ? valor
            : throw new RecursoNoEncontradoException("No existe el ticket.");
}
