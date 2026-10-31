using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets.Obtener;

/// <summary>
/// Consulta un ticket por Id (RF-02, RN-04). Orden de la decisión 92: existencia del ticket (<c>404</c>) y permiso
/// (<c>403</c> <c>TICKET_AJENO</c>); no hay validador ni Domain.
/// </summary>
public class ObtenerTicketHandler(
    IUsuarioActual usuarioActual,
    IConsultasTicket consultas,
    TimeProvider timeProvider)
{
    public async Task<TicketDetalleDto> ManejarAsync(ObtenerTicketConsulta consulta, CancellationToken cancellationToken)
    {
        // Una sola lectura del reloj por operación (RNF-08, decisión 76).
        var ahora = timeProvider.GetUtcNow().UtcDateTime;

        var detalle = await consultas.ObtenerDetalleAsync(consulta.TicketId, ahora, cancellationToken)
            ?? throw new RecursoNoEncontradoException($"No existe el ticket {consulta.TicketId}.");

        // El supervisor ve todos (RN-04, CA-02.6).
        var esAjeno = usuarioActual.Rol switch
        {
            Rol.Solicitante => detalle.Solicitante.Id != usuarioActual.Id,
            Rol.Tecnico => detalle.Tecnico?.Id != usuarioActual.Id,
            _ => false,
        };

        if (esAjeno)
        {
            throw new AccesoDenegadoException(
                CodigosErrorAplicacion.TicketAjeno,
                "El ticket no es del solicitante ni está asignado al técnico.");
        }

        return detalle;
    }
}
