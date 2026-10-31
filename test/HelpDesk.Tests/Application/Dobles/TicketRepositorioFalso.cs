using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Entidades;

namespace HelpDesk.Tests.Application.Dobles;

internal sealed class TicketRepositorioFalso : ITicketRepositorio
{
    public List<Ticket> Agregados { get; } = [];

    public void Agregar(Ticket ticket) => Agregados.Add(ticket);
}
