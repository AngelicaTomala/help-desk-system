using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HelpDesk.Api.Documentacion;

/// <summary>
/// Declara el encabezado <c>Location</c>, una ruta relativa, en las respuestas <c>201</c> (decisión 64, RNF-09).
/// </summary>
public sealed class TransformadorLocation : IOpenApiOperationTransformer
{
    public const string Encabezado = "Location";

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (operation.Responses?.TryGetValue("201", out var respuesta) == true && respuesta is OpenApiResponse creada)
        {
            creada.Headers ??= new Dictionary<string, IOpenApiHeader>();
            creada.Headers[Encabezado] = new OpenApiHeader
            {
                Description = "Ruta relativa del recurso creado, por ejemplo /api/v1/tickets/16.",
                Required = true,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            };
        }

        return Task.CompletedTask;
    }
}
