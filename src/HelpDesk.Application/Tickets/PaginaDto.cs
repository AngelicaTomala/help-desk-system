namespace HelpDesk.Application.Tickets;

/// <summary>
/// Respuesta paginada de los listados (contrato §1.5, decisión 9).
/// </summary>
public sealed record PaginaDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems);
