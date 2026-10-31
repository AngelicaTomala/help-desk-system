using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets;

/// <summary>
/// Detalle completo del ticket (CA-02.1, contrato §2.2). Lo devuelven la consulta por Id, crear y todas las acciones.
/// </summary>
public sealed record TicketDetalleDto(
    int Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    Categoria Categoria,
    Prioridad Prioridad,
    EstadoTicket Estado,
    PersonaDto Solicitante,
    PersonaDto? Tecnico,
    DateTime FechaCreacion,
    DateTime FechaActualizacion,
    DateTime FechaVencimiento,
    DateTime? FechaResolucion,
    DateTime? FechaCierre,
    EstadoSla EstadoSla,
    IReadOnlyList<ComentarioDto> Comentarios);

/// <summary>
/// Persona como <c>{ id, nombre }</c>; el rol no se expone (decisión 1).
/// </summary>
public sealed record PersonaDto(int Id, string Nombre);

/// <summary>
/// Comentario del detalle, ordenado por <c>fechaCreacion</c> y luego <c>id</c> ascendentes (decisión 68).
/// </summary>
public sealed record ComentarioDto(int Id, TipoComentario Tipo, string Texto, PersonaDto Autor, DateTime FechaCreacion);
