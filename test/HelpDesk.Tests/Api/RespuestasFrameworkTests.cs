using System.Net;
using System.Text;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Respuestas del framework con Problem Details, <c>traceId</c> y sin <c>codigo</c> (decisión 94, RNF-03).
/// </summary>
public class RespuestasFrameworkTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private readonly HttpClient _cliente = factory.CreateClient();

    [Fact]
    public async Task Enrutar_RutaInexistente_Responde404SinCodigo()
    {
        var respuesta = await _cliente.GetAsync("ruta-que-no-existe");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.NotFound, codigo: null);
    }

    [Fact]
    public async Task Enrutar_MetodoNoAdmitido_Responde405SinCodigo()
    {
        var respuesta = await _cliente.GetAsync("pruebas/cuerpo");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.MethodNotAllowed, codigo: null);
    }

    [Fact]
    public async Task LeerCuerpo_ContentTypeNoJson_Responde415SinCodigo()
    {
        var contenido = new StringContent("{\"titulo\": \"Monitor sin imagen\"}", Encoding.UTF8, "text/plain");

        var respuesta = await _cliente.PostAsync("pruebas/cuerpo", contenido);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.UnsupportedMediaType, codigo: null);
    }
}
