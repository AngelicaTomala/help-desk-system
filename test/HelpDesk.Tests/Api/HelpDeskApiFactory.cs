using HelpDesk.Api.Identidad;
using HelpDesk.Tests.Api.Dobles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Api en memoria con la cadena de la base de pruebas, <see cref="FakeTimeProvider"/> y el controlador de pruebas,
/// que solo existe en este proyecto (decisión 99).
/// </summary>
public class HelpDeskApiFactory : WebApplicationFactory<Program>
{
    private const string NombreCadenaPruebas = "HelpDeskPruebas";
    private const string BaseDeDesarrollo = "HelpDesk";

    /// <summary>
    /// Ana Solicitante, usuario semilla 1 (decisión 89); las pruebas que no dependen del rol lo usan.
    /// </summary>
    public const string IdUsuarioPorDefecto = "1";

    public RegistroDeLogs Logs { get; } = new();

    /// <summary>
    /// Cliente con <c>X-Usuario-Id</c> en todas las peticiones, para llegar a las acciones (RNF-01).
    /// </summary>
    public HttpClient CrearClienteIdentificado(string idUsuario = IdUsuarioPorDefecto)
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Add(MiddlewareUsuarioActual.Encabezado, idUsuario);
        return cliente;
    }

    public FakeTimeProvider Reloj { get; } = new(new DateTimeOffset(2026, 10, 28, 14, 30, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:HelpDesk", LeerCadenaConexion());
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Reloj);
            services.AddControllers().AddApplicationPart(typeof(PruebasController).Assembly);
        });
    }

    /// <summary>
    /// Misma lectura y guarda que <c>BaseDePruebasFixture</c> (pendiente unificarlas): user-secrets o variable de
    /// entorno, nunca el repositorio (RNF-02), y nunca la base de desarrollo.
    /// </summary>
    private static string LeerCadenaConexion()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<HelpDeskApiFactory>()
            .AddEnvironmentVariables()
            .Build();

        var cadenaConexion = configuration.GetConnectionString(NombreCadenaPruebas);
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{NombreCadenaPruebas}'. " +
                "Configúrala con user-secrets del proyecto de pruebas o con una variable de entorno (RNF-02).");
        }

        var catalogo = new SqlConnectionStringBuilder(cadenaConexion).InitialCatalog;
        if (string.IsNullOrWhiteSpace(catalogo) || string.Equals(catalogo, BaseDeDesarrollo, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"La cadena 'ConnectionStrings:{NombreCadenaPruebas}' debe indicar una base de pruebas distinta de '{BaseDeDesarrollo}'.");
        }

        return cadenaConexion;
    }
}
