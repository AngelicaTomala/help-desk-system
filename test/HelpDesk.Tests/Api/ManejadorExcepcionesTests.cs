using System.Net;
using HelpDesk.Api.Errores;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Excepciones;
using HelpDesk.Tests.Api.Dobles;
using HelpDesk.Tests.Integracion;
using Microsoft.Extensions.Logging;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Traducción de las excepciones de Application y Domain a Problem Details (ADR-004, decisiones 40, 41 y 98).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class ManejadorExcepcionesTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private readonly HttpClient _cliente = factory.CrearClienteIdentificado();

    [Fact]
    public async Task ManejarExcepcion_SolicitudInvalidaConErrores_Responde400ConErroresEnCamelCase()
    {
        var respuesta = await _cliente.GetAsync("pruebas/validacion");

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, "titulo", "descripcion");
    }

    [Fact]
    public async Task ManejarExcepcion_SolicitudInvalidaSinErrores_Responde400SinErrors()
    {
        var respuesta = await _cliente.GetAsync("pruebas/tecnico-no-valido");

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.TecnicoNoValido);
        Assert.False(cuerpo.TryGetProperty("errors", out _));
    }

    [Theory]
    [InlineData(CodigosErrorAplicacion.RolNoAutorizado)]
    [InlineData(CodigosErrorAplicacion.TicketAjeno)]
    public async Task ManejarExcepcion_AccesoDenegado_Responde403ConSuCodigo(string codigo)
    {
        var respuesta = await _cliente.GetAsync($"pruebas/acceso-denegado/{codigo}");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, codigo);
    }

    [Fact]
    public async Task ManejarExcepcion_RecursoNoEncontrado_Responde404TicketNoEncontrado()
    {
        var respuesta = await _cliente.GetAsync("pruebas/no-encontrado");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.NotFound, CodigosErrorAplicacion.TicketNoEncontrado);
    }

    [Theory]
    [InlineData(CodigosError.TransicionInvalida)]
    [InlineData(CodigosError.TicketNoModificable)]
    public async Task ManejarExcepcion_TransicionInvalida_Responde409ConSuCodigo(string codigo)
    {
        var respuesta = await _cliente.GetAsync($"pruebas/transicion/{codigo}");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Conflict, codigo);
    }

    [Theory]
    [InlineData("pruebas/invariante")]
    [InlineData("pruebas/excepcion")]
    public async Task ManejarExcepcion_NoControlada_Responde500ErrorInternoSinDetallesInternos(string ruta)
    {
        var respuesta = await _cliente.GetAsync(ruta);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.InternalServerError, CodigosError.ErrorInterno);
        var texto = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain(PruebasController.MensajeInterno, texto);
        Assert.DoesNotContain("Exception", texto);
        Assert.DoesNotContain(" at ", texto);
    }

    [Fact]
    public async Task ManejarExcepcion_NoControlada_RegistraLaExcepcionEnElLog()
    {
        await _cliente.GetAsync("pruebas/excepcion");

        // La factory se comparte en la clase: otras pruebas también pueden haber registrado esta excepción.
        Assert.Contains(factory.Logs.Entradas, entrada =>
            entrada.Categoria == typeof(ManejadorExcepciones).FullName
            && entrada.Nivel == LogLevel.Error
            && entrada.Excepcion is InvalidOperationException { Message: PruebasController.MensajeInterno });
    }
}
