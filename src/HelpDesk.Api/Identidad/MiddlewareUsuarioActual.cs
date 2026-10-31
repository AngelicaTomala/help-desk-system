using HelpDesk.Api.Errores;
using HelpDesk.Api.Identificadores;
using HelpDesk.Application.Abstracciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace HelpDesk.Api.Identidad;

/// <summary>
/// Valida <c>X-Usuario-Id</c> antes de la acción y fija el usuario en <see cref="UsuarioActual"/> (RNF-01, decisiones
/// 28, 95 y 97). Solo actúa sobre acciones de controlador: una ruta inexistente, un método no admitido (decisión 94) y
/// el documento OpenAPI (decisión 52) pasan sin el encabezado. Un usuario inactivo no es un <c>401</c>.
/// </summary>
public sealed class MiddlewareUsuarioActual(RequestDelegate next)
{
    public const string Encabezado = "X-Usuario-Id";

    public async Task InvokeAsync(
        HttpContext httpContext,
        IUsuarioRepositorio usuarios,
        UsuarioActual usuarioActual,
        IProblemDetailsService problemDetailsService)
    {
        if (httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>() is null)
        {
            await next(httpContext);
            return;
        }

        // Un encabezado repetido no es válido (decisión 95).
        var valores = httpContext.Request.Headers[Encabezado];
        if (valores.Count != 1 || !FormatoId.TryLeer(valores[0], out var id))
        {
            await ResponderNoAutorizadoAsync(httpContext, problemDetailsService, CodigosErrorApi.UsuarioNoIdentificado);
            return;
        }

        var usuario = await usuarios.ObtenerPorIdAsync(id, httpContext.RequestAborted);
        if (usuario is null)
        {
            await ResponderNoAutorizadoAsync(httpContext, problemDetailsService, CodigosErrorApi.UsuarioInexistente);
            return;
        }

        usuarioActual.Fijar(usuario.Id, usuario.Rol);

        await next(httpContext);
    }

    private static async Task ResponderNoAutorizadoAsync(
        HttpContext httpContext,
        IProblemDetailsService problemDetailsService,
        string codigo)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        var problema = new ProblemDetails { Status = StatusCodes.Status401Unauthorized };
        problema.Extensions[CamposProblemDetails.Codigo] = codigo;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
        });
    }
}

public static class MiddlewareUsuarioActualExtensions
{
    /// <summary>
    /// Va después de <c>UseRouting</c>, que elige el endpoint, y antes de la acción (orden <c>401</c> → <c>404</c>, §1.4).
    /// </summary>
    public static IApplicationBuilder UseUsuarioActual(this IApplicationBuilder app) =>
        app.UseMiddleware<MiddlewareUsuarioActual>();
}
