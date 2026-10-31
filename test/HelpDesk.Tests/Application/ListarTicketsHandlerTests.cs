using HelpDesk.Application.Excepciones;
using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Listar;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Application.Dobles;
using Microsoft.Extensions.Time.Testing;
using static HelpDesk.Tests.Application.Dobles.ElencoDeTickets;

namespace HelpDesk.Tests.Application;

public class ListarTicketsHandlerTests
{
    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(Ahora));
    private readonly ConsultasTicketFalsas _consultas;

    public ListarTicketsHandlerTests()
    {
        var tickets = new TicketRepositorioFalso();
        SembrarTodos(tickets);
        _consultas = new ConsultasTicketFalsas(tickets);
    }

    [Fact]
    [Trait("CA", "CA-02.5")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_Solicitante_SoloVeSusTickets()
    {
        var pagina = await Listar(AnaId, Rol.Solicitante, new());

        Assert.Equal([16, 11, 14], Ids(pagina));
        Assert.Equal(AnaId, _consultas.FiltroRecibido!.SolicitanteId);
    }

    [Fact]
    [Trait("CA", "CA-02.5")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_Tecnico_SoloVeLosAsignadosAEl()
    {
        var pagina = await Listar(FabianId, Rol.Tecnico, new());

        Assert.Equal([12, 14, 15], Ids(pagina));
        Assert.Equal(FabianId, _consultas.FiltroRecibido!.TecnicoId);
        Assert.Null(_consultas.FiltroRecibido.SolicitanteId);
    }

    [Fact]
    [Trait("CA", "CA-02.5")]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-04")]
    [Trait("RN", "RN-05")]
    public async Task ManejarAsync_Supervisor_VeTodosOrdenadosPorFechaYLuegoPorId()
    {
        // 11 y 13 se crearon a la misma hora: va primero el de mayor Id (decisión 59).
        var pagina = await Listar(JorgeId, Rol.Supervisor, new());

        Assert.Equal([16, 12, 13, 11, 14, 15], Ids(pagina));
        Assert.Null(_consultas.FiltroRecibido!.SolicitanteId);
        Assert.Null(_consultas.FiltroRecibido.TecnicoId);
    }

    [Fact]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    public async Task ManejarAsync_SinPaginacion_UsaPaginaUnoYTamanoVeinte()
    {
        var pagina = await Listar(JorgeId, Rol.Supervisor, new());

        Assert.Equal(1, pagina.Page);
        Assert.Equal(20, pagina.PageSize);
        Assert.Equal(6, pagina.TotalItems);
        Assert.Equal(Paginacion.PaginaPorDefecto, _consultas.FiltroRecibido!.Page);
        Assert.Equal(Paginacion.TamanoPorDefecto, _consultas.FiltroRecibido.PageSize);
    }

    [Fact]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    public async Task ManejarAsync_SegundaPagina_DevuelvePaginaYTotal()
    {
        var pagina = await Listar(JorgeId, Rol.Supervisor, new(Page: ["2"], PageSize: ["2"]));

        Assert.Equal([13, 11], Ids(pagina));
        Assert.Equal(2, pagina.Page);
        Assert.Equal(2, pagina.PageSize);
        Assert.Equal(6, pagina.TotalItems);
    }

    [Fact]
    [Trait("CA", "CA-02.4")]
    public async Task ManejarAsync_Solicitante_DevuelveResumenDeCadaTicket()
    {
        var pagina = await Listar(AnaId, Rol.Solicitante, new());

        Assert.Equal(
            new TicketResumenDto(11, "HD-000011", "No abre el sistema de nómina", EstadoTicket.Abierto, Prioridad.Critica,
                null, new DateTime(2026, 10, 28, 10, 0, 0, DateTimeKind.Utc), EstadoSla.Vencido),
            pagina.Items[1]);
        Assert.Equal(
            new TicketResumenDto(14, "HD-000014", "Restablecer contraseña de correo", EstadoTicket.Resuelto,
                Prioridad.Media, new PersonaDto(FabianId, "Fabián Técnico"),
                new DateTime(2026, 10, 27, 18, 0, 0, DateTimeKind.Utc), EstadoSla.Cumplido),
            pagina.Items[2]);
    }

    [Theory]
    [Trait("CA", "CA-02.3")]
    [InlineData("Asignado", "media", null, new[] { 12 })]
    [InlineData("resuelto", null, "6", new[] { 14 })]
    [InlineData(null, "Alta", null, new[] { 16, 13 })]
    [InlineData("Abierto", "Baja", null, new int[0])]
    public async Task ManejarAsync_FiltrosCombinados_DevuelveLosQueCumplenTodos(
        string? estado,
        string? prioridad,
        string? tecnicoId,
        int[] esperados)
    {
        var consulta = new ListarTicketsConsulta(Lista(estado), Lista(prioridad), Lista(tecnicoId));

        var pagina = await Listar(JorgeId, Rol.Supervisor, consulta);

        Assert.Equal(esperados, Ids(pagina));
        Assert.Equal(esperados.Length, pagina.TotalItems);
    }

    [Theory]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-11")]
    [InlineData("true")]
    [InlineData("TRUE")]
    public async Task ManejarAsync_SoloVencidos_DevuelveVencidosSinIncumplidos(string soloVencidos)
    {
        // El 15 está Incumplido (resuelto tarde): no es Vencido.
        var pagina = await Listar(JorgeId, Rol.Supervisor, new(SoloVencidos: [soloVencidos]));

        Assert.Equal([13, 11], Ids(pagina));
        Assert.All(pagina.Items, item => Assert.Equal(EstadoSla.Vencido, item.EstadoSla));
        Assert.True(_consultas.FiltroRecibido!.SoloVencidos);
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-11")]
    public async Task ManejarAsync_SoloVencidosFalse_EquivaleANoEnviarlo()
    {
        var pagina = await Listar(JorgeId, Rol.Supervisor, new(SoloVencidos: ["false"]));

        Assert.Equal(6, pagina.TotalItems);
        Assert.False(_consultas.FiltroRecibido!.SoloVencidos);
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-04")]
    [Trait("RN", "RN-11")]
    public async Task ManejarAsync_SolicitanteConSoloVencidos_FiltraDentroDeSusTickets()
    {
        var pagina = await Listar(AnaId, Rol.Solicitante, new(SoloVencidos: ["true"]));

        Assert.Equal([11], Ids(pagina));
    }

    [Theory]
    [Trait("CA", "CA-05.2")]
    [Trait("CA", "CA-05.3")]
    [Trait("RN", "RN-11")]
    [InlineData(16, EstadoSla.EnTiempo)]
    [InlineData(11, EstadoSla.Vencido)]
    [InlineData(14, EstadoSla.Cumplido)]
    [InlineData(15, EstadoSla.Incumplido)]
    public async Task ManejarAsync_Supervisor_CalculaEstadoSlaDeCadaTicket(int ticketId, EstadoSla esperado)
    {
        var pagina = await Listar(JorgeId, Rol.Supervisor, new());

        Assert.Equal(esperado, pagina.Items.Single(item => item.Id == ticketId).EstadoSla);
    }

    [Fact]
    [Trait("RN", "RN-11")]
    public async Task ManejarAsync_Supervisor_LeeElRelojUnaSolaVez()
    {
        // Cada lectura del reloj lo adelanta: una segunda lectura daría otra hora (RNF-08).
        _reloj.AutoAdvanceAmount = TimeSpan.FromMinutes(1);

        await Listar(JorgeId, Rol.Supervisor, new(SoloVencidos: ["true"]));

        Assert.Equal(Ahora, _consultas.AhoraRecibido);
        Assert.Equal(Ahora.AddMinutes(1), _reloj.GetUtcNow().UtcDateTime);
    }

    [Theory]
    [Trait("CA", "CA-02.7")]
    [Trait("RN", "RN-04")]
    [InlineData("7")]
    [InlineData("10")]
    [InlineData("06")]
    [InlineData("+6")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("6", "7")]
    [InlineData("7", "6")]
    [InlineData("6", "")]
    public async Task ManejarAsync_TecnicoConTecnicoIdDistintoDelSuyo_LanzaFiltroNoAutorizadoSinConsultar(
        params string[] tecnicoId)
    {
        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Listar(FabianId, Rol.Tecnico, new(TecnicoId: tecnicoId)));

        Assert.Equal(CodigosErrorAplicacion.FiltroNoAutorizado, ex.Codigo);
        Assert.Equal(0, _consultas.VecesListado);
    }

    [Fact]
    [Trait("CA", "CA-02.7")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_TecnicoConOtroTecnicoIdYPageInvalida_LanzaFiltroNoAutorizadoAntesQueValidacion()
    {
        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Listar(FabianId, Rol.Tecnico, new(TecnicoId: ["7"], Page: ["0"])));

        Assert.Equal(CodigosErrorAplicacion.FiltroNoAutorizado, ex.Codigo);
    }

    [Fact]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_TecnicoConSuPropioTecnicoId_DevuelveSusTickets()
    {
        var pagina = await Listar(FabianId, Rol.Tecnico, new(TecnicoId: ["6"]));

        Assert.Equal([12, 14, 15], Ids(pagina));
    }

    [Fact]
    [Trait("CA", "CA-02.8")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_TecnicoConSuIdRepetido_LanzaValidacionFallidaSinConsultar()
    {
        var ex = await Assert.ThrowsAsync<SolicitudInvalidaException>(() =>
            Listar(FabianId, Rol.Tecnico, new(TecnicoId: ["6", "6"])));

        Assert.Equal(CodigosErrorAplicacion.ValidacionFallida, ex.Codigo);
        Assert.Equal(["tecnicoId"], ex.Errores.Keys);
        Assert.Equal(0, _consultas.VecesListado);
    }

    [Theory]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-04")]
    [InlineData("6", new[] { 14 })]
    [InlineData("7", new int[0])]
    [InlineData("10", new int[0])] // No es técnico: lista vacía (decisión 69).
    public async Task ManejarAsync_SolicitanteConTecnicoId_FiltraDentroDeSusTickets(string tecnicoId, int[] esperados)
    {
        var pagina = await Listar(AnaId, Rol.Solicitante, new(TecnicoId: [tecnicoId]));

        Assert.Equal(esperados, Ids(pagina));
        Assert.Equal(AnaId, _consultas.FiltroRecibido!.SolicitanteId);
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-04")]
    public async Task ManejarAsync_SupervisorConTecnicoIdQueNoEsTecnico_DevuelveListaVacia()
    {
        // Decisión 69: un Id que no es de un técnico existente no es un error.
        var pagina = await Listar(JorgeId, Rol.Supervisor, new(TecnicoId: ["1"]));

        Assert.Empty(pagina.Items);
        Assert.Equal(0, pagina.TotalItems);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [Trait("RN", "RN-05")]
    [InlineData(AnaId, Rol.Solicitante)]
    [InlineData(JorgeId, Rol.Supervisor)]
    public async Task ManejarAsync_TecnicoIdMalFormadoSinSerTecnico_LanzaValidacionFallida(int usuarioId, Rol rol)
    {
        // FILTRO_NO_AUTORIZADO aplica solo al técnico (decisión 100).
        var ex = await Assert.ThrowsAsync<SolicitudInvalidaException>(() =>
            Listar(usuarioId, rol, new(TecnicoId: ["abc"])));

        Assert.Equal(["tecnicoId"], ex.Errores.Keys);
        Assert.Equal(0, _consultas.VecesListado);
    }

    [Fact]
    [Trait("CA", "CA-02.4")]
    [Trait("CA", "CA-02.8")]
    [Trait("RN", "RN-05")]
    public async Task ManejarAsync_ParametrosInvalidos_LanzaValidacionFallidaConTodasLasClavesSinConsultar()
    {
        var consulta = new ListarTicketsConsulta(
            Estado: ["Pendiente"], Prioridad: ["3"], SoloVencidos: ["si"], Page: ["0"], PageSize: ["101"]);

        var ex = await Assert.ThrowsAsync<SolicitudInvalidaException>(() => Listar(JorgeId, Rol.Supervisor, consulta));

        Assert.Equal(CodigosErrorAplicacion.ValidacionFallida, ex.Codigo);
        Assert.Equal(["estado", "page", "pageSize", "prioridad", "soloVencidos"], ex.Errores.Keys.Order());
        Assert.Equal(0, _consultas.VecesListado);
    }

    private static IReadOnlyList<string>? Lista(string? valor) => valor is null ? null : [valor];

    private static int[] Ids(PaginaDto<TicketResumenDto> pagina) => [.. pagina.Items.Select(item => item.Id)];

    private Task<PaginaDto<TicketResumenDto>> Listar(int usuarioId, Rol rol, ListarTicketsConsulta consulta) =>
        new ListarTicketsHandler(new UsuarioActualFalso(usuarioId, rol), new ListarTicketsValidator(), _consultas, _reloj)
            .ManejarAsync(consulta, CancellationToken.None);
}
