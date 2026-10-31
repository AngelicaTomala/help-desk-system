using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Tickets;

namespace HelpDesk.Tests.Application.Dobles;

/// <summary>
/// Arma el detalle desde los tickets guardados y los nombres de los usuarios semilla; calcula el estado del SLA con
/// el <c>ahora</c> recibido, como lo hará Infrastructure.
/// </summary>
internal sealed class ConsultasTicketFalsas(TicketRepositorioFalso tickets) : IConsultasTicket
{
    private static readonly Dictionary<int, string> Nombres = new()
    {
        [1] = "Ana Solicitante",
        [6] = "Fabián Técnico",
        [10] = "Jorge Supervisor",
    };

    public DateTime? AhoraRecibido { get; private set; }

    public Task<TicketDetalleDto?> ObtenerDetalleAsync(int ticketId, DateTime ahora, CancellationToken cancellationToken)
    {
        AhoraRecibido = ahora;

        var ticket = tickets.Agregados.SingleOrDefault(candidato => candidato.Id == ticketId && ticketId != 0);
        if (ticket is null)
        {
            return Task.FromResult<TicketDetalleDto?>(null);
        }

        var detalle = new TicketDetalleDto(
            ticket.Id,
            ticket.Codigo,
            ticket.Titulo,
            ticket.Descripcion,
            ticket.Categoria,
            ticket.Prioridad,
            ticket.Estado,
            Persona(ticket.SolicitanteId),
            ticket.TecnicoId is { } tecnicoId ? Persona(tecnicoId) : null,
            ticket.FechaCreacion,
            ticket.FechaActualizacion,
            ticket.FechaVencimiento,
            ticket.FechaResolucion,
            ticket.FechaCierre,
            ticket.CalcularEstadoSla(ahora),
            [.. ticket.Comentarios.Select(comentario => new ComentarioDto(
                comentario.Id, comentario.Tipo, comentario.Texto, Persona(comentario.AutorId), comentario.FechaCreacion))]);

        return Task.FromResult<TicketDetalleDto?>(detalle);
    }

    private static PersonaDto Persona(int id) => new(id, Nombres[id]);
}
