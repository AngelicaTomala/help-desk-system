namespace HelpDesk.Api.Tickets;

/// <summary>
/// Cuerpo de reclasificar (contrato §2.4). La categoría llega como texto y la valida el handler (decisiones 92 y 96).
/// </summary>
public sealed record ReclasificarTicketCuerpo(string? Categoria);
