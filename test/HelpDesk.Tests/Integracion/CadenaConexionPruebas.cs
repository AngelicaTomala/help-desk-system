using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Cadena de la base de pruebas, compartida por <see cref="BaseDePruebasFixture"/> y la factory de la Api
/// (decisiones 91 y 99).
/// </summary>
public static class CadenaConexionPruebas
{
    public const string Nombre = "HelpDeskPruebas";

    private const string BaseDeDesarrollo = "HelpDesk";

    /// <summary>
    /// La cadena se lee de los user-secrets del proyecto de pruebas o de una variable de entorno, nunca del
    /// repositorio (RNF-02). La guarda impide borrar, migrar o usar la base de desarrollo.
    /// </summary>
    public static string Leer()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(CadenaConexionPruebas).Assembly)
            .AddEnvironmentVariables()
            .Build();

        var cadenaConexion = configuration.GetConnectionString(Nombre);
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{Nombre}'. " +
                "Configúrala con user-secrets del proyecto de pruebas o con una variable de entorno (RNF-02).");
        }

        var catalogo = new SqlConnectionStringBuilder(cadenaConexion).InitialCatalog;
        if (string.IsNullOrWhiteSpace(catalogo) || string.Equals(catalogo, BaseDeDesarrollo, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"La cadena 'ConnectionStrings:{Nombre}' debe indicar una base de pruebas distinta de '{BaseDeDesarrollo}'.");
        }

        return cadenaConexion;
    }
}
