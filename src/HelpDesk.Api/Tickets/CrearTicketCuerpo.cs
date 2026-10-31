namespace HelpDesk.Api.Tickets;

/// <summary>
/// Cuerpo de crear ticket (contrato §2.1). Los enums llegan como texto y los valida el handler (decisiones 92 y 96);
/// el solicitante sale de <c>X-Usuario-Id</c> y un <c>solicitanteId</c> en el cuerpo se ignora (decisiones 54 y 66).
/// </summary>
public sealed record CrearTicketCuerpo(string? Titulo, string? Descripcion, string? Categoria, string? Prioridad);
