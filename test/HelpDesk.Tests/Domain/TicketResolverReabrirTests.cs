using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using static HelpDesk.Tests.Domain.TicketsDePrueba;

namespace HelpDesk.Tests.Domain;

public class TicketResolverReabrirTests
{
    [Fact]
    [Trait("RN", "RN-08")]
    [Trait("RN", "RN-09")]
    [Trait("CA", "CA-04.2")]
    public void Resolver_TicketEnProgreso_QuedaResueltoConFechaYComentarioSolucion()
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);

        ticket.Resolver(Solucion, TecnicoId, Ahora);

        Assert.Equal(EstadoTicket.Resuelto, ticket.Estado);
        Assert.Equal(Ahora, ticket.FechaResolucion);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
        var comentario = Assert.Single(ticket.Comentarios);
        Assert.Equal(TipoComentario.Solucion, comentario.Tipo);
        Assert.Equal(Solucion, comentario.Texto);
        Assert.Equal(TecnicoId, comentario.AutorId);
        Assert.Equal(Ahora, comentario.FechaCreacion);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public void Resolver_SolucionEnLimiteMaximo_SeAcepta()
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        var solucion = Texto(Comentario.TextoLongitudMaxima);

        ticket.Resolver(solucion, TecnicoId, Ahora);

        Assert.Equal(solucion, Assert.Single(ticket.Comentarios).Texto);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public void Resolver_SolucionSobreLimiteMaximo_LanzaInvarianteVioladaSinCambios()
    {
        AfirmarResolverRechazado(EstadoTicket.EnProgreso, Texto(Comentario.TextoLongitudMaxima + 1));
    }

    [Theory]
    [Trait("RN", "RN-09")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\tWi-Fi")]
    [InlineData("Wi-Fi ")]
    public void Resolver_SolucionEnBlancoOConEspaciosEnLosExtremos_LanzaInvarianteVioladaSinCambios(string? solucion)
    {
        AfirmarResolverRechazado(EstadoTicket.EnProgreso, solucion);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public void Resolver_SolucionConSaltoDeLineaEnMedio_SeAcepta()
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        const string solucion = "Se reinició el servicio.\nSe verificó el acceso.";

        ticket.Resolver(solucion, TecnicoId, Ahora);

        Assert.Equal(solucion, Assert.Single(ticket.Comentarios).Texto);
    }

    [Fact]
    [Trait("RN", "RN-08")]
    [Trait("RN", "RN-09")]
    public void Resolver_TextoEnBlancoEnEstadoNoPermitido_LanzaInvarianteViolada()
    {
        // El texto se valida antes que el estado (decisión 85).
        AfirmarResolverRechazado(EstadoTicket.Abierto, "");
    }

    [Fact]
    [Trait("RN", "RN-08")]
    [Trait("RN", "RN-09")]
    [Trait("CA", "CA-04.5")]
    public void Reabrir_TicketResuelto_VuelveAEnProgresoConMismoTecnicoYSinFechaResolucion()
    {
        var ticket = EnEstado(EstadoTicket.Resuelto);
        var vencimiento = ticket.FechaVencimiento;

        ticket.Reabrir(Motivo, SolicitanteId, Ahora);

        Assert.Equal(EstadoTicket.EnProgreso, ticket.Estado);
        Assert.Equal(TecnicoId, ticket.TecnicoId);
        Assert.Null(ticket.FechaResolucion);
        Assert.Equal(vencimiento, ticket.FechaVencimiento);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
        var reapertura = ticket.Comentarios.Last();
        Assert.Equal(2, ticket.Comentarios.Count);
        Assert.Equal(TipoComentario.Reapertura, reapertura.Tipo);
        Assert.Equal(Motivo, reapertura.Texto);
        Assert.Equal(SolicitanteId, reapertura.AutorId);
        Assert.Equal(Ahora, reapertura.FechaCreacion);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public void Reabrir_MotivoEnLimiteMaximo_SeAcepta()
    {
        var ticket = EnEstado(EstadoTicket.Resuelto);
        var motivo = Texto(Comentario.TextoLongitudMaxima);

        ticket.Reabrir(motivo, SolicitanteId, Ahora);

        Assert.Equal(motivo, ticket.Comentarios.Last().Texto);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public void Reabrir_MotivoSobreLimiteMaximo_LanzaInvarianteVioladaSinCambios()
    {
        AfirmarReabrirRechazado(EstadoTicket.Resuelto, Texto(Comentario.TextoLongitudMaxima + 1));
    }

    [Theory]
    [Trait("RN", "RN-09")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\tWi-Fi")]
    [InlineData("Wi-Fi ")]
    public void Reabrir_MotivoEnBlancoOConEspaciosEnLosExtremos_LanzaInvarianteVioladaSinCambios(string? motivo)
    {
        AfirmarReabrirRechazado(EstadoTicket.Resuelto, motivo);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public void Reabrir_MotivoConSaltoDeLineaEnMedio_SeAcepta()
    {
        var ticket = EnEstado(EstadoTicket.Resuelto);
        const string motivo = "El problema volvió.\nOcurre al iniciar sesión.";

        ticket.Reabrir(motivo, SolicitanteId, Ahora);

        Assert.Equal(motivo, ticket.Comentarios.Last().Texto);
    }

    [Fact]
    [Trait("RN", "RN-08")]
    [Trait("RN", "RN-09")]
    public void Reabrir_TextoEnBlancoEnEstadoNoPermitido_LanzaInvarianteViolada()
    {
        // El texto se valida antes que el estado (decisión 85).
        AfirmarReabrirRechazado(EstadoTicket.Cerrado, "");
    }

    private static void AfirmarResolverRechazado(EstadoTicket origen, string? solucion)
    {
        var ticket = EnEstado(origen);
        var comentariosAntes = ticket.Comentarios.Count;
        var actualizacionAntes = ticket.FechaActualizacion;

        var ex = Assert.Throws<InvarianteVioladaException>(() => ticket.Resolver(solucion!, TecnicoId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
        Assert.Equal(origen, ticket.Estado);
        Assert.Equal(comentariosAntes, ticket.Comentarios.Count);
        Assert.Equal(actualizacionAntes, ticket.FechaActualizacion);
    }

    private static void AfirmarReabrirRechazado(EstadoTicket origen, string? motivo)
    {
        var ticket = EnEstado(origen);
        var comentariosAntes = ticket.Comentarios.Count;
        var actualizacionAntes = ticket.FechaActualizacion;

        var ex = Assert.Throws<InvarianteVioladaException>(() => ticket.Reabrir(motivo!, SolicitanteId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
        Assert.Equal(origen, ticket.Estado);
        Assert.Equal(comentariosAntes, ticket.Comentarios.Count);
        Assert.Equal(actualizacionAntes, ticket.FechaActualizacion);
    }
}
