namespace HelpDesk.Application.Tickets.Reclasificar;

/// <summary>
/// Datos para reclasificar un ticket (CA-01.6). <c>TicketId</c> llega como <c>int</c>, ya validado por la Api, y
/// <c>Categoria</c> como texto (decisión 96).
/// </summary>
public sealed record ReclasificarTicketComando(int TicketId, string? Categoria);
