using HelpDesk.Api.Identidad;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HelpDesk.Api.Documentacion;

/// <summary>
/// Declara <c>X-Usuario-Id</c> como obligatorio en toda acción de controlador, el mismo criterio que
/// <see cref="MiddlewareUsuarioActual"/> (RNF-01, RNF-09). El formato es el de la decisión 95; el documento y Swagger UI
/// siguen sin exigirlo (decisión 52).
/// </summary>
public sealed class TransformadorEncabezadoUsuario : IOpenApiOperationTransformer
{
    public const string Patron = "^[1-9][0-9]{0,9}$";

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor)
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = MiddlewareUsuarioActual.Encabezado,
                In = ParameterLocation.Header,
                Required = true,
                Description =
                    "Id del usuario: entero positivo hasta 2147483647, solo dígitos ASCII, sin signo, sin espacios y " +
                    "sin ceros a la izquierda. Si falta, no es válido o se repite: 401 USUARIO_NO_IDENTIFICADO; si no " +
                    "corresponde a ningún usuario: 401 USUARIO_INEXISTENTE.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Pattern = Patron },
            });
        }

        return Task.CompletedTask;
    }
}
