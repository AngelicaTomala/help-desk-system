using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Tickets;
using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Detalle del ticket (CA-02.1, contrato §2.2) con dos consultas sin rastreo, sin importar cuántos comentarios tenga:
/// el ticket con sus comentarios y los nombres de las personas que aparecen en él. Listado paginado (CA-02.3, RN-05) con
/// el filtrado, el orden, el conteo y la paginación en SQL Server (RNF-06): dos consultas, sin importar el tamaño de la página.
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

    public async Task<PaginaDto<TicketResumenDto>> ListarAsync(
        FiltroListadoTickets filtro,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        // Los filtros se combinan y se aplican en SQL Server (CA-02.3, RNF-06).
        var consulta = context.Tickets.AsNoTracking();
        if (filtro.SolicitanteId is { } solicitanteId)
        {
            consulta = consulta.Where(t => t.SolicitanteId == solicitanteId);
        }

        if (filtro.TecnicoId is { } tecnicoId)
        {
            consulta = consulta.Where(t => t.TecnicoId == tecnicoId);
        }

        if (filtro.Estado is { } estado)
        {
            consulta = consulta.Where(t => t.Estado == estado);
        }

        if (filtro.Prioridad is { } prioridad)
        {
            consulta = consulta.Where(t => t.Prioridad == prioridad);
        }

        if (filtro.SoloVencidos)
        {
            // Condición única de RN-11 en Domain; la hora viaja como parámetro (decisión 107).
            consulta = consulta.Where(Ticket.EstaVencido(ahora));
        }

        var totalItems = await consulta.CountAsync(cancellationToken);

        // En long, porque con una página muy alta el producto desborda int. Más allá de la última página no se lee nada.
        var desplazamiento = (long)(filtro.Page - 1) * filtro.PageSize;
        if (desplazamiento >= totalItems)
        {
            return new PaginaDto<TicketResumenDto>([], filtro.Page, filtro.PageSize, totalItems);
        }

        // Fecha de creación y luego Id, descendentes (decisión 59). El nombre del técnico sale en la misma consulta.
        var filas = await consulta
            .OrderByDescending(t => t.FechaCreacion)
            .ThenByDescending(t => t.Id)
            .Skip((int)desplazamiento)
            .Take(filtro.PageSize)
            .Select(t => new
            {
                Ticket = t,
                TecnicoNombre = context.Usuarios.Where(u => u.Id == t.TecnicoId).Select(u => u.Nombre).FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var items = filas
            .Select(f => new TicketResumenDto(
                f.Ticket.Id,
                f.Ticket.Codigo,
                f.Ticket.Titulo,
                f.Ticket.Estado,
                f.Ticket.Prioridad,
                // null si no hay técnico asignado (decisión 49).
                f.Ticket.TecnicoId is { } asignadoId ? new PersonaDto(asignadoId, f.TecnicoNombre!) : null,
                f.Ticket.FechaVencimiento,
                // Sobre la página ya leída, con la regla de Domain (RN-11).
                f.Ticket.CalcularEstadoSla(ahora)))
            .ToList();

        return new PaginaDto<TicketResumenDto>(items, filtro.Page, filtro.PageSize, totalItems);
    }
}
