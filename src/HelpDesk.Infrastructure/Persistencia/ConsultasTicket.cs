using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Tickets;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Detalle del ticket (CA-02.1, contrato §2.2) con dos consultas sin rastreo, sin importar cuántos comentarios tenga:
/// el ticket con sus comentarios y los nombres de las personas que aparecen en él.
/// </summary>
public class ConsultasTicket(HelpDeskDbContext context) : IConsultasTicket
{
    public async Task<TicketDetalleDto?> ObtenerDetalleAsync(int ticketId, DateTime ahora, CancellationToken cancellationToken)
    {
        // Comentarios por fecha de creación y luego por Id, ascendentes, ordenados en SQL Server (decisión 68).
        var ticket = await context.Tickets
            .AsNoTracking()
            .Include(t => t.Comentarios.OrderBy(c => c.FechaCreacion).ThenBy(c => c.Id))
            .SingleOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        var personaIds = new HashSet<int>(ticket.Comentarios.Select(c => c.AutorId)) { ticket.SolicitanteId };
        if (ticket.TecnicoId is { } asignadoId)
        {
            personaIds.Add(asignadoId);
        }
        var nombres = await context.Usuarios
            .AsNoTracking()
            .Where(u => personaIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Nombre, cancellationToken);

        // Las personas se exponen como { id, nombre }, sin el rol (decisión 1).
        PersonaDto Persona(int id) => new(id, nombres[id]);

        return new TicketDetalleDto(
            ticket.Id,
            ticket.Codigo,
            ticket.Titulo,
            ticket.Descripcion,
            ticket.Categoria,
            ticket.Prioridad,
            ticket.Estado,
            Persona(ticket.SolicitanteId),
            ticket.TecnicoId is { } tecnicoId ? Persona(tecnicoId) : null,
            ticket.FechaCreacion,
            ticket.FechaActualizacion,
            ticket.FechaVencimiento,
            ticket.FechaResolucion,
            ticket.FechaCierre,
            // El estado del SLA se calcula con la regla de Domain, no se almacena (RN-11).
            ticket.CalcularEstadoSla(ahora),
            ticket.Comentarios
                .Select(c => new ComentarioDto(c.Id, c.Tipo, c.Texto, Persona(c.AutorId), c.FechaCreacion))
                .ToList());
    }

    public Task<PaginaDto<TicketResumenDto>> ListarAsync(
        FiltroListadoTickets filtro,
        DateTime ahora,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException("Se implementa en el subpaso de Infrastructure de RF-02.");
}
