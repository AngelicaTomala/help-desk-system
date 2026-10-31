namespace HelpDesk.Tests.Rendimiento;

/// <summary>
/// Prueba explícita (decisión 110): xUnit 2 no tiene <c>Explicit</c>, así que la prueba queda omitida en un
/// <c>dotnet test</c> normal y solo se ejecuta con la variable de entorno <see cref="Variable"/> en <c>1</c>.
/// </summary>
public sealed class PruebaExplicitaAttribute : FactAttribute
{
    public const string Variable = "HELPDESK_RENDIMIENTO";

    public PruebaExplicitaAttribute()
    {
        if (Environment.GetEnvironmentVariable(Variable) != "1")
        {
            Skip = $"Prueba explícita de RNF-06: define {Variable}=1 para ejecutarla (decisión 110).";
        }
    }
}
