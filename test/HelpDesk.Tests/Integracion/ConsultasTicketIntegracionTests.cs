using HelpDesk.Application.Tickets;
using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Infrastructure.Persistencia;
using HelpDesk.Tests.Domain;

namespace HelpDesk.Tests.Integracion;

[Collection(BaseDePruebasColeccion.Nombre)]
public class ConsultasTicketIntegracionTests(BaseDePruebasFixture baseDePruebas)
{
    private static readonly PersonaDto Ana = new(1, "Ana Solicitante");
    private static readonly PersonaDto Fabian = new(6, "Fabián Técnico");
    private static readonly PersonaDto Gabriela = new(7, "Gabriela Técnico");

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
