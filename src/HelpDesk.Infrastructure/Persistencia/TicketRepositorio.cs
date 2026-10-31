using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Escritura del agregado <see cref="Ticket"/> (decisión 97). Los cambios se guardan con <see cref="UnidadDeTrabajo"/>.
/// </summary>
public class TicketRepositorio(HelpDeskDbContext context) : ITicketRepositorio
{
    public void Agregar(Ticket ticket) => context.Tickets.Add(ticket);

    /// <summary>
    /// Carga el agregado rastreado, con sus comentarios, para que los cambios de Domain se guarden.
    /// </summary>
    public Task<Ticket?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken) =>
        context.Tickets
            .Include(t => t.Comentarios)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
}
