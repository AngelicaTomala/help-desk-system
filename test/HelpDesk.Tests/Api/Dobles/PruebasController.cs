using FluentValidation.Results;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Tests.Api.Dobles;

/// <summary>
/// Endpoints solo de pruebas: lanzan cada tipo de excepción o reciben un cuerpo JSON. La factory los agrega con
/// <c>AddApplicationPart</c>; no existen en la Api compilada.
/// </summary>
[ApiController]
[Route(Ruta)]
public sealed class PruebasController : ControllerBase
{
    public const string Ruta = "pruebas";
    public const string MensajeInterno = "Detalle interno que no debe salir de la Api";

    [HttpGet("validacion")]
    public IActionResult Validacion() =>
        throw SolicitudInvalidaException.DesdeValidacion(new ValidationResult(
        [
            new ValidationFailure("Titulo", "El título debe tener entre 5 y 150 caracteres."),
            new ValidationFailure("Descripcion", "La descripción es obligatoria."),
        ]));

    [HttpGet("tecnico-no-valido")]
    public IActionResult TecnicoNoValido() =>
        throw new SolicitudInvalidaException(CodigosErrorAplicacion.TecnicoNoValido, "El técnico no es válido.");

    [HttpGet("acceso-denegado/{codigo}")]
    public IActionResult AccesoDenegado(string codigo) =>
        throw new AccesoDenegadoException(codigo, "Acceso denegado.");

    [HttpGet("no-encontrado")]
    public IActionResult NoEncontrado() =>
        throw new RecursoNoEncontradoException("No existe el ticket.");

    [HttpGet("transicion/{codigo}")]
    public IActionResult Transicion(string codigo) =>
        throw new TransicionInvalidaException(codigo, "Transición no permitida.");

    [HttpGet("invariante")]
    public IActionResult Invariante() =>
        throw new InvarianteVioladaException(MensajeInterno);

    [HttpGet("excepcion")]
    public IActionResult Excepcion() =>
        throw new InvalidOperationException(MensajeInterno);

    [HttpPost("cuerpo")]
    public ActionResult<CuerpoDePrueba> Cuerpo(CuerpoDePrueba cuerpo) => cuerpo;

    [HttpPost("cuerpo-obligatorio")]
    public ActionResult<CuerpoNoNulable> CuerpoObligatorio(CuerpoNoNulable cuerpo) => cuerpo;

    [HttpGet("respuesta")]
    public ActionResult<RespuestaDePrueba> Respuesta() => new RespuestaDePrueba(
        EstadoTicket.EnProgreso,
        new DateTime(2026, 10, 28, 14, 30, 0, DateTimeKind.Utc),
        new DateTime(2026, 10, 28, 9, 0, 0, DateTimeKind.Unspecified),
        null);
}

public sealed record CuerpoDePrueba(string? Titulo, string? Categoria, int? TecnicoId);

// Titulo sin "?": MVC no debe exigirlo (SuppressImplicitRequiredAttributeForNonNullableReferenceTypes).
public sealed record CuerpoNoNulable(string Titulo);

public sealed record RespuestaDePrueba(EstadoTicket Estado, DateTime FechaUtc, DateTime FechaSinTipo, DateTime? FechaNula);
