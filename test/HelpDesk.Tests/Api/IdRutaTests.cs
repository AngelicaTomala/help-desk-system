using System.Net;
using System.Net.Http.Json;
using HelpDesk.Api.Errores;
using HelpDesk.Application.Excepciones;
using HelpDesk.Tests.Api.Dobles;
using HelpDesk.Tests.Integracion;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Lectura del <c>{id}</c> de la ruta (decisiones 65 y 95) y orden <c>401</c> → <c>404</c> (§1.4, RNF-04).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class IdRutaTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private const string RutaTickets = "pruebas/tickets/";

    private readonly HelpDeskApiFactory _factory = factory;

    [Fact]
    public async Task LeerTicket_IdValido_LlegaAlEndpoint()
    {
        var respuesta = await _factory.CrearClienteIdentificado().GetAsync(RutaTickets + "12");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(new IdDePrueba(12), await respuesta.Content.ReadFromJsonAsync<IdDePrueba>());
    }

    [Theory]
    [InlineData("012")]
    [InlineData("+12")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("2147483648")]
    public async Task LeerTicket_IdInvalido_Responde404TicketNoEncontrado(string id)
    {
        var respuesta = await _factory.CrearClienteIdentificado().GetAsync(RutaTickets + id);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.NotFound, CodigosErrorAplicacion.TicketNoEncontrado);
    }

    [Fact]
    public async Task LeerTicket_IdInvalidoSinEncabezado_Responde401()
    {
        var respuesta = await _factory.CreateClient().GetAsync(RutaTickets + "abc");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioNoIdentificado);
    }
}
