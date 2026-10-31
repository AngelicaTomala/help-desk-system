using HelpDesk.Api.Documentacion;

namespace HelpDesk.Api.Configuracion;

/// <summary>
/// Registra el documento OpenAPI con el encabezado <c>X-Usuario-Id</c>, los parámetros de la URL que se leen como
/// texto, el <c>Location</c> del <c>201</c>, los enums del cuerpo y los campos de Problem Details (RNF-09).
/// </summary>
public static class InyeccionDocumentacion
{
    public static IServiceCollection AgregarDocumentacion(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<TransformadorEncabezadoUsuario>();
            options.AddOperationTransformer<TransformadorParametrosUrl>();
            options.AddOperationTransformer<TransformadorLocation>();
            options.AddSchemaTransformer<TransformadorEsquemas>();
        });
}
