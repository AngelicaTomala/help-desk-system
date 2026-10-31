using FluentValidation;
using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets.Crear;

/// <summary>
/// Crea un ticket (RF-01). Orden de la decisión 92: permiso (<c>403</c>), validador (<c>400</c>) y Domain;
/// crear no tiene recurso de ruta (<c>404</c>).
/// </summary>
public class CrearTicketHandler(
    IUsuarioActual usuarioActual,
    IValidator<CrearTicketComando> validador,
    IGeneradorCodigoTicket generadorCodigo,
    ITicketRepositorio tickets,
    IUnidadDeTrabajo unidadDeTrabajo,
    IConsultasTicket consultas,
    TimeProvider timeProvider)
{
    public async Task<TicketDetalleDto> ManejarAsync(CrearTicketComando comando, CancellationToken cancellationToken)
    {
        if (usuarioActual.Rol != Rol.Solicitante)
        {
            throw new AccesoDenegadoException(
                CodigosErrorAplicacion.RolNoAutorizado,
                "Solo un solicitante puede crear tickets.");
        }

        // Recorta antes de validar y pasa a Domain el texto recortado (decisiones 31, 79 y 83).
        var recortado = comando with { Titulo = comando.Titulo?.Trim(), Descripcion = comando.Descripcion?.Trim() };

        var resultado = await validador.ValidateAsync(recortado, cancellationToken);
        if (!resultado.IsValid)
        {
            throw SolicitudInvalidaException.DesdeValidacion(resultado);
        }

        ValoresEnum.TryParse<Categoria>(recortado.Categoria, out var categoria);
        ValoresEnum.TryParse<Prioridad>(recortado.Prioridad, out var prioridad);

        // Una sola lectura del reloj por operación (RNF-08, decisión 76).
        var ahora = timeProvider.GetUtcNow().UtcDateTime;

        // Si la SEQUENCE se agota, la excepción se propaga sin crear el ticket (decisión 90).
        var codigo = await generadorCodigo.GenerarAsync(cancellationToken);

        var ticket = Ticket.Crear(
            codigo,
            recortado.Titulo!,
            recortado.Descripcion!,
            categoria,
            prioridad,
            usuarioActual.Id,
            ahora);

        tickets.Agregar(ticket);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return await consultas.ObtenerDetalleAsync(ticket.Id, ahora, cancellationToken)
            ?? throw new InvalidOperationException($"No se encontró el ticket {ticket.Id} recién creado.");
    }
}
