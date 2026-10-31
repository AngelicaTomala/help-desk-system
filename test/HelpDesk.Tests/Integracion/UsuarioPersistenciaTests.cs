using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Integracion;

[Collection(BaseDePruebasColeccion.Nombre)]
public class UsuarioPersistenciaTests(BaseDePruebasFixture baseDePruebas)
{
    // "The DELETE statement conflicted with the REFERENCE constraint."
    private const int ErrorConflictoClaveForanea = 547;

    [Fact]
    public async Task SaveChanges_BorrarSolicitanteConTicket_LanzaDbUpdateException()
    {
        await using var transaccion = await baseDePruebas.IniciarTransaccionAsync();
        await using (var context = transaccion.NuevoContexto())
        {
            context.Tickets.Add(TicketsDePrueba.Nuevo());
            await context.SaveChangesAsync();
        }

        // Contexto que no rastrea el ticket: el DELETE llega a SQL Server, que aplica el Restrict (decisión 87).
        await using var borrado = transaccion.NuevoContexto();
        var solicitante = await borrado.Usuarios.SingleAsync(u => u.Id == TicketsDePrueba.SolicitanteId);
        borrado.Usuarios.Remove(solicitante);

        var excepcion = await Assert.ThrowsAsync<DbUpdateException>(() => borrado.SaveChangesAsync());
        Assert.Equal(ErrorConflictoClaveForanea, Assert.IsType<SqlException>(excepcion.InnerException).Number);

        await using var lectura = transaccion.NuevoContexto();
        Assert.True(await lectura.Usuarios.AnyAsync(u => u.Id == TicketsDePrueba.SolicitanteId));
    }

    [Fact]
    public async Task Usuarios_BaseMigrada_SonLosDiezDeLaSemilla()
    {
        (int Id, string Nombre, string Email, Rol Rol, bool Activo)[] esperados =
        [
            (1, "Ana Solicitante", "ana.solicitante@example.com", Rol.Solicitante, true),
            (2, "Bruno Solicitante", "bruno.solicitante@example.com", Rol.Solicitante, true),
            (3, "Carla Solicitante", "carla.solicitante@example.com", Rol.Solicitante, true),
            (4, "Diego Solicitante", "diego.solicitante@example.com", Rol.Solicitante, true),
            (5, "Elena Solicitante", "elena.solicitante@example.com", Rol.Solicitante, true),
            (6, "Fabián Técnico", "fabian.tecnico@example.com", Rol.Tecnico, true),
            (7, "Gabriela Técnico", "gabriela.tecnico@example.com", Rol.Tecnico, true),
            (8, "Hugo Técnico", "hugo.tecnico@example.com", Rol.Tecnico, true),
            (9, "Irene Técnico", "irene.tecnico@example.com", Rol.Tecnico, false),
            (10, "Jorge Supervisor", "jorge.supervisor@example.com", Rol.Supervisor, true),
        ];

        await using var context = baseDePruebas.CrearContexto();
        var usuarios = await context.Usuarios.AsNoTracking().OrderBy(u => u.Id).ToListAsync();

        Assert.Equal(esperados, usuarios.Select(u => (u.Id, u.Nombre, u.Email, u.Rol, u.Activo)));
    }
}
