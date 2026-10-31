using HelpDesk.Domain.Entidades;

namespace HelpDesk.Application.Abstracciones;

/// <summary>
/// Escritura del agregado <see cref="Ticket"/> (decisión 97). Los cambios se persisten con <see cref="IUnidadDeTrabajo"/>.
/// </summary>
public interface ITicketRepositorio
{
    void Agregar(Ticket ticket);

    /// <summary>
    /// Carga el agregado completo, con sus comentarios, para modificarlo; <c>null</c> si el ticket no existe.
    /// </summary>
    Task<Ticket?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
}
