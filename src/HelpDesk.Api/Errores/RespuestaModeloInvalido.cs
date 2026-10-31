using System.Text.Json;
using HelpDesk.Application.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HelpDesk.Api.Errores;

/// <summary>
/// Respuesta de <c>[ApiController]</c> cuando el model binding falla (decisión 93). Un error de tipo en un campo del
/// cuerpo responde solo con la clave de ese campo; cualquier otro error del cuerpo (ausente, vacío, <c>null</c>, que
/// no es un objeto o mal formado), con la clave <c>cuerpo</c>.
/// </summary>
/// <remarks>
/// Requiere <c>AllowInputFormatterExceptionMessages = false</c>: así <see cref="ModelError.Exception"/> conserva la
/// <see cref="JsonException"/>. Si su <c>InnerException</c> es otra <see cref="JsonException"/>, el error es del
/// lector (JSON mal formado); si no, es de conversión de tipo y <c>Path</c> indica el campo.
/// </remarks>
public static class RespuestaModeloInvalido
{
    private const string PrefijoRutaJson = "$";

    public static IActionResult Crear(ActionContext context)
    {
        var parametroCuerpo = ParametroCuerpo(context.ActionDescriptor);
        var conErrores = context.ModelState.Where(entrada => entrada.Value?.Errors.Count > 0).ToList();

        var campoConTipoIncorrecto = conErrores
            .SelectMany(entrada => entrada.Value!.Errors)
            .Select(error => CampoConTipoIncorrecto(error, parametroCuerpo))
            .FirstOrDefault(campo => campo is not null);

        if (campoConTipoIncorrecto is not null)
        {
            return ProblemasCuerpo.TipoIncorrecto(context.HttpContext, campoConTipoIncorrecto);
        }

        if (parametroCuerpo is not null && conErrores.Any(entrada => EsErrorDelCuerpo(entrada.Key, parametroCuerpo)))
        {
            return ProblemasCuerpo.CuerpoIlegible(context.HttpContext);
        }

        // Errores que no son del cuerpo (ruta o query): claves en camelCase (decisión 61).
        var modelState = new ModelStateDictionary();
        foreach (var (clave, entrada) in conErrores)
        {
            foreach (var error in entrada!.Errors)
            {
                modelState.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(clave), error.ErrorMessage);
            }
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problema = factory.CreateValidationProblemDetails(context.HttpContext, modelState, StatusCodes.Status400BadRequest);
        problema.Extensions[CamposProblemDetails.Codigo] = CodigosErrorAplicacion.ValidacionFallida;

        return new BadRequestObjectResult(problema) { ContentTypes = { CamposProblemDetails.TipoContenido } };
    }

    internal static ParameterDescriptor? ParametroCuerpo(ActionDescriptor accion) =>
        accion.Parameters.FirstOrDefault(parametro => parametro.BindingInfo?.BindingSource == BindingSource.Body);

    private static bool EsErrorDelCuerpo(string clave, ParameterDescriptor parametroCuerpo) =>
        clave.Length == 0
        || clave.StartsWith(PrefijoRutaJson, StringComparison.Ordinal)
        || string.Equals(clave, parametroCuerpo.Name, StringComparison.OrdinalIgnoreCase);

    private static string? CampoConTipoIncorrecto(ModelError error, ParameterDescriptor? parametroCuerpo)
    {
        if (parametroCuerpo is null
            || error.Exception is not JsonException excepcion
            || excepcion.InnerException is JsonException)
        {
            return null;
        }

        return CampoDeLaRuta(excepcion.Path, parametroCuerpo.ParameterType);
    }

    // "$.titulo" → "titulo", si es una propiedad del cuerpo; "$" o "$[0]" no indican un campo.
    private static string? CampoDeLaRuta(string? ruta, Type tipoCuerpo)
    {
        if (ruta is null || !ruta.StartsWith(PrefijoRutaJson + ".", StringComparison.Ordinal))
        {
            return null;
        }

        var segmento = ruta[2..];
        var fin = segmento.IndexOfAny(['.', '[']);
        if (fin >= 0)
        {
            segmento = segmento[..fin];
        }

        var propiedad = tipoCuerpo.GetProperties()
            .FirstOrDefault(candidata => string.Equals(candidata.Name, segmento, StringComparison.OrdinalIgnoreCase));

        return propiedad is null ? null : JsonNamingPolicy.CamelCase.ConvertName(propiedad.Name);
    }
}
