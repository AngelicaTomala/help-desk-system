using HelpDesk.Domain.Entidades;
using HelpDesk.Tests.Application.Dobles;
using HelpDesk.Tests.Integracion;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Guarda en la base de pruebas los tickets 11 a 15 del elenco del contrato (§1.10), construidos por
/// <see cref="ElencoDeTickets"/>. El 16 no se guarda: no existe antes del ejemplo de crear ticket. Los Id reales los
/// asigna el <c>IDENTITY</c> de la base, así que las pruebas buscan cada ticket por su código.
/// </summary>
internal static class ElencoEnBase
{
    public const string Ticket11 = "HD-000011";
    public const string Ticket12 = "HD-000012";
    public const string Ticket13 = "HD-000013";
    public const string Ticket14 = "HD-000014";
    public const string Ticket15 = "HD-000015";

    private const string Ticket16 = "HD-000016";

    /// <summary>
    /// Guarda los tickets de uno en uno y en orden, para que el Id siga el orden del elenco (el 13 tiene un Id mayor
    /// que el 11, con la misma <c>fechaCreacion</c>), y devuelve el Id de cada código.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, int>> GuardarAsync(BaseDePruebasFixture baseDePruebas)
    {
        var elenco = new TicketRepositorioFalso();
        ElencoDeTickets.SembrarTodos(elenco);

        var ids = new Dictionary<string, int>();
        foreach (var ticket in elenco.Agregados.Where(ticket => ticket.Codigo != Ticket16).OrderBy(ticket => ticket.Codigo))
        {
            // El repositorio falso fija el Id del contrato; la base asigna el suyo.
            typeof(Ticket).GetProperty(nameof(Ticket.Id))!.SetValue(ticket, 0);
            ids[ticket.Codigo] = await baseDePruebas.GuardarAsync(ticket);
        }

        return ids;
    }
}
