using HelpDesk.Api.Identificadores;
using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Crear;
using HelpDesk.Application.Tickets.Reclasificar;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Tickets;

/// <summary>
/// Operaciones de RF-01 (contrato §2.1 y §2.4). Solo traduce HTTP a comandos y llama a los handlers, que aplican el
/// orden de la decisión 92; el <c>401</c> lo responde antes el middleware de <c>X-Usuario-Id</c>.
/// </summary>
[ApiController]
[Route(Ruta)]
public sealed class TicketsController : ControllerBase
{
    public const string Ruta = "api/v1/tickets";

    [HttpPost]
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

    [HttpPost("{id}/reclasificar")]
    public async Task<ActionResult<TicketDetalleDto>> Reclasificar(
        string id,
        ReclasificarTicketCuerpo cuerpo,
        [FromServices] ReclasificarTicketHandler handler,
        CancellationToken cancellationToken)
    {
        var ticketId = IdRuta.LeerTicket(id);

        return await handler.ManejarAsync(new ReclasificarTicketComando(ticketId, cuerpo.Categoria), cancellationToken);
    }
}
