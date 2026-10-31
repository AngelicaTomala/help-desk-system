using HelpDesk.Application.Abstracciones;
using HelpDesk.Infrastructure;
using HelpDesk.Infrastructure.Persistencia;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests.Infrastructure;

/// <summary>
/// Registro de dependencias sin abrir conexión: resolver el DbContext no conecta con la base.
/// </summary>
public class InyeccionDependenciasTests
{
    // Valor ficticio; nunca se usa para conectar.
    private const string DataSourceFicticio = "sin-conexion";
    private const string CadenaConexionFicticia = $"Data Source={DataSourceFicticio}";

    [Fact]
    public void AgregarInfraestructura_ConCadenaConexion_RegistraDbContextSqlServerScoped()
    {
        var services = Registrar(CadenaConexionFicticia);

        Assert.Equal(ServiceLifetime.Scoped, Descriptor<HelpDeskDbContext>(services).Lifetime);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HelpDeskDbContext>();

        Assert.True(context.Database.IsSqlServer());
        // EF Core agrega "Application Name" a la cadena; basta con comprobar el origen leído de la configuración.
        var cadenaUsada = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
        Assert.Equal(DataSourceFicticio, cadenaUsada.DataSource);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public void AgregarInfraestructura_ConCadenaConexion_RegistraGeneradorCodigoTicketScoped()
    {
        var services = Registrar(CadenaConexionFicticia);

        Assert.Equal(ServiceLifetime.Scoped, Descriptor<IGeneradorCodigoTicket>(services).Lifetime);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.IsType<GeneradorCodigoTicket>(scope.ServiceProvider.GetRequiredService<IGeneradorCodigoTicket>());
    }

    [Theory]
    [InlineData(typeof(ITicketRepositorio), typeof(TicketRepositorio))]
    [InlineData(typeof(IUnidadDeTrabajo), typeof(UnidadDeTrabajo))]
    [InlineData(typeof(IConsultasTicket), typeof(ConsultasTicket))]
    [InlineData(typeof(IUsuarioRepositorio), typeof(UsuarioRepositorio))]
    public void AgregarInfraestructura_ConCadenaConexion_RegistraPersistenciaScoped(Type servicio, Type implementacion)
    {
        var services = Registrar(CadenaConexionFicticia);

        // Mismo tiempo de vida que el DbContext, para compartir la instancia de la petición.
        Assert.Equal(ServiceLifetime.Scoped, Assert.Single(services, d => d.ServiceType == servicio).Lifetime);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.IsType(implementacion, scope.ServiceProvider.GetRequiredService(servicio));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgregarInfraestructura_SinCadenaConexion_LanzaExcepcionQueNombraLaClave(string? cadenaConexion)
    {
        var excepcion = Assert.Throws<InvalidOperationException>(() => Registrar(cadenaConexion));

        Assert.Contains("ConnectionStrings:HelpDesk", excepcion.Message);
    }

    private static ServiceCollection Registrar(string? cadenaConexion)
    {
        var valores = new Dictionary<string, string?>();
        if (cadenaConexion is not null)
        {
            valores["ConnectionStrings:HelpDesk"] = cadenaConexion;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(valores).Build();
        var services = new ServiceCollection();
        services.AgregarInfraestructura(configuration);

        return services;
    }

    private static ServiceDescriptor Descriptor<T>(IServiceCollection services) =>
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(T));
}
