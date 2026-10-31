using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Integracion;

[Collection(BaseDePruebasColeccion.Nombre)]
public class TicketPersistenciaTests(BaseDePruebasFixture baseDePruebas)
{
    // "Cannot insert duplicate key row in object with unique index."
    private const int ErrorIndiceUnicoDuplicado = 2601;

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task SaveChanges_DosTicketsConMismoCodigo_LanzaDbUpdateException()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        await using var context = transaccion.NuevoContexto();
        context.Tickets.Add(TicketsDePrueba.Nuevo());
        await context.SaveChangesAsync();

        context.Tickets.Add(TicketsDePrueba.Nuevo());

        var excepcion = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.Equal(ErrorIndiceUnicoDuplicado, Assert.IsType<SqlException>(excepcion.InnerException).Number);

        await using var lectura = transaccion.NuevoContexto();
        Assert.Equal(1, await lectura.Tickets.CountAsync(t => t.Codigo == TicketsDePrueba.Codigo));
    }

    [Fact]
    public async Task Ticket_GuardadoYLeidoEnOtroContexto_ConservaFechasUtc()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var ticket = TicketsDePrueba.EnEstado(EstadoTicket.Cerrado);
        await using (var context = transaccion.NuevoContexto())
        {
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        await using var lectura = transaccion.NuevoContexto();
        var leido = await lectura.Tickets.SingleAsync(t => t.Id == ticket.Id);

        Assert.NotSame(ticket, leido);
        AssertFechaUtc(ticket.FechaCreacion, leido.FechaCreacion);
        AssertFechaUtc(ticket.FechaActualizacion, leido.FechaActualizacion);
        AssertFechaUtc(ticket.FechaVencimiento, leido.FechaVencimiento);
        AssertFechaUtc(ticket.FechaResolucion, leido.FechaResolucion);
        AssertFechaUtc(ticket.FechaCierre, leido.FechaCierre);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public async Task Ticket_ResueltoGuardadoYLeido_IncluyeComentarioSolucion()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var ticket = TicketsDePrueba.EnEstado(EstadoTicket.Resuelto);
        await using (var context = transaccion.NuevoContexto())
        {
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        await using var lectura = transaccion.NuevoContexto();
        // EF Core llena el campo privado _comentarios (acceso por campo); Comentarios es de solo lectura.
        var leido = await lectura.Tickets.Include(t => t.Comentarios).SingleAsync(t => t.Id == ticket.Id);

        var comentario = Assert.Single(leido.Comentarios);
        Assert.Equal(ticket.Id, comentario.TicketId);
        Assert.Equal(TipoComentario.Solucion, comentario.Tipo);
        Assert.Equal(TicketsDePrueba.Solucion, comentario.Texto);
        Assert.Equal(TicketsDePrueba.TecnicoId, comentario.AutorId);
        AssertFechaUtc(leido.FechaResolucion, comentario.FechaCreacion);
    }

    private static void AssertFechaUtc(DateTime? esperada, DateTime? leida)
    {
        Assert.NotNull(leida);
        // DateTime.Equals ignora Kind: se comprueba aparte.
        Assert.Equal(esperada, leida);
        Assert.Equal(DateTimeKind.Utc, leida.Value.Kind);
    }
}
