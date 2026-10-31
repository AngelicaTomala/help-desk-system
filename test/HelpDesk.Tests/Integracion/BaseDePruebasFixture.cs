using HelpDesk.Infrastructure.Persistencia;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Crea la base de pruebas desde cero con la migración Inicial antes de las pruebas y la conserva al
/// terminar para revisarla; la ejecución siguiente la vuelve a crear (ADR-002, Enmienda 2: LocalDB).
/// </summary>
public class BaseDePruebasFixture : IAsyncLifetime
{
    public const string NombreCadenaConexion = "HelpDeskPruebas";

    private const string BaseDeDesarrollo = "HelpDesk";

    public string CadenaConexion { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        CadenaConexion = LeerCadenaConexion();

        await using var context = CrearContexto();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public HelpDeskDbContext CrearContexto() =>
        new(new DbContextOptionsBuilder<HelpDeskDbContext>().UseSqlServer(CadenaConexion).Options);

    public Task<TransaccionDePrueba> IniciarTransaccionAsync() => TransaccionDePrueba.IniciarAsync(CadenaConexion);

    /// <summary>
    /// La cadena se lee de los user-secrets del proyecto de pruebas o de una variable de entorno, nunca del
    /// repositorio (RNF-02). La guarda impide borrar o migrar la base de desarrollo.
    /// </summary>
    private static string LeerCadenaConexion()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<BaseDePruebasFixture>()
            .AddEnvironmentVariables()
            .Build();

        var cadenaConexion = configuration.GetConnectionString(NombreCadenaConexion);
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{NombreCadenaConexion}'. " +
                "Configúrala con user-secrets del proyecto de pruebas o con una variable de entorno (RNF-02).");
        }

        var catalogo = new SqlConnectionStringBuilder(cadenaConexion).InitialCatalog;
        if (string.IsNullOrWhiteSpace(catalogo) || string.Equals(catalogo, BaseDeDesarrollo, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"La cadena 'ConnectionStrings:{NombreCadenaConexion}' debe indicar una base de pruebas distinta de '{BaseDeDesarrollo}'.");
        }

        return cadenaConexion;
    }
}
