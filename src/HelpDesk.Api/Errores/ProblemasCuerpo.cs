using HelpDesk.Application.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HelpDesk.Api.Errores;

/// <summary>
/// <c>400</c> <c>VALIDACION_FALLIDA</c> por un cuerpo que no se puede leer (decisión 93): la clave <c>cuerpo</c> o la
/// del campo con tipo incorrecto. <see cref="ProblemDetailsFactory"/> agrega <c>traceId</c>.
/// </summary>
public static class ProblemasCuerpo
{
    public const string ClaveCuerpo = "cuerpo";
    public const string MensajeCuerpo = "El cuerpo de la petición es obligatorio y debe ser un objeto JSON válido.";
    public const string MensajeTipo = "El valor no tiene el tipo esperado.";

    public static IActionResult CuerpoIlegible(HttpContext httpContext) =>
        Crear(httpContext, ClaveCuerpo, MensajeCuerpo);

    public static IActionResult TipoIncorrecto(HttpContext httpContext, string campo) =>
        Crear(httpContext, campo, MensajeTipo);

    private static IActionResult Crear(HttpContext httpContext, string clave, string mensaje)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(clave, mensaje);

        var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problema = factory.CreateValidationProblemDetails(httpContext, modelState, StatusCodes.Status400BadRequest);
        problema.Extensions[CamposProblemDetails.Codigo] = CodigosErrorAplicacion.ValidacionFallida;

        return new BadRequestObjectResult(problema) { ContentTypes = { CamposProblemDetails.TipoContenido } };
    }
}
