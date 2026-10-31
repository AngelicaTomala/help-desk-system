using FluentValidation;
using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets.Reclasificar;

/// <summary>
/// Reclasifica un ticket (RF-01, RN-13). Orden de la decisión 92: existencia del ticket (<c>404</c>), permiso
/// (<c>403</c>), validador (<c>400</c>) y Domain (<c>409</c> <c>TICKET_NO_MODIFICABLE</c>).
/// </summary>
public class ReclasificarTicketHandler(
    IUsuarioActual usuarioActual,
    IValidator<ReclasificarTicketComando> validador,
    ITicketRepositorio tickets,
    IUnidadDeTrabajo unidadDeTrabajo,
    IConsultasTicket consultas,
    TimeProvider timeProvider)
{
    public async Task<TicketDetalleDto> ManejarAsync(ReclasificarTicketComando comando, CancellationToken cancellationToken)
    {
        var ticket = await tickets.ObtenerPorIdAsync(comando.TicketId, cancellationToken)
            ?? throw new RecursoNoEncontradoException($"No existe el ticket {comando.TicketId}.");

        if (usuarioActual.Rol == Rol.Solicitante)
        {
            throw new AccesoDenegadoException(
                CodigosErrorAplicacion.RolNoAutorizado,
                "Solo el técnico asignado o un supervisor pueden reclasificar tickets.");
        }

        // Un ticket Abierto no tiene técnico, así que solo lo reclasifica un supervisor (hallazgo 11).
        if (usuarioActual.Rol == Rol.Tecnico && ticket.TecnicoId != usuarioActual.Id)
        {
            throw new AccesoDenegadoException(
                CodigosErrorAplicacion.TicketAjeno,
                "Solo el técnico asignado puede reclasificar el ticket.");
        }

        var resultado = await validador.ValidateAsync(comando, cancellationToken);
        if (!resultado.IsValid)
        {
            throw SolicitudInvalidaException.DesdeValidacion(resultado);
        }

        ValoresEnum.TryParse<Categoria>(comando.Categoria, out var categoria);

        // Una sola lectura del reloj por operación (RNF-08, decisión 76).
        var ahora = timeProvider.GetUtcNow().UtcDateTime;

        // Con la categoría actual Domain no cambia nada (decisión 67) y guardar no persiste cambios.
        ticket.Reclasificar(categoria, ahora);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return await consultas.ObtenerDetalleAsync(ticket.Id, ahora, cancellationToken)
            ?? throw new InvalidOperationException($"No se encontró el ticket {ticket.Id} reclasificado.");
    }
}
