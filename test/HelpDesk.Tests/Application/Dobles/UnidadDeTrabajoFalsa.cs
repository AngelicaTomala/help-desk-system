using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Entidades;

namespace HelpDesk.Tests.Application.Dobles;

/// <summary>
/// Al guardar asigna Id a los tickets nuevos, como la columna de identidad de EF Core.
/// </summary>
internal sealed class UnidadDeTrabajoFalsa(TicketRepositorioFalso tickets) : IUnidadDeTrabajo
{
    private int _siguienteId = 1;

    public int VecesGuardado { get; private set; }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
    {
        foreach (var ticket in tickets.Agregados.Where(ticket => ticket.Id == 0))
        {
            typeof(Ticket).GetProperty(nameof(Ticket.Id))!.SetValue(ticket, _siguienteId++);
        }

        VecesGuardado++;
        return Task.CompletedTask;
    }
}
