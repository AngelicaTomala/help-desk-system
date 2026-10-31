using System.Reflection;
using System.Text.Json.Nodes;
using HelpDesk.Api.Errores;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HelpDesk.Api.Documentacion;

/// <summary>
/// Completa los esquemas del documento (RNF-09): los valores del enum en los campos de texto marcados con
/// <see cref="ValoresEnumAttribute"/> (decisión 96) y los campos de extensión de Problem Details, <c>codigo</c> y
/// <c>traceId</c> (decisiones 29 y 62), y el tipo texto de enums y fechas. <c>codigo</c> no es obligatorio: las
/// respuestas de la decisión 94 no lo llevan.
/// </summary>
public sealed class TransformadorEsquemas : IOpenApiSchemaTransformer
{
    public const string TraceId = "traceId";

    public const string DescripcionEnum = "No distingue mayúsculas y minúsculas; no admite valores numéricos (decisión 58).";

    /// <summary>
    /// Catálogo de <c>codigo</c> de ADR-004 (Enmienda 3), en el orden de la tabla del contrato §1.3.
    /// </summary>
    public static readonly IReadOnlyList<string> Catalogo =
    [
        CodigosErrorAplicacion.ValidacionFallida,
        CodigosErrorAplicacion.TecnicoNoValido,
        CodigosErrorAplicacion.MismoTecnico,
        CodigosErrorApi.UsuarioNoIdentificado,
        CodigosErrorApi.UsuarioInexistente,
        CodigosErrorAplicacion.RolNoAutorizado,
        CodigosErrorAplicacion.TicketAjeno,
        CodigosErrorAplicacion.FiltroNoAutorizado,
        CodigosErrorAplicacion.TicketNoEncontrado,
        CodigosError.TransicionInvalida,
        CodigosError.TicketNoModificable,
        CodigosError.ErrorInterno,
    ];

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var valoresEnum = context.JsonPropertyInfo?.AttributeProvider?
            .GetCustomAttributes(typeof(ValoresEnumAttribute), inherit: false)
            .OfType<ValoresEnumAttribute>()
            .FirstOrDefault();

        if (valoresEnum is not null)
        {
            schema.Enum = [.. Enum.GetNames(valoresEnum.TipoEnum).Select(nombre => (JsonNode)nombre)];
            schema.Description = DescripcionEnum;
        }
        else if (context.JsonPropertyInfo is null && typeof(ProblemDetails).IsAssignableFrom(context.JsonTypeInfo.Type))
        {
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
            schema.Properties[CamposProblemDetails.Codigo] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Caso de negocio del catálogo de ADR-004; no aparece en los 404 de ruta, 405 y 415.",
                Enum = [.. Catalogo.Select(codigo => (JsonNode)codigo)],
            };
            schema.Properties[TraceId] = new OpenApiSchema { Type = JsonSchemaType.String };
        }

        // Con JsonStringEnumConverter y ConversorFechaUtcJson el generador omite el tipo: los enums son texto
        // (decisión 48) y las fechas, texto ISO 8601 en UTC (decisión 60).
        var tipo = context.JsonTypeInfo.Type;
        var subyacente = Nullable.GetUnderlyingType(tipo) ?? tipo;
        if (schema.Type is null && (subyacente.IsEnum || subyacente == typeof(DateTime)))
        {
            schema.Type = subyacente == tipo ? JsonSchemaType.String : JsonSchemaType.String | JsonSchemaType.Null;
        }

        return Task.CompletedTask;
    }
}
