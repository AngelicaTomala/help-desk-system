using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets;

/// <summary>
/// Resumen de cada ticket del listado (CA-02.4, decisión 21).
/// </summary>
public sealed record TicketResumenDto(
    int Id,
    string Codigo,
    string Titulo,
    EstadoTicket Estado,
    Prioridad Prioridad,
    PersonaDto? Tecnico,
    DateTime FechaVencimiento,
    EstadoSla EstadoSla);
