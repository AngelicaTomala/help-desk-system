using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Tickets;
using HelpDesk.Domain.Entidades;

namespace HelpDesk.Tests.Application.Dobles;

/// <summary>
/// Arma el detalle y el listado desde los tickets guardados y los nombres de los usuarios semilla; filtra los vencidos
/// y calcula el estado del SLA con el <c>ahora</c> recibido y las reglas de Domain, como lo hará Infrastructure.
/// </summary>
internal sealed class ConsultasTicketFalsas(TicketRepositorioFalso tickets) : IConsultasTicket
{
    private static readonly Dictionary<int, string> Nombres = new()
    {
        [1] = "Ana Solicitante",
        [2] = "Bruno Solicitante",
        [3] = "Carla Solicitante",
        [4] = "Diego Solicitante",
        [6] = "Fabián Técnico",
        [7] = "Gabriela Técnico",
        [8] = "Hugo Técnico",
        [10] = "Jorge Supervisor",
    };

    public DateTime? AhoraRecibido { get; private set; }

    public FiltroListadoTickets? FiltroRecibido { get; private set; }

    public int VecesListado { get; private set; }

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

    public Task<PaginaDto<TicketResumenDto>> ListarAsync(
        FiltroListadoTickets filtro,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        AhoraRecibido = ahora;
        FiltroRecibido = filtro;
        VecesListado++;

        var estaVencido = Ticket.EstaVencido(ahora).Compile();
        var filtrados = tickets.Agregados
            .Where(t => filtro.SolicitanteId is null || t.SolicitanteId == filtro.SolicitanteId)
            .Where(t => filtro.TecnicoId is null || t.TecnicoId == filtro.TecnicoId)
            .Where(t => filtro.Estado is null || t.Estado == filtro.Estado)
            .Where(t => filtro.Prioridad is null || t.Prioridad == filtro.Prioridad)
            .Where(t => !filtro.SoloVencidos || estaVencido(t))
            .ToList();

        // Orden de la decisión 59.
        var items = filtrados
            .OrderByDescending(t => t.FechaCreacion)
            .ThenByDescending(t => t.Id)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(t => new TicketResumenDto(
                t.Id,
                t.Codigo,
                t.Titulo,
                t.Estado,
                t.Prioridad,
                t.TecnicoId is { } tecnicoId ? Persona(tecnicoId) : null,
                t.FechaVencimiento,
                t.CalcularEstadoSla(ahora)))
            .ToList();

        return Task.FromResult(new PaginaDto<TicketResumenDto>(items, filtro.Page, filtro.PageSize, filtrados.Count));
    }

    private static PersonaDto Persona(int id) => new(id, Nombres[id]);
}
