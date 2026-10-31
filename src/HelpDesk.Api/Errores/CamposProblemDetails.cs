namespace HelpDesk.Api.Errores;

/// <summary>
/// Campos de extensión y tipo de contenido de los Problem Details del contrato (§1.3, decisiones 29 y 62).
/// </summary>
public static class CamposProblemDetails
{
    public const string Codigo = "codigo";
    public const string TipoContenido = "application/problem+json";
}
