using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using static HelpDesk.Tests.Domain.TicketsDePrueba;

namespace HelpDesk.Tests.Domain;

public class TicketReclasificarCambiarPrioridadTests
{
    [Theory]
    [Trait("RN", "RN-13")]
    [Trait("CA", "CA-01.6")]
    [InlineData(EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Asignado)]
    [InlineData(EstadoTicket.EnProgreso)]
    public void Reclasificar_TicketModificable_CambiaCategoriaSinCambiarEstado(EstadoTicket estado)
    {
        var ticket = EnEstado(estado);

        ticket.Reclasificar(Categoria.Hardware, Ahora);

        Assert.Equal(Categoria.Hardware, ticket.Categoria);
        Assert.Equal(estado, ticket.Estado);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    [Theory]
    [Trait("RN", "RN-13")]
    [Trait("CA", "CA-01.7")]
    [InlineData(EstadoTicket.Resuelto, Categoria.Hardware)]
    [InlineData(EstadoTicket.Resuelto, Categoria.Software)]
    [InlineData(EstadoTicket.Cerrado, Categoria.Hardware)]
    [InlineData(EstadoTicket.Cerrado, Categoria.Software)]
    public void Reclasificar_TicketResueltoOCerrado_LanzaTicketNoModificableSinCambios(EstadoTicket estado, Categoria categoria)
    {
        // Categoria.Software es la categoría actual: la decisión 70 exige el mismo resultado con el mismo valor.
        var ticket = EnEstado(estado);
        var actualizacionAntes = ticket.FechaActualizacion;

        var ex = Assert.Throws<TransicionInvalidaException>(() => ticket.Reclasificar(categoria, Ahora));

        Assert.Equal("TICKET_NO_MODIFICABLE", ex.Codigo);
        Assert.Equal(Categoria.Software, ticket.Categoria);
        Assert.Equal(estado, ticket.Estado);
        Assert.Equal(actualizacionAntes, ticket.FechaActualizacion);
    }

    [Fact]
    [Trait("RN", "RN-13")]
    public void Reclasificar_MismaCategoria_NoCambiaFechaActualizacion()
    {
        var ticket = EnEstado(EstadoTicket.Asignado);
        var actualizacionAntes = ticket.FechaActualizacion;
        var vencimientoAntes = ticket.FechaVencimiento;

        ticket.Reclasificar(Categoria.Software, Ahora);

        Assert.Equal(Categoria.Software, ticket.Categoria);
        Assert.Equal(actualizacionAntes, ticket.FechaActualizacion);
        Assert.Equal(vencimientoAntes, ticket.FechaVencimiento);
    }

    [Theory]
    [Trait("RN", "RN-02")]
    [Trait("RN", "RN-13")]
    [InlineData(EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Cerrado)]
    public void Reclasificar_CategoriaNoDefinida_LanzaInvarianteVioladaAntesQueLaGuardaDeEstado(EstadoTicket estado)
    {
        var ticket = EnEstado(estado);

        var ex = Assert.Throws<InvarianteVioladaException>(() => ticket.Reclasificar((Categoria)9, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
        Assert.Equal(Categoria.Software, ticket.Categoria);
    }

    [Theory]
    [Trait("RN", "RN-10")]
    [Trait("RN", "RN-12")]
    [Trait("CA", "CA-05.4")]
    [InlineData(EstadoTicket.Abierto, Prioridad.Critica, 2)]
    [InlineData(EstadoTicket.Asignado, Prioridad.Alta, 4)]
    [InlineData(EstadoTicket.EnProgreso, Prioridad.Baja, 24)]
    public void CambiarPrioridad_TicketModificable_RecalculaVencimientoDesdeLaCreacion(
        EstadoTicket estado, Prioridad nueva, int horas)
    {
        var ticket = EnEstado(estado);

        ticket.CambiarPrioridad(nueva, Ahora);

        Assert.Equal(nueva, ticket.Prioridad);
        Assert.Equal(Creacion.AddHours(horas), ticket.FechaVencimiento);
        Assert.Equal(estado, ticket.Estado);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
    }

    [Theory]
    [Trait("RN", "RN-12")]
    [Trait("CA", "CA-05.5")]
    [InlineData(EstadoTicket.Resuelto, Prioridad.Critica)]
    [InlineData(EstadoTicket.Resuelto, Prioridad.Media)]
    [InlineData(EstadoTicket.Cerrado, Prioridad.Critica)]
    [InlineData(EstadoTicket.Cerrado, Prioridad.Media)]
    public void CambiarPrioridad_TicketResueltoOCerrado_LanzaTicketNoModificableSinCambios(EstadoTicket estado, Prioridad prioridad)
    {
        // Prioridad.Media es la prioridad actual: la decisión 70 exige el mismo resultado con el mismo valor.
        var ticket = EnEstado(estado);
        var vencimientoAntes = ticket.FechaVencimiento;
        var actualizacionAntes = ticket.FechaActualizacion;

        var ex = Assert.Throws<TransicionInvalidaException>(() => ticket.CambiarPrioridad(prioridad, Ahora));

        Assert.Equal("TICKET_NO_MODIFICABLE", ex.Codigo);
        Assert.Equal(Prioridad.Media, ticket.Prioridad);
        Assert.Equal(vencimientoAntes, ticket.FechaVencimiento);
        Assert.Equal(actualizacionAntes, ticket.FechaActualizacion);
    }

    [Fact]
    [Trait("RN", "RN-12")]
    public void CambiarPrioridad_MismaPrioridad_NoCambiaFechaActualizacionNiVencimiento()
    {
        var ticket = EnEstado(EstadoTicket.EnProgreso);
        var actualizacionAntes = ticket.FechaActualizacion;
        var vencimientoAntes = ticket.FechaVencimiento;

        ticket.CambiarPrioridad(Prioridad.Media, Ahora);

        Assert.Equal(Prioridad.Media, ticket.Prioridad);
        Assert.Equal(actualizacionAntes, ticket.FechaActualizacion);
        Assert.Equal(vencimientoAntes, ticket.FechaVencimiento);
    }

    [Theory]
    [Trait("RN", "RN-02")]
    [Trait("RN", "RN-12")]
    [InlineData(EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Cerrado)]
    public void CambiarPrioridad_PrioridadNoDefinida_LanzaInvarianteVioladaAntesQueLaGuardaDeEstado(EstadoTicket estado)
    {
        var ticket = EnEstado(estado);

        var ex = Assert.Throws<InvarianteVioladaException>(() => ticket.CambiarPrioridad((Prioridad)7, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
        Assert.Equal(Prioridad.Media, ticket.Prioridad);
    }
}
