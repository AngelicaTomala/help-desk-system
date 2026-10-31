using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using static HelpDesk.Tests.Domain.TicketsDePrueba;

namespace HelpDesk.Tests.Domain;

/// <summary>
/// La expresión "ticket vencido" y <see cref="Ticket.CalcularEstadoSla"/> usan la misma condición de RN-11 (decisión 107).
/// </summary>
public class TicketVencidoTests
{
    // Ticket Media creado a las 08:00: vence a las 16:00 (RN-10).
    private static readonly DateTime Vencimiento = Creacion.AddHours(8);

    [Theory]
    [Trait("RN", "RN-11")]
    [InlineData(EstadoTicket.Abierto, -1)]
    [InlineData(EstadoTicket.Abierto, 0)]
    [InlineData(EstadoTicket.Abierto, 1)]
    [InlineData(EstadoTicket.Asignado, -1)]
    [InlineData(EstadoTicket.Asignado, 0)]
    [InlineData(EstadoTicket.Asignado, 1)]
    [InlineData(EstadoTicket.EnProgreso, -1)]
    [InlineData(EstadoTicket.EnProgreso, 0)]
    [InlineData(EstadoTicket.EnProgreso, 1)]
    [InlineData(EstadoTicket.Resuelto, -1)]
    [InlineData(EstadoTicket.Resuelto, 0)]
    [InlineData(EstadoTicket.Resuelto, 1)]
    [InlineData(EstadoTicket.Cerrado, -1)]
    [InlineData(EstadoTicket.Cerrado, 0)]
    [InlineData(EstadoTicket.Cerrado, 1)]
    public void EstaVencido_CadaEstadoAlrededorDelVencimiento_CoincideConCalcularEstadoSla(EstadoTicket estado, int segundos)
    {
        var ticket = EnEstado(estado);

        AfirmarCoinciden(ticket, Vencimiento.AddSeconds(segundos));
    }

    [Theory]
    [Trait("RN", "RN-11")]
    [InlineData(false, -1)]
    [InlineData(false, 1)]
    [InlineData(true, -1)]
    [InlineData(true, 1)]
    public void EstaVencido_ResueltoDespuesDelVencimiento_CoincideConCalcularEstadoSla(bool cerrado, int segundos)
    {
        // Incumplido: resuelto tarde, pero no vencido (soloVencidos no incluye Incumplido, CA-02.3).
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        ticket.Resolver(Solucion, TecnicoId, Vencimiento.AddHours(1));
        if (cerrado)
        {
            ticket.Cerrar(Vencimiento.AddHours(2));
        }

        var ahora = Vencimiento.AddHours(3).AddSeconds(segundos);

        Assert.False(Ticket.EstaVencido(ahora).Compile()(ticket));
        AfirmarCoinciden(ticket, ahora);
    }

    [Theory]
    [Trait("RN", "RN-11")]
    [Trait("RN", "RN-09")]
    [InlineData(-1)]
    [InlineData(1)]
    public void EstaVencido_TicketReabierto_CoincideConCalcularEstadoSla(int segundos)
    {
        var ticket = EnEstado(EstadoTicket.Resuelto);
        ticket.Reabrir(Motivo, SolicitanteId, Creacion.AddHours(1));

        AfirmarCoinciden(ticket, Vencimiento.AddSeconds(segundos));
    }

    [Fact]
    [Trait("RN", "RN-11")]
    public void EstaVencido_NoResueltoDespuesDelVencimiento_EsVerdadero()
    {
        var ticket = EnEstado(EstadoTicket.Asignado);

        Assert.True(Ticket.EstaVencido(Vencimiento.AddSeconds(1)).Compile()(ticket));
        Assert.False(Ticket.EstaVencido(Vencimiento).Compile()(ticket));
    }

    private static void AfirmarCoinciden(Ticket ticket, DateTime ahora)
    {
        var vencidoSegunExpresion = Ticket.EstaVencido(ahora).Compile()(ticket);

        Assert.Equal(ticket.CalcularEstadoSla(ahora) == EstadoSla.Vencido, vencidoSegunExpresion);
    }
}
