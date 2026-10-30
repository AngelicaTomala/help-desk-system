using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using static HelpDesk.Tests.Domain.TicketsDePrueba;

namespace HelpDesk.Tests.Domain;

public class TicketTransicionesTests
{
    /// <summary>Las 6 acciones de la tabla de RN-08.</summary>
    public enum AccionRn08
    {
        Asignar,
        Reasignar,
        IniciarAtencion,
        Resolver,
        Cerrar,
        Reabrir,
    }

    [Theory]
    [Trait("RN", "RN-08")]
    [InlineData(EstadoTicket.Abierto, AccionRn08.Asignar, EstadoTicket.Asignado)]
    [InlineData(EstadoTicket.Asignado, AccionRn08.Reasignar, EstadoTicket.Asignado)]
    [InlineData(EstadoTicket.Asignado, AccionRn08.IniciarAtencion, EstadoTicket.EnProgreso)]
    [InlineData(EstadoTicket.EnProgreso, AccionRn08.Reasignar, EstadoTicket.Asignado)]
    [InlineData(EstadoTicket.EnProgreso, AccionRn08.Resolver, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.Cerrar, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.Reabrir, EstadoTicket.EnProgreso)]
    public void AccionPermitida_SegunRn08_PasaAlEstadoNuevoYFijaFechaActualizacion(
        EstadoTicket origen, AccionRn08 accion, EstadoTicket destino)
    {
        var ticket = EnEstado(origen);

        Ejecutar(ticket, accion, Ahora);

        Assert.Equal(destino, ticket.Estado);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    [Theory]
    [Trait("RN", "RN-08")]
    [Trait("CA", "CA-04.6")]
    [InlineData(EstadoTicket.Abierto, AccionRn08.Reasignar)]
    [InlineData(EstadoTicket.Abierto, AccionRn08.IniciarAtencion)]
    [InlineData(EstadoTicket.Abierto, AccionRn08.Resolver)]
    [InlineData(EstadoTicket.Abierto, AccionRn08.Cerrar)]
    [InlineData(EstadoTicket.Abierto, AccionRn08.Reabrir)]
    [InlineData(EstadoTicket.Asignado, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.Asignado, AccionRn08.Resolver)]
    [InlineData(EstadoTicket.Asignado, AccionRn08.Cerrar)]
    [InlineData(EstadoTicket.Asignado, AccionRn08.Reabrir)]
    [InlineData(EstadoTicket.EnProgreso, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.EnProgreso, AccionRn08.IniciarAtencion)]
    [InlineData(EstadoTicket.EnProgreso, AccionRn08.Cerrar)]
    [InlineData(EstadoTicket.EnProgreso, AccionRn08.Reabrir)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.Reasignar)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.IniciarAtencion)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.Resolver)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.Reasignar)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.IniciarAtencion)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.Resolver)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.Cerrar)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.Reabrir)]
    public void AccionNoPermitida_SegunRn08_LanzaTransicionInvalidaSinCambios(EstadoTicket origen, AccionRn08 accion)
    {
        AfirmarTransicionInvalidaSinCambios(origen, accion);
    }

    [Theory]
    [Trait("RN", "RN-07")]
    [Trait("RN", "RN-08")]
    [Trait("CA", "CA-03.4")]
    [InlineData(EstadoTicket.Asignado, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.EnProgreso, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.Asignar)]
    [InlineData(EstadoTicket.Abierto, AccionRn08.Reasignar)]
    [InlineData(EstadoTicket.Resuelto, AccionRn08.Reasignar)]
    [InlineData(EstadoTicket.Cerrado, AccionRn08.Reasignar)]
    public void AsignarOReasignar_EstadoCruzado_LanzaTransicionInvalidaSinCambios(EstadoTicket origen, AccionRn08 accion)
    {
        AfirmarTransicionInvalidaSinCambios(origen, accion);
    }

    [Fact]
    [Trait("RN", "RN-07")]
    [Trait("RN", "RN-08")]
    [Trait("CA", "CA-03.1")]
    public void Asignar_TicketAbierto_QuedaAsignadoConElTecnico()
    {
        var ticket = EnEstado(EstadoTicket.Abierto);

        ticket.Asignar(TecnicoId, Ahora);

        Assert.Equal(EstadoTicket.Asignado, ticket.Estado);
        Assert.Equal(TecnicoId, ticket.TecnicoId);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    [Theory]
    [Trait("RN", "RN-07")]
    [Trait("RN", "RN-08")]
    [Trait("CA", "CA-03.2")]
    [InlineData(EstadoTicket.Asignado)]
    [InlineData(EstadoTicket.EnProgreso)]
    public void Reasignar_TicketAsignadoOEnProgreso_CambiaTecnicoYQuedaAsignado(EstadoTicket origen)
    {
        var ticket = EnEstado(origen);

        ticket.Reasignar(OtroTecnicoId, Ahora);

        Assert.Equal(EstadoTicket.Asignado, ticket.Estado);
        Assert.Equal(OtroTecnicoId, ticket.TecnicoId);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    [Fact]
    [Trait("RN", "RN-07")]
    public void Reasignar_MismoTecnico_NoLanzaExcepcion()
    {
        // MISMO_TECNICO (RN-06) se valida en ReasignarTicketHandler (decisión 81); Domain no lo rechaza.
        var ticket = EnEstado(EstadoTicket.Asignado);

        ticket.Reasignar(TecnicoId, Ahora);

        Assert.Equal(EstadoTicket.Asignado, ticket.Estado);
        Assert.Equal(TecnicoId, ticket.TecnicoId);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    [Fact]
    [Trait("RN", "RN-08")]
    [Trait("CA", "CA-04.1")]
    public void IniciarAtencion_TicketAsignado_QuedaEnProgreso()
    {
        var ticket = EnEstado(EstadoTicket.Asignado);

        ticket.IniciarAtencion(Ahora);

        Assert.Equal(EstadoTicket.EnProgreso, ticket.Estado);
        Assert.Equal(TecnicoId, ticket.TecnicoId);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    [Fact]
    [Trait("RN", "RN-08")]
    [Trait("CA", "CA-04.4")]
    public void Cerrar_TicketResuelto_QuedaCerradoConFechaCierre()
    {
        var ticket = EnEstado(EstadoTicket.Resuelto);
        var fechaResolucion = ticket.FechaResolucion;

        ticket.Cerrar(Ahora);

        Assert.Equal(EstadoTicket.Cerrado, ticket.Estado);
        Assert.Equal(Ahora, ticket.FechaCierre);
        Assert.Equal(fechaResolucion, ticket.FechaResolucion);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    private static void AfirmarTransicionInvalidaSinCambios(EstadoTicket origen, AccionRn08 accion)
    {
        var ticket = EnEstado(origen);
        var tecnicoAntes = ticket.TecnicoId;
        var actualizacionAntes = ticket.FechaActualizacion;

        var ex = Assert.Throws<TransicionInvalidaException>(() => Ejecutar(ticket, accion, Ahora));

        Assert.Equal("TRANSICION_INVALIDA", ex.Codigo);
        Assert.Equal(origen, ticket.Estado);
        Assert.Equal(tecnicoAntes, ticket.TecnicoId);
        Assert.Equal(actualizacionAntes, ticket.FechaActualizacion);
    }

    private static void Ejecutar(Ticket ticket, AccionRn08 accion, DateTime ahora)
    {
        switch (accion)
        {
            case AccionRn08.Asignar:
                ticket.Asignar(OtroTecnicoId, ahora);
                break;
            case AccionRn08.Reasignar:
                ticket.Reasignar(OtroTecnicoId, ahora);
                break;
            case AccionRn08.IniciarAtencion:
                ticket.IniciarAtencion(ahora);
                break;
            case AccionRn08.Resolver:
                ticket.Resolver(Solucion, TecnicoId, ahora);
                break;
            case AccionRn08.Cerrar:
                ticket.Cerrar(ahora);
                break;
            case AccionRn08.Reabrir:
                ticket.Reabrir(Motivo, SolicitanteId, ahora);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(accion), accion, null);
        }
    }
}
