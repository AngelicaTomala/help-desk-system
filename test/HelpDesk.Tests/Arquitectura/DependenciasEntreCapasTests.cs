using System.Reflection;
using NetArchTest.Rules;
using static HelpDesk.Tests.Arquitectura.Ensamblados;

namespace HelpDesk.Tests.Arquitectura;

/// <summary>
/// Regla de dependencias de C4 §3 sobre los tipos compilados (RNF-07, decisión 36).
/// </summary>
public class DependenciasEntreCapasTests
{
    // Program y sus tipos anidados generados por el compilador (lambdas de los top-level statements).
    private const string TipoRegistroDependencias = "Program";

    [Fact]
    public void Domain_TiposDelEnsamblado_NoDependenDeOtrasCapas()
    {
        AfirmarSinDependencias(CapaDomain, NamespaceApplication, NamespaceInfrastructure, NamespaceApi);
    }

    [Fact]
    public void Domain_TiposDelEnsamblado_NoDependenDeEfCoreAspNetCoreNiFluentValidation()
    {
        AfirmarSinDependencias(CapaDomain, NamespaceEfCore, NamespaceAspNetCore, NamespaceFluentValidation);
    }

    [Fact]
    public void Application_TiposDelEnsamblado_NoDependenDeInfrastructureNiApi()
    {
        AfirmarSinDependencias(CapaApplication, NamespaceInfrastructure, NamespaceApi);
    }

    [Fact]
    public void Application_TiposDelEnsamblado_NoDependenDeEfCoreNiAspNetCore()
    {
        AfirmarSinDependencias(CapaApplication, NamespaceEfCore, NamespaceAspNetCore);
    }

    [Fact]
    public void Infrastructure_TiposDelEnsamblado_NoDependenDeApi()
    {
        AfirmarSinDependencias(CapaInfrastructure, NamespaceApi);
    }

    [Fact]
    public void Api_TiposDistintosDeProgram_NoDependenDeInfrastructure()
    {
        var infractores = TiposQueDependen(CapaApi, NamespaceInfrastructure)
            .Where(nombre => !EsRegistroDependencias(nombre))
            .ToList();

        Assert.True(infractores.Count == 0, Mensaje(CapaApi, [NamespaceInfrastructure], infractores));
    }

    private static void AfirmarSinDependencias(Assembly ensamblado, params string[] prohibidos)
    {
        var infractores = TiposQueDependen(ensamblado, prohibidos);

        Assert.True(infractores.Count == 0, Mensaje(ensamblado, prohibidos, infractores));
    }

    private static List<string> TiposQueDependen(Assembly ensamblado, params string[] prohibidos)
    {
        var resultado = Types.InAssembly(ensamblado)
            .Should()
            .NotHaveDependencyOnAny(prohibidos)
            .GetResult();

        return resultado.IsSuccessful ? [] : [.. resultado.FailingTypeNames ?? []];
    }

    // Mono.Cecil separa los tipos anidados con "/"; la reflexión, con "+".
    private static bool EsRegistroDependencias(string nombreCompleto) =>
        nombreCompleto == TipoRegistroDependencias
        || nombreCompleto.StartsWith(TipoRegistroDependencias + "/", StringComparison.Ordinal)
        || nombreCompleto.StartsWith(TipoRegistroDependencias + "+", StringComparison.Ordinal);

    private static string Mensaje(Assembly ensamblado, IEnumerable<string> prohibidos, IEnumerable<string> infractores) =>
        $"{ensamblado.GetName().Name} no debe depender de [{string.Join(", ", prohibidos)}], " +
        $"pero estos tipos sí dependen: {string.Join(", ", infractores)}";
}
