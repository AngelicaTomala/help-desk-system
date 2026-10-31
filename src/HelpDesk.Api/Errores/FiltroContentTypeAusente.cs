using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDesk.Api.Errores;

/// <summary>
/// Una petición sin <c>Content-Type</c> a una acción con cuerpo se trata como un cuerpo ausente: <c>400</c>
/// <c>cuerpo</c> (decisión 93). Se ejecuta antes del model binding, que respondería <c>415</c>; con un
/// <c>Content-Type</c> presente que no es JSON, el <c>415</c> se mantiene (decisión 94).
/// </summary>
public sealed class FiltroContentTypeAusente : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var tieneCuerpo = RespuestaModeloInvalido.ParametroCuerpo(context.ActionDescriptor) is not null;

        if (tieneCuerpo && string.IsNullOrWhiteSpace(context.HttpContext.Request.ContentType))
        {
            context.Result = ProblemasCuerpo.CuerpoIlegible(context.HttpContext);
            return;
        }

        await next();
    }
}
