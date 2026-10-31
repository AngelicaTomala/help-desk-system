using HelpDesk.Application.Tickets;

namespace HelpDesk.Application.Abstracciones;

/// <summary>
/// Lectura del detalle del ticket con los nombres del solicitante, del técnico y de los autores (decisión 97), y del
/// listado paginado (decisión 108).
/// </summary>
public interface IConsultasTicket
{
    /// <summary>
    /// Devuelve el detalle o <c>null</c> si el ticket no existe. <paramref name="ahora"/> es la hora que el handler
    /// leyó una sola vez de <c>TimeProvider</c>; con ella se calcula <c>estadoSla</c> mediante
    /// <c>Ticket.CalcularEstadoSla</c> (RN-11, RNF-08).
    /// </summary>
    Task<TicketDetalleDto?> ObtenerDetalleAsync(int ticketId, DateTime ahora, CancellationToken cancellationToken);

    /// <summary>
    /// Devuelve la página pedida de los tickets que cumplen todos los filtros, ordenados por <c>fechaCreacion</c> y
    /// luego por <c>id</c>, ambos descendentes (RN-05, decisión 59). <paramref name="ahora"/> es la hora que el
    /// handler leyó una sola vez de <c>TimeProvider</c>: con ella se filtra <c>SoloVencidos</c> mediante
    /// <c>Ticket.EstaVencido</c> y se calcula <c>estadoSla</c> mediante <c>Ticket.CalcularEstadoSla</c> (RN-11, RNF-08).
    /// </summary>
    Task<PaginaDto<TicketResumenDto>> ListarAsync(
        FiltroListadoTickets filtro,
        DateTime ahora,
        CancellationToken cancellationToken);
}
