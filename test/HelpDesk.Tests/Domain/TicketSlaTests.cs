using HelpDesk.Domain.Enumeraciones;
using static HelpDesk.Tests.Domain.TicketsDePrueba;

namespace HelpDesk.Tests.Domain;

public class TicketSlaTests
{
    // Ticket Media creado a las 08:00: vence a las 16:00 (RN-10).
    private static readonly DateTime Vencimiento = Creacion.AddHours(8);

    [Theory]
    [Trait("RN", "RN-11")]
    [Trait("CA", "CA-05.2")]
    [InlineData(-60)]
    [InlineData(0)]
    public void CalcularEstadoSla_NoResueltoHastaElVencimiento_EsEnTiempo(int minutosRespectoAlVencimiento)
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);

        var estado = ticket.CalcularEstadoSla(Vencimiento.AddMinutes(minutosRespectoAlVencimiento));

        Assert.Equal(EstadoSla.EnTiempo, estado);
    }

    [Fact]
    [Trait("RN", "RN-11")]
    [Trait("CA", "CA-05.2")]
    public void CalcularEstadoSla_NoResueltoDespuesDelVencimiento_EsVencido()
    {
        var ticket = EnEstado(EstadoTicket.Asignado);

        var estado = ticket.CalcularEstadoSla(Vencimiento.AddSeconds(1));

        Assert.Equal(EstadoSla.Vencido, estado);
    }

    [Theory]
    [Trait("RN", "RN-11")]
    [Trait("CA", "CA-05.3")]
    [InlineData(-60)]
    [InlineData(0)]
    public void CalcularEstadoSla_ResueltoHastaElVencimiento_EsCumplido(int minutosRespectoAlVencimiento)
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        ticket.Resolver(Solucion, TecnicoId, Vencimiento.AddMinutes(minutosRespectoAlVencimiento));

        var estado = ticket.CalcularEstadoSla(Vencimiento.AddDays(2));

        Assert.Equal(EstadoSla.Cumplido, estado);
    }

    [Fact]
    [Trait("RN", "RN-11")]
    [Trait("CA", "CA-05.3")]
    public void CalcularEstadoSla_ResueltoDespuesDelVencimiento_EsIncumplido()
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        ticket.Resolver(Solucion, TecnicoId, Vencimiento.AddSeconds(1));

        var estado = ticket.CalcularEstadoSla(Vencimiento.AddSeconds(1));

        Assert.Equal(EstadoSla.Incumplido, estado);
    }

    [Fact]
    [Trait("RN", "RN-11")]
    [Trait("CA", "CA-05.3")]
    public void CalcularEstadoSla_TicketCerradoResueltoATiempo_EsCumplidoAunqueAhoraSupereElVencimiento()
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        ticket.Resolver(Solucion, TecnicoId, Vencimiento.AddHours(-1));
        ticket.Cerrar(Vencimiento.AddHours(5));

        var estado = ticket.CalcularEstadoSla(Vencimiento.AddDays(3));

        Assert.Equal(EstadoSla.Cumplido, estado);
    }

    [Fact]
    [Trait("RN", "RN-11")]
    [Trait("RN", "RN-09")]
    public void CalcularEstadoSla_TicketReabiertoDespuesDelVencimiento_ConservaVencimientoYEsVencido()
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        ticket.Resolver(Solucion, TecnicoId, Vencimiento.AddHours(-1));
        Assert.Equal(EstadoSla.Cumplido, ticket.CalcularEstadoSla(Vencimiento.AddHours(1)));

        ticket.Reabrir(Motivo, SolicitanteId, Vencimiento.AddHours(1));

        Assert.Equal(Vencimiento, ticket.FechaVencimiento);
        Assert.Equal(EstadoSla.Vencido, ticket.CalcularEstadoSla(Vencimiento.AddHours(1)));
    }
}
