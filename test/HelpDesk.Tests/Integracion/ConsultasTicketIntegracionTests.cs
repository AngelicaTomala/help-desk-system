using HelpDesk.Application.Tickets;
using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Infrastructure.Persistencia;
using HelpDesk.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace HelpDesk.Tests.Integracion;

[Collection(BaseDePruebasColeccion.Nombre)]
public class ConsultasTicketIntegracionTests(BaseDePruebasFixture baseDePruebas, ITestOutputHelper salida)
{
    // SQL constante, sin concatenación ni interpolación (RNF-02). Se revierte con la transacción de la prueba.
    private const string BorrarTickets = "DELETE FROM dbo.Comentarios; DELETE FROM dbo.Tickets";

    private static readonly PersonaDto Ana = new(1, "Ana Solicitante");
    private static readonly PersonaDto Fabian = new(6, "Fabián Técnico");
    private static readonly PersonaDto Gabriela = new(7, "Gabriela Técnico");
    private static readonly PersonaDto Hugo = new(8, "Hugo Técnico");
    private const int Bruno = 2;
    private const int Jorge = 10;

    [Fact]
    [Trait("CA", "CA-02.1")]
    public async Task ObtenerDetalle_TicketResuelto_DevuelveDetalleConNombresDePersonas()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var ticket = TicketsDePrueba.EnEstado(EstadoTicket.Resuelto);
        await GuardarAsync(transaccion, ticket);

        var detalle = await ObtenerDetalleAsync(transaccion, ticket.Id, TicketsDePrueba.Ahora);

        Assert.NotNull(detalle);
        Assert.Equal(ticket.Id, detalle.Id);
        Assert.Equal(TicketsDePrueba.Codigo, detalle.Codigo);
        Assert.Equal(TicketsDePrueba.Titulo, detalle.Titulo);
        Assert.Equal(TicketsDePrueba.Descripcion, detalle.Descripcion);
        Assert.Equal(Categoria.Software, detalle.Categoria);
        Assert.Equal(Prioridad.Media, detalle.Prioridad);
        Assert.Equal(EstadoTicket.Resuelto, detalle.Estado);
        Assert.Equal(Ana, detalle.Solicitante);
        Assert.Equal(Fabian, detalle.Tecnico);
        Assert.Equal(ticket.FechaVencimiento, detalle.FechaVencimiento);
        Assert.Equal(ticket.FechaResolucion, detalle.FechaResolucion);
        Assert.Null(detalle.FechaCierre);
        // Resuelto a las 08:30 con vencimiento a las 16:00 (RN-10: Media = 8 h).
        Assert.Equal(EstadoSla.Cumplido, detalle.EstadoSla);

        var comentario = Assert.Single(detalle.Comentarios);
        Assert.Equal(TipoComentario.Solucion, comentario.Tipo);
        Assert.Equal(TicketsDePrueba.Solucion, comentario.Texto);
        Assert.Equal(Fabian, comentario.Autor);
    }

    [Fact]
    [Trait("CA", "CA-02.1")]
    public async Task ObtenerDetalle_TicketSinTecnico_TecnicoNullYSinComentarios()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var ticket = TicketsDePrueba.Nuevo();
        await GuardarAsync(transaccion, ticket);

        var detalle = await ObtenerDetalleAsync(transaccion, ticket.Id, TicketsDePrueba.Ahora);

        Assert.NotNull(detalle);
        Assert.Equal(EstadoTicket.Abierto, detalle.Estado);
        Assert.Equal(Ana, detalle.Solicitante);
        Assert.Null(detalle.Tecnico);
        Assert.Null(detalle.FechaResolucion);
        Assert.Empty(detalle.Comentarios);
    }

    [Fact]
    [Trait("CA", "CA-02.1")]
    public async Task ObtenerDetalle_ComentariosDesordenados_OrdenaPorFechaYLuegoPorId()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var (ticketId, ids) = await GuardarConTresComentariosAsync(transaccion);

        var detalle = await ObtenerDetalleAsync(transaccion, ticketId, TicketsDePrueba.Ahora);

        // 09:00 (id menor), 09:00 (id mayor) y 10:00 (decisión 68).
        Assert.NotNull(detalle);
        Assert.Equal([ids[1], ids[2], ids[0]], detalle.Comentarios.Select(c => c.Id));
        Assert.Equal([Ana, Gabriela, Fabian], detalle.Comentarios.Select(c => c.Autor));
        Assert.Equal(
            [TipoComentario.Reapertura, TipoComentario.Solucion, TipoComentario.Solucion],
            detalle.Comentarios.Select(c => c.Tipo));
    }

    [Fact]
    [Trait("CA", "CA-05.2")]
    [Trait("RN", "RN-11")]
    public async Task ObtenerDetalle_MismoTicketConDistintoAhora_EnTiempoYLuegoVencido()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        // Media creado a las 08:00: vence a las 16:00 (RN-10).
        var ticket = TicketsDePrueba.Nuevo(Prioridad.Media);
        await GuardarAsync(transaccion, ticket);
        var vencimiento = TicketsDePrueba.Creacion.AddHours(8);

        var enVencimiento = await ObtenerDetalleAsync(transaccion, ticket.Id, vencimiento);
        var despues = await ObtenerDetalleAsync(transaccion, ticket.Id, vencimiento.AddSeconds(1));

        Assert.Equal(vencimiento, enVencimiento!.FechaVencimiento);
        Assert.Equal(EstadoSla.EnTiempo, enVencimiento.EstadoSla);
        Assert.Equal(EstadoSla.Vencido, despues!.EstadoSla);
    }

    [Fact]
    public async Task ObtenerDetalle_TicketCerrado_FechasUtc()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var ticket = TicketsDePrueba.EnEstado(EstadoTicket.Cerrado);
        await GuardarAsync(transaccion, ticket);

        var detalle = await ObtenerDetalleAsync(transaccion, ticket.Id, TicketsDePrueba.Ahora);

        Assert.NotNull(detalle);
        AssertFechaUtc(ticket.FechaCreacion, detalle.FechaCreacion);
        AssertFechaUtc(ticket.FechaActualizacion, detalle.FechaActualizacion);
        AssertFechaUtc(ticket.FechaVencimiento, detalle.FechaVencimiento);
        AssertFechaUtc(ticket.FechaResolucion, detalle.FechaResolucion);
        AssertFechaUtc(ticket.FechaCierre, detalle.FechaCierre);
        AssertFechaUtc(ticket.FechaResolucion, Assert.Single(detalle.Comentarios).FechaCreacion);
    }

    [Fact]
    public async Task ObtenerDetalle_TicketInexistente_DevuelveNull()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();

        var detalle = await ObtenerDetalleAsync(transaccion, int.MaxValue, TicketsDePrueba.Ahora);

        Assert.Null(detalle);
    }

    [Fact]
    public async Task ObtenerDetalle_TresComentariosDeTresAutores_EjecutaDosConsultas()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var (ticketId, _) = await GuardarConTresComentariosAsync(transaccion);
        var contador = new ContadorDeComandos();

        await using var context = transaccion.NuevoContexto(contador);
        var detalle = await new ConsultasTicket(context).ObtenerDetalleAsync(ticketId, TicketsDePrueba.Ahora, CancellationToken.None);

        // Una para el ticket con sus comentarios y otra para los usuarios, no una por comentario ni por usuario.
        Assert.Equal(3, detalle!.Comentarios.Count);
        Assert.Equal(2, contador.Consultas);
    }

    [Fact]
    [Trait("RN", "RN-04")]
    public async Task Listar_SinFiltros_DevuelveTodos()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, TicketsDePrueba.Creacion),
            CrearTicket(2, TicketsDePrueba.Creacion, EstadoTicket.Asignado, solicitanteId: Bruno, tecnicoId: Gabriela.Id),
            CrearTicket(3, TicketsDePrueba.Creacion, EstadoTicket.Cerrado));

        // Supervisor: Application no fija solicitante ni técnico (RN-04).
        var pagina = await ListarAsync(transaccion, Filtro());

        Assert.Equal(3, pagina.TotalItems);
        Assert.Equal(Ids(tickets).Order(), pagina.Items.Select(i => i.Id).Order());
    }

    [Fact]
    [Trait("RN", "RN-04")]
    public async Task Listar_FiltroSolicitante_DevuelveSoloLosSuyos()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, TicketsDePrueba.Creacion),
            CrearTicket(2, TicketsDePrueba.Creacion, solicitanteId: Bruno),
            CrearTicket(3, TicketsDePrueba.Creacion, EstadoTicket.Resuelto));

        var pagina = await ListarAsync(transaccion, Filtro() with { SolicitanteId = Ana.Id });

        Assert.Equal(2, pagina.TotalItems);
        Assert.Equal(Ids(tickets[0], tickets[2]).Order(), pagina.Items.Select(i => i.Id).Order());
    }

    [Fact]
    [Trait("RN", "RN-04")]
    public async Task Listar_FiltroTecnico_DevuelveSoloLosAsignados()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, TicketsDePrueba.Creacion, EstadoTicket.Asignado),
            CrearTicket(2, TicketsDePrueba.Creacion, EstadoTicket.Asignado, tecnicoId: Gabriela.Id),
            CrearTicket(3, TicketsDePrueba.Creacion),
            CrearTicket(4, TicketsDePrueba.Creacion, EstadoTicket.EnProgreso));

        var pagina = await ListarAsync(transaccion, Filtro() with { TecnicoId = Fabian.Id });

        Assert.Equal(2, pagina.TotalItems);
        Assert.Equal(Ids(tickets[0], tickets[3]).Order(), pagina.Items.Select(i => i.Id).Order());
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    public async Task Listar_FiltroEstado_DevuelveSoloEseEstado()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, TicketsDePrueba.Creacion),
            CrearTicket(2, TicketsDePrueba.Creacion, EstadoTicket.Asignado),
            CrearTicket(3, TicketsDePrueba.Creacion, EstadoTicket.EnProgreso));

        var pagina = await ListarAsync(transaccion, Filtro() with { Estado = EstadoTicket.Asignado });

        Assert.Equal(1, pagina.TotalItems);
        Assert.Equal(tickets[1].Id, Assert.Single(pagina.Items).Id);
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    public async Task Listar_FiltroPrioridad_DevuelveSoloEsaPrioridad()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, TicketsDePrueba.Creacion, prioridad: Prioridad.Alta),
            CrearTicket(2, TicketsDePrueba.Creacion, prioridad: Prioridad.Media),
            CrearTicket(3, TicketsDePrueba.Creacion, prioridad: Prioridad.Baja));

        var pagina = await ListarAsync(transaccion, Filtro() with { Prioridad = Prioridad.Alta });

        Assert.Equal(1, pagina.TotalItems);
        Assert.Equal(tickets[0].Id, Assert.Single(pagina.Items).Id);
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    public async Task Listar_FiltrosCombinados_DevuelveLosQueCumplenTodos()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var creacion = TicketsDePrueba.Creacion;
        // El primero cumple todos los filtros; cada uno de los demás falla en uno solo.
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, creacion, EstadoTicket.EnProgreso, Prioridad.Alta),
            CrearTicket(2, creacion, EstadoTicket.EnProgreso, Prioridad.Alta, solicitanteId: Bruno),
            CrearTicket(3, creacion, EstadoTicket.EnProgreso, Prioridad.Alta, tecnicoId: Gabriela.Id),
            CrearTicket(4, creacion, EstadoTicket.Asignado, Prioridad.Alta),
            CrearTicket(5, creacion, EstadoTicket.EnProgreso, Prioridad.Critica),
            // Alta creada a las 13:00: vence a las 17:00, en tiempo a las 14:30.
            CrearTicket(6, creacion.AddHours(5), EstadoTicket.EnProgreso, Prioridad.Alta));

        var pagina = await ListarAsync(transaccion, new FiltroListadoTickets(
            Ana.Id, Fabian.Id, EstadoTicket.EnProgreso, Prioridad.Alta, SoloVencidos: true, Page: 1, PageSize: 20));

        Assert.Equal(1, pagina.TotalItems);
        Assert.Equal(tickets[0].Id, Assert.Single(pagina.Items).Id);
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    public async Task Listar_TecnicoIdQueNoEsTecnico_ListaVacia()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        await GuardarListadoAsync(transaccion, CrearTicket(1, TicketsDePrueba.Creacion, EstadoTicket.Asignado));

        // Jorge es supervisor: ningún ticket lo tiene como técnico (decisión 69).
        var pagina = await ListarAsync(transaccion, Filtro() with { TecnicoId = Jorge });

        Assert.Empty(pagina.Items);
        Assert.Equal(0, pagina.TotalItems);
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-11")]
    public async Task Listar_SoloVencidos_ExcluyeEnTiempoCumplidoEIncumplido()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var creacion = TicketsDePrueba.Creacion;
        var tickets = await GuardarListadoAsync(
            transaccion,
            // Alta: vence a las 12:00 → Vencido a las 14:30.
            CrearTicket(1, creacion, EstadoTicket.EnProgreso, Prioridad.Alta),
            // Crítica: vence a las 10:00 → Vencido.
            CrearTicket(2, creacion, prioridad: Prioridad.Critica),
            // Media: vence a las 16:00 → EnTiempo.
            CrearTicket(3, creacion),
            // Alta resuelta a las 13:00, después del vencimiento de las 12:00 → Incumplido.
            CrearTicket(4, creacion, EstadoTicket.Resuelto, Prioridad.Alta, resolucion: creacion.AddHours(5)),
            // Media resuelta a las 08:30 y cerrada → Cumplido.
            CrearTicket(5, creacion, EstadoTicket.Cerrado));

        var pagina = await ListarAsync(transaccion, Filtro() with { SoloVencidos = true });

        Assert.Equal(2, pagina.TotalItems);
        Assert.Equal(Ids(tickets[0], tickets[1]).Order(), pagina.Items.Select(i => i.Id).Order());
        Assert.All(pagina.Items, i => Assert.Equal(EstadoSla.Vencido, i.EstadoSla));
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("CA", "CA-05.2")]
    [Trait("RN", "RN-11")]
    public async Task Listar_SoloVencidosConAhoraIgualAlVencimiento_NoLoIncluyeYUnSegundoDespuesSi()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        // Media creado a las 08:00: vence a las 16:00 (RN-10).
        var tickets = await GuardarListadoAsync(transaccion, CrearTicket(1, TicketsDePrueba.Creacion));
        var vencimiento = TicketsDePrueba.Creacion.AddHours(8);

        var enVencimiento = await ListarAsync(transaccion, Filtro() with { SoloVencidos = true }, vencimiento);
        var despues = await ListarAsync(transaccion, Filtro() with { SoloVencidos = true }, vencimiento.AddSeconds(1));

        Assert.Empty(enVencimiento.Items);
        Assert.Equal(0, enVencimiento.TotalItems);
        var vencido = Assert.Single(despues.Items);
        Assert.Equal(tickets[0].Id, vencido.Id);
        Assert.Equal(EstadoSla.Vencido, vencido.EstadoSla);
    }

    [Fact]
    [Trait("RN", "RN-05")]
    public async Task Listar_DosTicketsConLaMismaFechaCreacion_OrdenaPorFechaYLuegoPorIdDescendentes()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var creacion = TicketsDePrueba.Creacion;
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, creacion),
            CrearTicket(2, creacion.AddHours(1)),
            CrearTicket(3, creacion.AddHours(1)),
            CrearTicket(4, creacion.AddHours(-1)));
        var empatados = Ids(tickets[1], tickets[2]).OrderDescending();

        var pagina = await ListarAsync(transaccion, Filtro());

        // 09:00 (Id mayor), 09:00 (Id menor), 08:00 y 07:00 (decisión 59).
        Assert.Equal([.. empatados, tickets[0].Id, tickets[3].Id], pagina.Items.Select(i => i.Id));
    }

    [Fact]
    [Trait("RN", "RN-05")]
    public async Task Listar_SegundaPagina_DevuelveElRestoYElTotalFiltrado()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var creacion = TicketsDePrueba.Creacion;
        // Cinco de Ana, de las 08:00 a las 12:00, y uno de Bruno que el filtro deja fuera del total.
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, creacion),
            CrearTicket(2, creacion.AddHours(1)),
            CrearTicket(3, creacion.AddHours(2)),
            CrearTicket(4, creacion.AddHours(3)),
            CrearTicket(5, creacion.AddHours(4)),
            CrearTicket(6, creacion.AddHours(5), solicitanteId: Bruno));

        var pagina = await ListarAsync(transaccion, Filtro(page: 2, pageSize: 2) with { SolicitanteId = Ana.Id });

        Assert.Equal(2, pagina.Page);
        Assert.Equal(2, pagina.PageSize);
        Assert.Equal(5, pagina.TotalItems);
        // Página 1: 12:00 y 11:00; página 2: 10:00 y 09:00.
        Assert.Equal([tickets[2].Id, tickets[1].Id], pagina.Items.Select(i => i.Id));
    }

    [Fact]
    [Trait("RN", "RN-05")]
    public async Task Listar_PaginaMasAllaDeLaUltima_ItemsVacioYTotalReal()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, TicketsDePrueba.Creacion),
            CrearTicket(2, TicketsDePrueba.Creacion),
            CrearTicket(3, TicketsDePrueba.Creacion));
        var contador = new ContadorDeComandos();

        var pagina = await ListarAsync(transaccion, Filtro(page: 3, pageSize: 2), contador: contador);

        Assert.Empty(pagina.Items);
        Assert.Equal(3, pagina.Page);
        Assert.Equal(2, pagina.PageSize);
        Assert.Equal(3, pagina.TotalItems);
        // Solo el conteo: la página no se lee.
        Assert.Equal(1, contador.Consultas);
    }

    [Fact]
    [Trait("RN", "RN-05")]
    public async Task Listar_PageSize100Con101Tickets_DevuelveCienYTotal101()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var tickets = await GuardarListadoAsync(
            transaccion,
            Enumerable.Range(1, 101).Select(n => CrearTicket(n, TicketsDePrueba.Creacion.AddMinutes(n))).ToArray());

        var primera = await ListarAsync(transaccion, Filtro(page: 1, pageSize: 100));
        var segunda = await ListarAsync(transaccion, Filtro(page: 2, pageSize: 100));

        Assert.Equal(100, primera.Items.Count);
        Assert.Equal(101, primera.TotalItems);
        Assert.Equal(tickets[100].Id, primera.Items[0].Id);
        // El más antiguo queda solo en la segunda página.
        Assert.Equal(tickets[0].Id, Assert.Single(segunda.Items).Id);
        Assert.Equal(101, segunda.TotalItems);
    }

    [Fact]
    [Trait("RN", "RN-05")]
    public async Task Listar_PaginaIntMaxValue_ItemsVacioSinError()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        await GuardarListadoAsync(transaccion, CrearTicket(1, TicketsDePrueba.Creacion), CrearTicket(2, TicketsDePrueba.Creacion));

        // (int.MaxValue - 1) * 100 no cabe en int.
        var pagina = await ListarAsync(transaccion, Filtro(page: int.MaxValue, pageSize: 100));

        Assert.Empty(pagina.Items);
        Assert.Equal(int.MaxValue, pagina.Page);
        Assert.Equal(2, pagina.TotalItems);
    }

    [Fact]
    public async Task Listar_Resumen_TecnicoNullSinAsignarYNombreDelAsignado()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var creacion = TicketsDePrueba.Creacion;
        var tickets = await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, creacion, prioridad: Prioridad.Critica),
            CrearTicket(2, creacion.AddHours(1), EstadoTicket.Asignado, tecnicoId: Gabriela.Id));

        var pagina = await ListarAsync(transaccion, Filtro());

        var asignado = pagina.Items[0];
        Assert.Equal(tickets[1].Id, asignado.Id);
        Assert.Equal("HD-000002", asignado.Codigo);
        Assert.Equal(TicketsDePrueba.Titulo, asignado.Titulo);
        Assert.Equal(EstadoTicket.Asignado, asignado.Estado);
        Assert.Equal(Prioridad.Media, asignado.Prioridad);
        Assert.Equal(Gabriela, asignado.Tecnico);
        Assert.Equal(tickets[1].FechaVencimiento, asignado.FechaVencimiento);

        var sinAsignar = pagina.Items[1];
        Assert.Equal(tickets[0].Id, sinAsignar.Id);
        Assert.Equal(EstadoTicket.Abierto, sinAsignar.Estado);
        Assert.Equal(Prioridad.Critica, sinAsignar.Prioridad);
        // Sin técnico, null (decisión 49).
        Assert.Null(sinAsignar.Tecnico);
    }

    [Fact]
    [Trait("CA", "CA-05.2")]
    [Trait("CA", "CA-05.3")]
    [Trait("RN", "RN-11")]
    public async Task Listar_Resumen_EstadoSlaDeCadaTicket()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var creacion = TicketsDePrueba.Creacion;
        await GuardarListadoAsync(
            transaccion,
            // Media: vence a las 16:00, sin resolver a las 14:30.
            CrearTicket(1, creacion),
            // Crítica: vence a las 10:00, sin resolver a las 14:30.
            CrearTicket(2, creacion, prioridad: Prioridad.Critica),
            // Media resuelta a las 08:30, antes de las 16:00.
            CrearTicket(3, creacion, EstadoTicket.Resuelto),
            // Alta resuelta a las 13:00, después de las 12:00, y cerrada.
            CrearTicket(4, creacion, EstadoTicket.Cerrado, Prioridad.Alta, resolucion: creacion.AddHours(5)));

        var pagina = await ListarAsync(transaccion, Filtro());

        var estados = pagina.Items.ToDictionary(i => i.Codigo, i => i.EstadoSla);
        Assert.Equal(EstadoSla.EnTiempo, estados["HD-000001"]);
        Assert.Equal(EstadoSla.Vencido, estados["HD-000002"]);
        Assert.Equal(EstadoSla.Cumplido, estados["HD-000003"]);
        Assert.Equal(EstadoSla.Incumplido, estados["HD-000004"]);
    }

    [Fact]
    public async Task Listar_Resumen_FechaVencimientoUtc()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var tickets = await GuardarListadoAsync(transaccion, CrearTicket(1, TicketsDePrueba.Creacion));

        var pagina = await ListarAsync(transaccion, Filtro());

        AssertFechaUtc(tickets[0].FechaVencimiento, Assert.Single(pagina.Items).FechaVencimiento);
    }

    [Fact]
    public async Task Listar_PaginaConVariosTecnicos_EjecutaDosConsultas()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var creacion = TicketsDePrueba.Creacion;
        await GuardarListadoAsync(
            transaccion,
            CrearTicket(1, creacion, EstadoTicket.Asignado),
            CrearTicket(2, creacion, EstadoTicket.Asignado, tecnicoId: Gabriela.Id),
            CrearTicket(3, creacion, EstadoTicket.EnProgreso, tecnicoId: Hugo.Id),
            CrearTicket(4, creacion));
        var contador = new ContadorDeComandos();

        var pagina = await ListarAsync(transaccion, Filtro(), contador: contador);

        // El conteo y la página con el nombre del técnico, no una por ticket ni por técnico.
        Assert.Equal(4, pagina.Items.Count);
        Assert.Equal([Fabian, Gabriela, Hugo], pagina.Items.Where(i => i.Tecnico is not null).Select(i => i.Tecnico!).OrderBy(p => p.Id));
        Assert.Equal(2, contador.Consultas);
    }

    [Fact]
    public async Task Listar_SolicitanteYSoloVencidos_GeneraWhereOrderByDescOffsetFetchYHoraComoParametro()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        await GuardarListadoAsync(transaccion, CrearTicket(1, TicketsDePrueba.Creacion, prioridad: Prioridad.Critica));
        var contador = new ContadorDeComandos();

        var pagina = await ListarAsync(
            transaccion, Filtro() with { SolicitanteId = Ana.Id, SoloVencidos = true }, contador: contador);

        Assert.Single(pagina.Items);
        Assert.Equal(2, contador.Comandos.Count);
        foreach (var comando in contador.Comandos)
        {
            salida.WriteLine(comando.Texto);
            salida.WriteLine("-- Parámetros: " + string.Join(", ", comando.Valores));
            Assert.Contains("WHERE", comando.Texto);
            Assert.Contains("[SolicitanteId]", comando.Texto);
            Assert.Contains("[FechaVencimiento]", comando.Texto);
            // La hora no está escrita en el SQL: viaja como parámetro.
            Assert.DoesNotContain("2026", comando.Texto);
            Assert.Contains(TicketsDePrueba.Ahora, comando.Valores.OfType<DateTime>());
        }

        Assert.Contains("COUNT(*)", contador.Comandos[0].Texto);
        Assert.Matches(@"ORDER BY \[\w+\]\.\[FechaCreacion\] DESC, \[\w+\]\.\[Id\] DESC", contador.Comandos[1].Texto);
        Assert.Contains("OFFSET", contador.Comandos[1].Texto);
        Assert.Contains("FETCH NEXT", contador.Comandos[1].Texto);
    }

    private static FiltroListadoTickets Filtro(int page = 1, int pageSize = 20) =>
        new(SolicitanteId: null, TecnicoId: null, Estado: null, Prioridad: null, SoloVencidos: false, page, pageSize);

    private static int[] Ids(params Ticket[] tickets) => tickets.Select(t => t.Id).ToArray();

    /// <summary>
    /// Ticket con código propio (índice único) recorriendo las transiciones de RN-08. Con <paramref name="resolucion"/>
    /// se fija la hora de resolución, para obtener un SLA <c>Incumplido</c>.
    /// </summary>
    private static Ticket CrearTicket(
        int numero,
        DateTime creacion,
        EstadoTicket estado = EstadoTicket.Abierto,
        Prioridad prioridad = Prioridad.Media,
        int solicitanteId = TicketsDePrueba.SolicitanteId,
        int tecnicoId = TicketsDePrueba.TecnicoId,
        DateTime? resolucion = null)
    {
        var ticket = Ticket.Crear(
            $"HD-{numero:D6}", TicketsDePrueba.Titulo, TicketsDePrueba.Descripcion, Categoria.Software, prioridad, solicitanteId, creacion);
        if (estado == EstadoTicket.Abierto)
        {
            return ticket;
        }

        ticket.Asignar(tecnicoId, creacion.AddMinutes(10));
        if (estado == EstadoTicket.Asignado)
        {
            return ticket;
        }

        ticket.IniciarAtencion(creacion.AddMinutes(20));
        if (estado == EstadoTicket.EnProgreso)
        {
            return ticket;
        }

        var fechaResolucion = resolucion ?? creacion.AddMinutes(30);
        ticket.Resolver(TicketsDePrueba.Solucion, tecnicoId, fechaResolucion);
        if (estado == EstadoTicket.Resuelto)
        {
            return ticket;
        }

        ticket.Cerrar(fechaResolucion.AddMinutes(10));
        return ticket;
    }

    /// <summary>
    /// Deja en la transacción solo los tickets de la prueba, para que el total no dependa de otros datos.
    /// </summary>
    private static async Task<Ticket[]> GuardarListadoAsync(TransaccionDePrueba transaccion, params Ticket[] tickets)
    {
        await using var context = transaccion.NuevoContexto();
        await context.Database.ExecuteSqlRawAsync(BorrarTickets);
        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();

        return tickets;
    }

    private static async Task<PaginaDto<TicketResumenDto>> ListarAsync(
        TransaccionDePrueba transaccion,
        FiltroListadoTickets filtro,
        DateTime? ahora = null,
        ContadorDeComandos? contador = null)
    {
        await using var context = contador is null ? transaccion.NuevoContexto() : transaccion.NuevoContexto(contador);
        return await new ConsultasTicket(context).ListarAsync(filtro, ahora ?? TicketsDePrueba.Ahora, CancellationToken.None);
    }

    private static async Task<TicketDetalleDto?> ObtenerDetalleAsync(TransaccionDePrueba transaccion, int ticketId, DateTime ahora)
    {
        await using var context = transaccion.NuevoContexto();
        return await new ConsultasTicket(context).ObtenerDetalleAsync(ticketId, ahora, CancellationToken.None);
    }

    private static async Task GuardarAsync(TransaccionDePrueba transaccion, Ticket ticket)
    {
        await using var context = transaccion.NuevoContexto();
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Guarda un comentario por vez, para que los Id sigan el orden de inserción, con fechas que no siguen ese orden:
    /// Solución de Fabián a las 10:00, reapertura de Ana a las 09:00 y solución de Gabriela a las 09:00.
    /// Domain no valida el autor ni que <c>ahora</c> avance (decisión 77).
    /// </summary>
    private static async Task<(int TicketId, int[] ComentarioIds)> GuardarConTresComentariosAsync(TransaccionDePrueba transaccion)
    {
        var nueve = TicketsDePrueba.Creacion.AddHours(1);
        var diez = TicketsDePrueba.Creacion.AddHours(2);

        await using var context = transaccion.NuevoContexto();
        var ticket = TicketsDePrueba.EnEstado(EstadoTicket.EnProgreso);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        ticket.Resolver(TicketsDePrueba.Solucion, Fabian.Id, diez);
        await context.SaveChangesAsync();
        ticket.Reabrir(TicketsDePrueba.Motivo, Ana.Id, nueve);
        await context.SaveChangesAsync();
        ticket.Resolver(TicketsDePrueba.Solucion, Gabriela.Id, nueve);
        await context.SaveChangesAsync();

        return (ticket.Id, ticket.Comentarios.Select(c => c.Id).ToArray());
    }

    private static void AssertFechaUtc(DateTime? esperada, DateTime? leida)
    {
        Assert.NotNull(leida);
        // DateTime.Equals ignora Kind: se comprueba aparte.
        Assert.Equal(esperada, leida);
        Assert.Equal(DateTimeKind.Utc, leida.Value.Kind);
    }
}
