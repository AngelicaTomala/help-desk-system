using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets;

/// <summary>
/// Filtros ya validados del listado, con la visibilidad de RN-04 aplicada. Un valor <c>null</c> no filtra. Todos los
/// filtros se combinan (CA-02.3).
/// </summary>
public sealed record FiltroListadoTickets(
    int? SolicitanteId,
    int? TecnicoId,
    EstadoTicket? Estado,
    Prioridad? Prioridad,
    bool SoloVencidos,
    int Page,
    int PageSize);
