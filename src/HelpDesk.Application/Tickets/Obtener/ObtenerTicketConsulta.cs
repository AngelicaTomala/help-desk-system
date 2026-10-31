namespace HelpDesk.Application.Tickets.Obtener;

/// <summary>
/// Consulta de un ticket por Id (CA-02.1). El <c>{id}</c> llega como <c>int</c>, ya validado por la Api (decisión 96),
/// así que la consulta no tiene validador.
/// </summary>
public sealed record ObtenerTicketConsulta(int TicketId);
