using HelpDesk.Domain.Entidades;
using HelpDesk.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Crea la base de pruebas desde cero con la migración Inicial antes de las pruebas y la conserva al
/// terminar para revisarla; la ejecución siguiente la vuelve a crear (ADR-002, Enmienda 2: LocalDB).
/// </summary>
public class BaseDePruebasFixture : IAsyncLifetime
{
    // SQL constante, sin concatenación ni interpolación (RNF-02). La SEQUENCE no se revierte con una transacción, así
    // que se reinicia con RESTART WITH explícito (decisión 99).
    private const string BorrarTickets =
        "DELETE FROM dbo.Comentarios; DELETE FROM dbo.Tickets; ALTER SEQUENCE dbo.CodigoTicket RESTART WITH 1";

    private const string ReiniciarSecuenciaEnMaximo = "ALTER SEQUENCE dbo.CodigoTicket RESTART WITH 999999";

    public string CadenaConexion { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        CadenaConexion = CadenaConexionPruebas.Leer();

        await using var context = CrearContexto();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public HelpDeskDbContext CrearContexto() =>
        new(new DbContextOptionsBuilder<HelpDeskDbContext>().UseSqlServer(CadenaConexion).Options);

    public Task<TransaccionDePrueba> IniciarTransaccionAsync() => TransaccionDePrueba.IniciarAsync(CadenaConexion);

    /// <summary>
    /// Aislamiento de las pruebas de extremo a extremo: sin tickets ni comentarios y la SEQUENCE en 1 (decisión 99).
    /// </summary>
    public async Task ReiniciarTicketsAsync()
    {
        await using var context = CrearContexto();
        await context.Database.ExecuteSqlRawAsync(BorrarTickets);
    }

    /// <summary>
    /// Deja la SEQUENCE en su último valor, <c>HD-999999</c> (decisión 90).
    /// </summary>
    public async Task ReiniciarSecuenciaEnMaximoAsync()
    {
        await using var context = CrearContexto();
        await context.Database.ExecuteSqlRawAsync(ReiniciarSecuenciaEnMaximo);
    }

    /// <summary>
    /// Guarda un ticket preparado con Domain, fuera de la Api, y devuelve su Id.
    /// </summary>
    public async Task<int> GuardarAsync(Ticket ticket)
    {
        await using var context = CrearContexto();
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        return ticket.Id;
    }
}
