using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Errores;

/// <summary>
/// Traduce las excepciones a Problem Details con el <c>codigo</c> del catálogo (ADR-004, decisiones 40 y 98).
/// Cualquier otra excepción, incluida <see cref="InvarianteVioladaException"/>, responde <c>500</c>
/// <c>ERROR_INTERNO</c> sin detalles internos y se registra en el log (decisión 41).
/// </summary>
public sealed class ManejadorExcepciones(
    IProblemDetailsService problemDetailsService,
    ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problema = Traducir(exception);

        // En .NET 10 el middleware no registra las excepciones que maneja un IExceptionHandler.
        if (problema.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Error no controlado en {Metodo} {Ruta}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = problema.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
        });
    }

    private static ProblemDetails Traducir(Exception exception) => exception switch
    {
        SolicitudInvalidaException { Errores.Count: > 0 } solicitud => ConCodigo(
            new HttpValidationProblemDetails(solicitud.Errores.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
            },
            solicitud.Codigo),
        SolicitudInvalidaException solicitud => Problema(StatusCodes.Status400BadRequest, solicitud.Codigo),
        AccesoDenegadoException acceso => Problema(StatusCodes.Status403Forbidden, acceso.Codigo),
        RecursoNoEncontradoException recurso => Problema(StatusCodes.Status404NotFound, recurso.Codigo),
        TransicionInvalidaException transicion => Problema(StatusCodes.Status409Conflict, transicion.Codigo),
        _ => Problema(StatusCodes.Status500InternalServerError, CodigosError.ErrorInterno),
    };

    private static ProblemDetails Problema(int status, string codigo) =>
        ConCodigo(new ProblemDetails { Status = status }, codigo);

    private static ProblemDetails ConCodigo(ProblemDetails problema, string codigo)
    {
        problema.Extensions[CamposProblemDetails.Codigo] = codigo;
        return problema;
    }
}
