using System.Net;
using System.Text;
using System.Text.Json;
using HelpDesk.Tests.Integracion;

namespace HelpDesk.Tests.Api;

/// <summary>
/// JSON del contrato: camelCase, enums como texto, fechas UTC con <c>Z</c> y campos no previstos ignorados
/// (decisiones 47, 48, 60 y 66).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class SerializacionJsonTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private readonly HttpClient _cliente = factory.CrearClienteIdentificado();

    [Fact]
    public async Task Serializar_RespuestaConEnumYFechas_EnumComoTextoYFechasConZ()
    {
        var respuesta = await _cliente.GetAsync("pruebas/respuesta");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("application/json", respuesta.Content.Headers.ContentType?.MediaType);
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var cuerpo = documento.RootElement;

        Assert.Equal("EnProgreso", cuerpo.GetProperty("estado").GetString());
        Assert.Equal("2026-10-28T14:30:00Z", cuerpo.GetProperty("fechaUtc").GetString());
        Assert.Equal("2026-10-28T09:00:00Z", cuerpo.GetProperty("fechaSinTipo").GetString());
        Assert.Equal(JsonValueKind.Null, cuerpo.GetProperty("fechaNula").ValueKind);
    }

    [Fact]
    public async Task Deserializar_CampoNoPrevisto_LoIgnora()
    {
        var contenido = new StringContent(
            "{\"titulo\": \"Monitor sin imagen\", \"solicitanteId\": 5}",
            Encoding.UTF8,
            "application/json");

        var respuesta = await _cliente.PostAsync("pruebas/cuerpo", contenido);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal("Monitor sin imagen", documento.RootElement.GetProperty("titulo").GetString());
        Assert.False(documento.RootElement.TryGetProperty("solicitanteId", out _));
    }
}
