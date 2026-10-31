using System.Net;
using System.Text.Json;
using HelpDesk.Api.Errores;
using HelpDesk.Api.Identidad;
using HelpDesk.Api.Tickets;
using HelpDesk.Application.Excepciones;
using HelpDesk.Tests.Integracion;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;
using static HelpDesk.Tests.Api.ElencoEnBase;

namespace HelpDesk.Tests.Api;

/// <summary>
/// <c>GET /api/v1/tickets/{id}</c> de extremo a extremo con la base de pruebas (contrato §2.2). Cada prueba guarda los
/// tickets 11 a 15 del elenco (§1.10) con el reloj en el instante de referencia; antes y después se borran los tickets
/// (decisión 99).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class ObtenerTicketEndpointTests(HelpDeskApiFactory factory, BaseDePruebasFixture baseDePruebas)
    : IClassFixture<HelpDeskApiFactory>, IAsyncLifetime
{
    private const string Ana = "1";
    private const string Bruno = "2";
    private const string Fabian = "6";
    private const string Gabriela = "7";
    private const string Supervisor = "10";

    private IReadOnlyDictionary<string, int> ids = new Dictionary<string, int>();

    public async Task InitializeAsync()
    {
        await baseDePruebas.ReiniciarTicketsAsync();
        ids = await ElencoEnBase.GuardarAsync(baseDePruebas);
    }

    public Task DisposeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    [Fact]
    [Trait("CA", "CA-02.1")]
    [Trait("RN", "RN-04")]
    [Trait("RN", "RN-11")]
    public async Task Obtener_EjemploDelContrato_Responde200ConDetalleYComentario()
    {
        // Contrato §2.2: Ana (1) consulta su ticket 14, resuelto.
        var respuesta = await ObtenerAsync(Ana, ids[Ticket14].ToString());

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("application/json", respuesta.Content.Headers.ContentType?.MediaType);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal(
            ["id", "codigo", "titulo", "descripcion", "categoria", "prioridad", "estado", "solicitante", "tecnico",
             "fechaCreacion", "fechaActualizacion", "fechaVencimiento", "fechaResolucion", "fechaCierre", "estadoSla",
             "comentarios"],
            ticket.EnumerateObject().Select(propiedad => propiedad.Name));
        Assert.Equal(ids[Ticket14], ticket.GetProperty("id").GetInt32());
        Assert.Equal(Ticket14, ticket.GetProperty("codigo").GetString());
        Assert.Equal("Restablecer contraseña de correo", ticket.GetProperty("titulo").GetString());
        Assert.Equal("No recuerdo la contraseña de mi cuenta de correo.", ticket.GetProperty("descripcion").GetString());
        Assert.Equal("Accesos", ticket.GetProperty("categoria").GetString());
        Assert.Equal("Media", ticket.GetProperty("prioridad").GetString());
        Assert.Equal("Resuelto", ticket.GetProperty("estado").GetString());
        AfirmarPersona(ticket.GetProperty("solicitante"), 1, "Ana Solicitante");
        AfirmarPersona(ticket.GetProperty("tecnico"), 6, "Fabián Técnico");
        Assert.Equal("2026-10-27T10:00:00Z", ticket.GetProperty("fechaCreacion").GetString());
        Assert.Equal("2026-10-27T16:00:00Z", ticket.GetProperty("fechaActualizacion").GetString());
        Assert.Equal("2026-10-27T18:00:00Z", ticket.GetProperty("fechaVencimiento").GetString());
        Assert.Equal("2026-10-27T16:00:00Z", ticket.GetProperty("fechaResolucion").GetString());
        Assert.Equal(JsonValueKind.Null, ticket.GetProperty("fechaCierre").ValueKind);
        // Resuelto a las 16:00 con vencimiento a las 18:00 (RN-10: Media = 8 h; RN-11).
        Assert.Equal("Cumplido", ticket.GetProperty("estadoSla").GetString());

        var comentario = Assert.Single(ticket.GetProperty("comentarios").EnumerateArray());
        Assert.Equal(["id", "tipo", "texto", "autor", "fechaCreacion"], comentario.EnumerateObject().Select(p => p.Name));
        Assert.True(comentario.GetProperty("id").GetInt32() > 0);
        Assert.Equal("Solucion", comentario.GetProperty("tipo").GetString());
        Assert.Equal("Se restableció la contraseña y se envió el enlace de acceso.", comentario.GetProperty("texto").GetString());
        AfirmarPersona(comentario.GetProperty("autor"), 6, "Fabián Técnico");
        Assert.Equal("2026-10-27T16:00:00Z", comentario.GetProperty("fechaCreacion").GetString());
    }

    [Theory]
    [Trait("CA", "CA-02.1")]
    [Trait("RN", "RN-04")]
    [InlineData(Fabian)]
    [InlineData(Supervisor)]
    public async Task Obtener_TecnicoAsignadoOSupervisor_Responde200(string usuario)
    {
        var respuesta = await ObtenerAsync(usuario, ids[Ticket12].ToString());

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal(Ticket12, ticket.GetProperty("codigo").GetString());
        Assert.Equal("Asignado", ticket.GetProperty("estado").GetString());
        AfirmarPersona(ticket.GetProperty("solicitante"), 2, "Bruno Solicitante");
        AfirmarPersona(ticket.GetProperty("tecnico"), 6, "Fabián Técnico");
    }

    [Fact]
    [Trait("CA", "CA-02.1")]
    [Trait("RN", "RN-04")]
    public async Task Obtener_TicketSinTecnico_TecnicoNullYSinComentarios()
    {
        var respuesta = await ObtenerAsync(Ana, ids[Ticket11].ToString());

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal("Abierto", ticket.GetProperty("estado").GetString());
        // Decisión 49.
        Assert.Equal(JsonValueKind.Null, ticket.GetProperty("tecnico").ValueKind);
        Assert.Equal(JsonValueKind.Null, ticket.GetProperty("fechaResolucion").ValueKind);
        Assert.Empty(ticket.GetProperty("comentarios").EnumerateArray());
    }

    [Theory]
    [Trait("CA", "CA-05.2")]
    [Trait("CA", "CA-05.3")]
    [Trait("RN", "RN-11")]
    [InlineData(Ticket11, "Vencido")]
    [InlineData(Ticket12, "EnTiempo")]
    [InlineData(Ticket14, "Cumplido")]
    [InlineData(Ticket15, "Incumplido")]
    public async Task Obtener_TicketsDelElenco_EstadoSlaSegunRN11(string codigo, string estadoSla)
    {
        // Instante de referencia 14:30 del 28/10 (§1.10).
        var respuesta = await ObtenerAsync(Supervisor, ids[codigo].ToString());

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(estadoSla, (await LeerJsonAsync(respuesta)).GetProperty("estadoSla").GetString());
    }

    [Theory]
    [Trait("CA", "CA-02.2")]
    [InlineData(Supervisor)]
    [InlineData(Ana)]
    public async Task Obtener_TicketInexistente_Responde404(string usuario)
    {
        // Para el solicitante también es 404, no 403: el 404 va antes (§1.4).
        var respuesta = await ObtenerAsync(usuario, int.MaxValue.ToString());

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.NotFound, CodigosErrorAplicacion.TicketNoEncontrado);
    }

    [Theory]
    [Trait("CA", "CA-02.2")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("012")]
    [InlineData("+12")]
    [InlineData("2147483648")]
    public async Task Obtener_IdNoValido_Responde404TicketNoEncontrado(string id)
    {
        // Decisiones 65 y 95.
        var respuesta = await ObtenerAsync(Supervisor, id);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.NotFound, CodigosErrorAplicacion.TicketNoEncontrado);
    }

    [Fact]
    [Trait("CA", "CA-02.6")]
    [Trait("RN", "RN-04")]
    public async Task Obtener_SolicitanteNoDueno_Responde403TicketAjeno()
    {
        // El ticket 11 es de Ana; Bruno (2) no lo ve.
        var respuesta = await ObtenerAsync(Bruno, ids[Ticket11].ToString());

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, CodigosErrorAplicacion.TicketAjeno);
    }

    [Theory]
    [Trait("CA", "CA-02.6")]
    [Trait("RN", "RN-04")]
    [InlineData(Gabriela, Ticket12)]
    [InlineData(Fabian, Ticket11)]
    public async Task Obtener_TecnicoNoAsignado_Responde403TicketAjeno(string tecnico, string codigo)
    {
        // El 12 está asignado a Fabián; el 11 no tiene técnico.
        var respuesta = await ObtenerAsync(tecnico, ids[codigo].ToString());

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, CodigosErrorAplicacion.TicketAjeno);
    }

    [Theory]
    [InlineData(null, CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("abc", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("0", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("-1", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("2147483648", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("11", CodigosErrorApi.UsuarioInexistente)]
    public async Task Obtener_EncabezadoAusenteInvalidoOUsuarioInexistente_Responde401(string? usuario, string codigo)
    {
        var respuesta = await ObtenerAsync(usuario, ids[Ticket14].ToString());

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, codigo);
    }

    [Fact]
    public async Task Obtener_IdNoValidoSinEncabezado_Responde401()
    {
        // Orden 401 → 404 (§1.4): el middleware responde antes de leer el {id}.
        var respuesta = await ObtenerAsync(null, "abc");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioNoIdentificado);
    }

    private async Task<HttpResponseMessage> ObtenerAsync(string? usuario, string id)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Get, $"{TicketsController.Ruta}/{id}");
        if (usuario is not null)
        {
            // Sin validación del cliente: el valor llega tal cual.
            Assert.True(peticion.Headers.TryAddWithoutValidation(MiddlewareUsuarioActual.Encabezado, usuario));
        }

        return await factory.CreateClient().SendAsync(peticion);
    }

    private static void AfirmarPersona(JsonElement persona, int id, string nombre)
    {
        Assert.Equal(["id", "nombre"], persona.EnumerateObject().Select(propiedad => propiedad.Name));
        Assert.Equal(id, persona.GetProperty("id").GetInt32());
        Assert.Equal(nombre, persona.GetProperty("nombre").GetString());
    }

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage respuesta)
    {
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        return documento.RootElement.Clone();
    }
}
