namespace HelpDesk.Api.Errores;

/// <summary>
/// Valores del catálogo de <c>codigo</c> que decide la Api: los <c>401</c> de <c>X-Usuario-Id</c> (ADR-004, Enmienda 3).
/// </summary>
public static class CodigosErrorApi
{
    public const string UsuarioNoIdentificado = "USUARIO_NO_IDENTIFICADO";
    public const string UsuarioInexistente = "USUARIO_INEXISTENTE";
}
