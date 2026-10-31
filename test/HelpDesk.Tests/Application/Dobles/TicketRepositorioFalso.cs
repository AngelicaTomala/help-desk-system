using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Entidades;

namespace HelpDesk.Tests.Application.Dobles;

internal sealed class TicketRepositorioFalso : ITicketRepositorio
{
    public List<Ticket> Agregados { get; } = [];

    public void Agregar(Ticket ticket) => Agregados.Add(ticket);

    public Task<Ticket?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(Agregados.SingleOrDefault(ticket => ticket.Id == id && id != 0));

    /// <summary>
    /// Agrega un ticket ya existente con el Id indicado, como si viniera de la base.
    /// </summary>
    public Ticket Sembrar(Ticket ticket, int id)
    {
        typeof(Ticket).GetProperty(nameof(Ticket.Id))!.SetValue(ticket, id);
        Agregados.Add(ticket);
        return ticket;
    }
}
