using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Infrastructure.Persistencia;
using HelpDesk.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Integracion;

[Collection(BaseDePruebasColeccion.Nombre)]
public class TicketRepositorioIntegracionTests(BaseDePruebasFixture baseDePruebas)
{
    [Fact]
    public async Task Agregar_TicketNuevo_SeLeeEnOtroContexto()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var ticket = TicketsDePrueba.Nuevo();
        await using (var context = transaccion.NuevoContexto())
        {
            new TicketRepositorio(context).Agregar(ticket);
            await new UnidadDeTrabajo(context).GuardarCambiosAsync(CancellationToken.None);
        }

        await using var lectura = transaccion.NuevoContexto();
        var leido = await lectura.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);

        Assert.Equal(TicketsDePrueba.Codigo, leido.Codigo);
        Assert.Equal(TicketsDePrueba.Titulo, leido.Titulo);
        Assert.Equal(EstadoTicket.Abierto, leido.Estado);
        Assert.Equal(TicketsDePrueba.SolicitanteId, leido.SolicitanteId);
        Assert.Null(leido.TecnicoId);
    }

    [Fact]
    public async Task ObtenerPorId_TicketConComentarios_CargaComentariosRastreados()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var id = await GuardarAsync(transaccion, TicketsDePrueba.EnEstado(EstadoTicket.Resuelto));

        await using var context = transaccion.NuevoContexto();
        var ticket = await new TicketRepositorio(context).ObtenerPorIdAsync(id, CancellationToken.None);

        Assert.NotNull(ticket);
        var comentario = Assert.Single(ticket.Comentarios);
        Assert.Equal(TipoComentario.Solucion, comentario.Tipo);
        Assert.Equal(EntityState.Unchanged, context.Entry(ticket).State);
    }

    [Fact]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    public async Task ObtenerPorId_Reclasificar_PersisteCategoriaYFechaActualizacion()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var id = await GuardarAsync(transaccion, TicketsDePrueba.Nuevo(categoria: Categoria.Software));

        await using (var context = transaccion.NuevoContexto())
        {
            var ticket = await new TicketRepositorio(context).ObtenerPorIdAsync(id, CancellationToken.None);
            ticket!.Reclasificar(Categoria.Hardware, TicketsDePrueba.Ahora);
            await new UnidadDeTrabajo(context).GuardarCambiosAsync(CancellationToken.None);
        }

        await using var lectura = transaccion.NuevoContexto();
        var leido = await lectura.Tickets.AsNoTracking().SingleAsync(t => t.Id == id);

        Assert.Equal(Categoria.Hardware, leido.Categoria);
        Assert.Equal(TicketsDePrueba.Ahora, leido.FechaActualizacion);
    }

    [Fact]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    public async Task ObtenerPorId_ReclasificarMismaCategoria_NoCambiaFechaActualizacion()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        var id = await GuardarAsync(transaccion, TicketsDePrueba.Nuevo(categoria: Categoria.Software));

        await using (var context = transaccion.NuevoContexto())
        {
            var ticket = await new TicketRepositorio(context).ObtenerPorIdAsync(id, CancellationToken.None);
            ticket!.Reclasificar(Categoria.Software, TicketsDePrueba.Ahora);

            // Decisión 67: sin cambios, guardar no envía nada a la base.
            Assert.False(context.ChangeTracker.HasChanges());
            await new UnidadDeTrabajo(context).GuardarCambiosAsync(CancellationToken.None);
        }

        await using var lectura = transaccion.NuevoContexto();
        var leido = await lectura.Tickets.AsNoTracking().SingleAsync(t => t.Id == id);

        Assert.Equal(Categoria.Software, leido.Categoria);
        Assert.Equal(TicketsDePrueba.Creacion, leido.FechaActualizacion);
    }

    [Fact]
    public async Task ObtenerPorId_TicketInexistente_DevuelveNull()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        await using var context = transaccion.NuevoContexto();

        var ticket = await new TicketRepositorio(context).ObtenerPorIdAsync(int.MaxValue, CancellationToken.None);

        Assert.Null(ticket);
    }

    private static async Task<int> GuardarAsync(TransaccionDePrueba transaccion, Ticket ticket)
    {
        await using var context = transaccion.NuevoContexto();
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        return ticket.Id;
    }
}
