using HelpDesk.Application.Excepciones;
using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Obtener;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Application.Dobles;
using Microsoft.Extensions.Time.Testing;
using static HelpDesk.Tests.Application.Dobles.ElencoDeTickets;

namespace HelpDesk.Tests.Application;

public class ObtenerTicketHandlerTests
{
    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(Ahora));
    private readonly ConsultasTicketFalsas _consultas;

    public ObtenerTicketHandlerTests()
    {
        var tickets = new TicketRepositorioFalso();
        SembrarTodos(tickets);
        _consultas = new ConsultasTicketFalsas(tickets);
    }

    [Fact]
    [Trait("CA", "CA-02.1")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_Supervisor_DevuelveDetalleCompleto()
    {
        var detalle = await Handler(JorgeId, Rol.Supervisor).ManejarAsync(new(14), CancellationToken.None);

        Assert.Equal(14, detalle.Id);
        Assert.Equal("HD-000014", detalle.Codigo);
        Assert.Equal("Restablecer contraseña de correo", detalle.Titulo);
        Assert.Equal("No recuerdo la contraseña de mi cuenta de correo.", detalle.Descripcion);
        Assert.Equal(Categoria.Accesos, detalle.Categoria);
        Assert.Equal(Prioridad.Media, detalle.Prioridad);
        Assert.Equal(EstadoTicket.Resuelto, detalle.Estado);
        Assert.Equal(new PersonaDto(AnaId, "Ana Solicitante"), detalle.Solicitante);
        Assert.Equal(new PersonaDto(FabianId, "Fabián Técnico"), detalle.Tecnico);
        Assert.Equal(new DateTime(2026, 10, 27, 10, 0, 0, DateTimeKind.Utc), detalle.FechaCreacion);
        Assert.Equal(new DateTime(2026, 10, 27, 16, 0, 0, DateTimeKind.Utc), detalle.FechaActualizacion);
        Assert.Equal(new DateTime(2026, 10, 27, 18, 0, 0, DateTimeKind.Utc), detalle.FechaVencimiento);
        Assert.Equal(new DateTime(2026, 10, 27, 16, 0, 0, DateTimeKind.Utc), detalle.FechaResolucion);
        Assert.Null(detalle.FechaCierre);
        Assert.Equal(EstadoSla.Cumplido, detalle.EstadoSla);

        var comentario = Assert.Single(detalle.Comentarios);
        Assert.Equal(TipoComentario.Solucion, comentario.Tipo);
        Assert.Equal("Se restableció la contraseña y se envió el enlace de acceso.", comentario.Texto);
        Assert.Equal(new PersonaDto(FabianId, "Fabián Técnico"), comentario.Autor);
        Assert.Equal(detalle.FechaResolucion, comentario.FechaCreacion);
    }

    [Fact]
    [Trait("CA", "CA-02.1")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_SolicitanteDueno_DevuelveDetalle()
    {
        var detalle = await Handler(AnaId, Rol.Solicitante).ManejarAsync(new(14), CancellationToken.None);

        Assert.Equal(14, detalle.Id);
        Assert.Equal(AnaId, detalle.Solicitante.Id);
    }

    [Fact]
    [Trait("CA", "CA-02.1")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_TecnicoAsignado_DevuelveDetalle()
    {
        var detalle = await Handler(FabianId, Rol.Tecnico).ManejarAsync(new(12), CancellationToken.None);

        Assert.Equal(12, detalle.Id);
        Assert.Equal(new PersonaDto(FabianId, "Fabián Técnico"), detalle.Tecnico);
        Assert.Empty(detalle.Comentarios);
    }

    [Theory]
    [Trait("CA", "CA-02.2")]
    [InlineData(AnaId, Rol.Solicitante)]
    [InlineData(FabianId, Rol.Tecnico)]
    [InlineData(JorgeId, Rol.Supervisor)]
    public async Task ManejarAsync_TicketInexistente_LanzaTicketNoEncontrado(int usuarioId, Rol rol)
    {
        // El 404 va antes que el 403: el ticket no existe para nadie (decisión 92).
        var ex = await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            Handler(usuarioId, rol).ManejarAsync(new(99), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketNoEncontrado, ex.Codigo);
    }

    [Theory]
    [Trait("CA", "CA-02.6")]
    [Trait("RN", "RN-04")]
    [InlineData(BrunoId, 14)]
    [InlineData(AnaId, 12)]
    public async Task ManejarAsync_SolicitanteNoDueno_LanzaTicketAjeno(int solicitanteId, int ticketId)
    {
        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(solicitanteId, Rol.Solicitante).ManejarAsync(new(ticketId), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketAjeno, ex.Codigo);
    }

    [Theory]
    [Trait("CA", "CA-02.6")]
    [Trait("RN", "RN-04")]
    [InlineData(GabrielaId, 12)]
    [InlineData(FabianId, 13)]
    [InlineData(FabianId, 11)] // Abierto, sin técnico asignado.
    public async Task ManejarAsync_TecnicoNoAsignado_LanzaTicketAjeno(int tecnicoId, int ticketId)
    {
        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(tecnicoId, Rol.Tecnico).ManejarAsync(new(ticketId), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketAjeno, ex.Codigo);
    }

    [Theory]
    [Trait("CA", "CA-05.2")]
    [Trait("RN", "RN-11")]
    [InlineData(16, EstadoSla.EnTiempo)]
    [InlineData(12, EstadoSla.EnTiempo)]
    [InlineData(11, EstadoSla.Vencido)]
    [InlineData(13, EstadoSla.Vencido)]
    public async Task ManejarAsync_TicketNoResuelto_DevuelveEstadoSlaSegunElVencimiento(int ticketId, EstadoSla esperado)
    {
        var detalle = await Handler(JorgeId, Rol.Supervisor).ManejarAsync(new(ticketId), CancellationToken.None);

        Assert.Equal(esperado, detalle.EstadoSla);
    }

    [Theory]
    [Trait("CA", "CA-05.3")]
    [Trait("RN", "RN-11")]
    [InlineData(14, EstadoSla.Cumplido)]
    [InlineData(15, EstadoSla.Incumplido)]
    public async Task ManejarAsync_TicketResuelto_DevuelveEstadoSlaSegunLaResolucion(int ticketId, EstadoSla esperado)
    {
        var detalle = await Handler(JorgeId, Rol.Supervisor).ManejarAsync(new(ticketId), CancellationToken.None);

        Assert.Equal(esperado, detalle.EstadoSla);
    }

    [Fact]
    [Trait("RN", "RN-11")]
    public async Task ManejarAsync_Supervisor_LeeElRelojUnaSolaVez()
    {
        // Cada lectura del reloj lo adelanta: una segunda lectura daría otra hora (RNF-08).
        _reloj.AutoAdvanceAmount = TimeSpan.FromMinutes(1);

        await Handler(JorgeId, Rol.Supervisor).ManejarAsync(new(16), CancellationToken.None);

        Assert.Equal(Ahora, _consultas.AhoraRecibido);
        Assert.Equal(Ahora.AddMinutes(1), _reloj.GetUtcNow().UtcDateTime);
    }

    private ObtenerTicketHandler Handler(int usuarioId, Rol rol) =>
        new(new UsuarioActualFalso(usuarioId, rol), _consultas, _reloj);
}
