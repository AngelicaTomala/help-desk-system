using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDesk.Api.Configuracion;
using HelpDesk.Api.Errores;
using HelpDesk.Api.Identidad;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Api.Dobles;
using HelpDesk.Tests.Integracion;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Middleware de <c>X-Usuario-Id</c> con los usuarios semilla de la base de pruebas, sin modificarlos (RNF-01,
/// decisiones 28, 52, 89, 95, 97 y 99).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class UsuarioActualMiddlewareTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private const string RutaUsuarioActual = "pruebas/usuario-actual";

    // Las del contrato: el rol llega como texto (decisión 48).
    private static readonly JsonSerializerOptions OpcionesJson = CrearOpcionesJson();

    private readonly HttpClient _cliente = factory.CreateClient();

    [Theory]
    [InlineData("1", 1, Rol.Solicitante)]
    [InlineData("6", 6, Rol.Tecnico)]
    [InlineData("10", 10, Rol.Supervisor)]
    public async Task LeerUsuario_UsuarioExistente_FijaIdYRol(string encabezado, int id, Rol rol)
    {
        var respuesta = await _cliente.SendAsync(Peticion(encabezado));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(new UsuarioDePrueba(id, rol), await respuesta.Content.ReadFromJsonAsync<UsuarioDePrueba>(OpcionesJson));
    }

    [Fact]
    public async Task LeerUsuario_TecnicoInactivo_FijaIdYRol()
    {
        // Irene (9) está inactiva: no puede recibir tickets (RN-06), pero se identifica igual.
        var respuesta = await _cliente.SendAsync(Peticion("9"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(new UsuarioDePrueba(9, Rol.Tecnico), await respuesta.Content.ReadFromJsonAsync<UsuarioDePrueba>(OpcionesJson));
    }

    [Fact]
    public async Task LeerUsuario_EncabezadoAusente_Responde401UsuarioNoIdentificado()
    {
        var respuesta = await _cliente.GetAsync(RutaUsuarioActual);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioNoIdentificado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData(" 1")]
    [InlineData("01")]
    [InlineData("1,2")]
    [InlineData("2147483648")]
    public async Task LeerUsuario_EncabezadoInvalido_Responde401UsuarioNoIdentificado(string encabezado)
    {
        var respuesta = await _cliente.SendAsync(Peticion(encabezado));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioNoIdentificado);
    }

    [Fact]
    public async Task LeerUsuario_EncabezadoRepetido_Responde401UsuarioNoIdentificado()
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Get, RutaUsuarioActual);
        peticion.Headers.Add(MiddlewareUsuarioActual.Encabezado, ["1", "2"]);

        var respuesta = await _cliente.SendAsync(peticion);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioNoIdentificado);
    }

    [Fact]
    public async Task LeerUsuario_UsuarioInexistente_Responde401UsuarioInexistente()
    {
        var respuesta = await _cliente.SendAsync(Peticion("11"));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioInexistente);
    }

    [Theory]
    [InlineData("swagger/index.html")]
    [InlineData("openapi/v1.json")]
    public async Task LeerUsuario_SwaggerYOpenApiSinEncabezado_Responde200(string ruta)
    {
        var respuesta = await _cliente.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    private static JsonSerializerOptions CrearOpcionesJson()
    {
        var opciones = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        ConfiguracionJson.Aplicar(opciones);
        return opciones;
    }

    // Sin validación del cliente: el valor llega tal cual, también vacío o con espacios.
    private static HttpRequestMessage Peticion(string encabezado)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Get, RutaUsuarioActual);
        Assert.True(peticion.Headers.TryAddWithoutValidation(MiddlewareUsuarioActual.Encabezado, encabezado));
        return peticion;
    }
}
