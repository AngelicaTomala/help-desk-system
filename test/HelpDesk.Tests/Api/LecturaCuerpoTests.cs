using System.Net;
using System.Net.Http.Json;
using System.Text;
using HelpDesk.Api.Errores;
using HelpDesk.Application.Excepciones;
using HelpDesk.Tests.Api.Dobles;
using HelpDesk.Tests.Integracion;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Errores de lectura del cuerpo (decisión 93) y validación implícita de MVC desactivada (decisión 92).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class LecturaCuerpoTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private const string RutaCuerpo = "pruebas/cuerpo";

    private readonly HttpClient _cliente = factory.CrearClienteIdentificado();

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{")]
    [InlineData("{\"titulo\": }")]
    public async Task LeerCuerpo_AusenteNuloNoObjetoOMalFormado_Responde400ConClaveCuerpo(string json)
    {
        var respuesta = await _cliente.PostAsync(RutaCuerpo, Json(json));

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, ProblemasCuerpo.ClaveCuerpo);
    }

    [Theory]
    [InlineData("{\"titulo\": 123, \"categoria\": 3}", "titulo")]
    [InlineData("{\"titulo\": \"Monitor sin imagen\", \"categoria\": 3}", "categoria")]
    [InlineData("{\"Titulo\": true}", "titulo")]
    [InlineData("{\"tecnicoId\": \"8\"}", "tecnicoId")]
    public async Task LeerCuerpo_CampoConTipoIncorrecto_Responde400SoloConLaClaveDeEseCampo(string json, string clave)
    {
        var respuesta = await _cliente.PostAsync(RutaCuerpo, Json(json));

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
    }

    [Fact]
    public async Task LeerCuerpo_SinContentType_Responde400ConClaveCuerpo()
    {
        var contenido = new ByteArrayContent(Encoding.UTF8.GetBytes("{\"titulo\": \"Monitor sin imagen\"}"));

        var respuesta = await _cliente.PostAsync(RutaCuerpo, contenido);

        Assert.Null(contenido.Headers.ContentType);
        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, ProblemasCuerpo.ClaveCuerpo);
    }

    [Fact]
    public async Task LeerCuerpo_CampoNoNulableAusente_EjecutaLaAccionSinValidacionDeMvc()
    {
        var respuesta = await _cliente.PostAsync("pruebas/cuerpo-obligatorio", Json("{}"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var eco = await respuesta.Content.ReadFromJsonAsync<CuerpoNoNulable>();
        Assert.Null(eco!.Titulo);
    }

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");
}
