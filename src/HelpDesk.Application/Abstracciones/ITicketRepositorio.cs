using HelpDesk.Domain.Entidades;

namespace HelpDesk.Application.Abstracciones;

/// <summary>
/// Escritura del agregado <see cref="Ticket"/> (decisión 97). Los cambios se persisten con <see cref="IUnidadDeTrabajo"/>.
/// </summary>
public interface ITicketRepositorio
{
    void Agregar(Ticket ticket);
}
