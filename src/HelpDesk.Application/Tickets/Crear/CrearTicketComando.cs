namespace HelpDesk.Application.Tickets.Crear;

/// <summary>
/// Datos para crear un ticket (CA-01.1). <c>Categoria</c> y <c>Prioridad</c> llegan como texto (decisión 96); el
/// solicitante es el usuario actual, nunca viene en el cuerpo (decisión 54).
/// </summary>
public sealed record CrearTicketComando(string? Titulo, string? Descripcion, string? Categoria, string? Prioridad);
