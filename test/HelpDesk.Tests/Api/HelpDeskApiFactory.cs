using HelpDesk.Api.Identidad;
using HelpDesk.Tests.Api.Dobles;
using HelpDesk.Tests.Integracion;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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

    // Lectura y guarda compartidas con BaseDePruebasFixture: nunca la base de desarrollo (decisión 99).
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:HelpDesk", CadenaConexionPruebas.Leer());
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Reloj);
            services.AddControllers().AddApplicationPart(typeof(PruebasController).Assembly);
        });
    }
}
