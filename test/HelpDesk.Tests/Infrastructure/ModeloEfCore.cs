using HelpDesk.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HelpDesk.Tests.Infrastructure;

/// <summary>
/// Modelo de diseño de EF Core, construido sin abrir conexión. Se usa el de diseño porque conserva
/// los datos de <c>HasData</c> y las anotaciones relacionales que el modelo de ejecución descarta.
/// </summary>
internal static class ModeloEfCore
{
    private static readonly Lazy<IModel> Modelo = new(Construir);

    public static IModel Diseno => Modelo.Value;

    public static IProperty Propiedad(Type entidad, string propiedad) =>
        Diseno.FindEntityType(entidad)!.FindProperty(propiedad)!;

    private static IModel Construir()
    {
        var options = new DbContextOptionsBuilder<HelpDeskDbContext>().UseSqlServer().Options;
        using var context = new HelpDeskDbContext(options);

        return context.GetService<IDesignTimeModel>().Model;
    }
}
