using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HelpDesk.Api.Documentacion;

/// <summary>
/// Agrega a la operación los parámetros de la URL declarados con <see cref="ParametroUrlAttribute"/>, que el generador
/// no ve porque la acción los lee de <c>Request.Query</c> (decisión 101, RNF-09). Ninguno es obligatorio: un parámetro
/// ausente toma su valor por defecto o no filtra (decisión 104).
/// </summary>
public sealed class TransformadorParametrosUrl : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor accion)
        {
            foreach (var parametro in accion.MethodInfo.GetCustomAttributes<ParametroUrlAttribute>())
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = parametro.Nombre,
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = parametro.Descripcion(),
                    Schema = parametro.CrearEsquema(),
                });
            }
        }

        return Task.CompletedTask;
    }
}
