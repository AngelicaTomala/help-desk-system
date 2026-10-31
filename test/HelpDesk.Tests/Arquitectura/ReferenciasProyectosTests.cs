using System.Xml.Linq;

namespace HelpDesk.Tests.Arquitectura;

/// <summary>
/// ProjectReference y PackageReference de los .csproj de src/ según C4 §3 (RNF-07, revisión de los .csproj).
/// </summary>
public class ReferenciasProyectosTests
{
    private const string ArchivoSolucion = "HelpDesk.slnx";

    public static TheoryData<string, string[]> ReferenciasPermitidas => new()
    {
        { "HelpDesk.Domain", [] },
        { "HelpDesk.Application", ["HelpDesk.Domain"] },
        { "HelpDesk.Infrastructure", ["HelpDesk.Application", "HelpDesk.Domain"] },
        { "HelpDesk.Api", ["HelpDesk.Application", "HelpDesk.Infrastructure"] },
    };

    [Theory]
    [MemberData(nameof(ReferenciasPermitidas))]
    public void ProjectReference_DeCadaProyecto_CoincidenConLaReglaDeDependencias(string proyecto, string[] esperadas)
    {
        var actuales = Elementos(proyecto, "ProjectReference")
            .Select(elemento => Path.GetFileNameWithoutExtension((string?)elemento.Attribute("Include") ?? string.Empty))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sobran = actuales.Except(esperadas, StringComparer.OrdinalIgnoreCase).Order().ToList();
        var faltan = esperadas.Except(actuales, StringComparer.OrdinalIgnoreCase).Order().ToList();

        Assert.True(
            sobran.Count == 0 && faltan.Count == 0,
            $"{proyecto}.csproj no cumple la regla de dependencias. " +
            $"Sobran: [{string.Join(", ", sobran)}]. Faltan: [{string.Join(", ", faltan)}].");
    }

    [Fact]
    public void DomainCsproj_PackageReference_NoTieneNinguno()
    {
        var paquetes = Elementos("HelpDesk.Domain", "PackageReference")
            .Select(elemento => (string?)elemento.Attribute("Include") ?? string.Empty)
            .ToList();

        Assert.True(
            paquetes.Count == 0,
            $"HelpDesk.Domain.csproj no debe referenciar paquetes, pero referencia: [{string.Join(", ", paquetes)}].");
    }

    private static IEnumerable<XElement> Elementos(string proyecto, string nombre)
    {
        var ruta = Path.Combine(RaizSolucion(), "src", proyecto, $"{proyecto}.csproj");

        return XDocument.Load(ruta).Descendants().Where(elemento => elemento.Name.LocalName == nombre);
    }

    private static string RaizSolucion()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (directorio is not null && !File.Exists(Path.Combine(directorio.FullName, ArchivoSolucion)))
        {
            directorio = directorio.Parent;
        }

        return directorio?.FullName
            ?? throw new InvalidOperationException($"No se encontró {ArchivoSolucion} desde {AppContext.BaseDirectory}.");
    }
}
