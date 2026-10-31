using HelpDesk.Application.Abstracciones;
using HelpDesk.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure;

/// <summary>
/// Registra los servicios de Infrastructure. La Api lo invoca solo desde <c>Program.cs</c> (RNF-07).
/// </summary>
public static class InyeccionDependencias
{
    public const string NombreCadenaConexion = "HelpDesk";

    /// <summary>
    /// La cadena de conexión se lee de la configuración (user-secrets o variables de entorno), nunca
    /// del repositorio (RNF-02). Se lee al registrar para que su ausencia falle al arrancar.
    /// </summary>
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        var cadenaConexion = configuration.GetConnectionString(NombreCadenaConexion);
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{NombreCadenaConexion}'. " +
                "Configúrala con user-secrets o con una variable de entorno (RNF-02).");
        }

        services.AddDbContext<HelpDeskDbContext>(options => options.UseSqlServer(cadenaConexion));
        services.AddScoped<IGeneradorCodigoTicket, GeneradorCodigoTicket>();

        return services;
    }
}
