using System.Net.Mime;
using HelpDesk.Api.Errores;
using HelpDesk.Api.Identificadores;
using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Crear;
using HelpDesk.Application.Tickets.Listar;
using HelpDesk.Application.Tickets.Obtener;
using HelpDesk.Application.Tickets.Reclasificar;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Tickets;

/// <summary>
/// Operaciones de RF-01 y RF-02 (contrato §2.1 a §2.4). Solo traduce HTTP a comandos y consultas y llama a los
/// handlers, que aplican el orden de la decisión 92; el <c>401</c> lo responde antes el middleware de <c>X-Usuario-Id</c>.
/// </summary>
[ApiController]
[Route(Ruta)]
public sealed class TicketsController : ControllerBase
{
    public const string Ruta = "api/v1/tickets";

    // Respuestas documentadas (RNF-09): las de §2.1, el 415 de la decisión 94 y el 500 de §2.
    [HttpPost]
    [ProducesResponseType<TicketDetalleDto>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, CamposProblemDetails.TipoContenido)]
    public async Task<ActionResult<TicketDetalleDto>> Crear(
        CrearTicketCuerpo cuerpo,
        [FromServices] CrearTicketHandler handler,
        CancellationToken cancellationToken)
    {
        var detalle = await handler.ManejarAsync(
            new CrearTicketComando(cuerpo.Titulo, cuerpo.Descripcion, cuerpo.Categoria, cuerpo.Prioridad),
            cancellationToken);

        // Location relativo (decisión 64).
        return Created($"/{Ruta}/{detalle.Id}", detalle);
    }

    // Respuestas documentadas (RNF-09): las de §2.4, el 415 de la decisión 94 y el 500 de §2.
    [HttpPost("{id}/reclasificar")]
    [ProducesResponseType<TicketDetalleDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, CamposProblemDetails.TipoContenido)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, CamposProblemDetails.TipoContenido)]
    public async Task<ActionResult<TicketDetalleDto>> Reclasificar(
        string id,
        ReclasificarTicketCuerpo cuerpo,
        [FromServices] ReclasificarTicketHandler handler,
        CancellationToken cancellationToken)
    {
        var ticketId = IdRuta.LeerTicket(id);

        return await handler.ManejarAsync(new ReclasificarTicketComando(ticketId, cuerpo.Categoria), cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDetalleDto>> Obtener(
        string id,
        [FromServices] ObtenerTicketHandler handler,
        CancellationToken cancellationToken)
    {
        var ticketId = IdRuta.LeerTicket(id);

        return await handler.ManejarAsync(new ObtenerTicketConsulta(ticketId), cancellationToken);
    }

    // Los parámetros de la URL llegan como texto, sin convertir: los valida Application después del 403 (decisión 101).
    [HttpGet]
    public async Task<ActionResult<PaginaDto<TicketResumenDto>>> Listar(
        [FromServices] ListarTicketsHandler handler,
        CancellationToken cancellationToken) =>
        await handler.ManejarAsync(
            new ListarTicketsConsulta(
                Estado: ValoresParametro("estado"),
                Prioridad: ValoresParametro("prioridad"),
                TecnicoId: ValoresParametro("tecnicoId"),
                SoloVencidos: ValoresParametro("soloVencidos"),
                Page: ValoresParametro("page"),
                PageSize: ValoresParametro("pageSize")),
            cancellationToken);

    /// <summary>
    /// Todos los valores con que aparece el parámetro: <c>null</c> si falta, varios si está repetido y <c>""</c> si está
    /// vacío (decisiones 103 y 104). <c>Request.Query</c> no distingue mayúsculas en los nombres y los parámetros que
    /// no se leen aquí se ignoran (decisión 106).
    /// </summary>
    private string[]? ValoresParametro(string nombre) =>
        Request.Query.TryGetValue(nombre, out var valores)
            ? valores.Select(valor => valor ?? string.Empty).ToArray()
            : null;
}
