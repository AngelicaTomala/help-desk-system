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
/// <c>GET /api/v1/tickets</c> de extremo a extremo con la base de pruebas (contrato §1.5, §1.11 y §2.3). Cada prueba
/// guarda los tickets 11 a 15 del elenco (§1.10) con el reloj en el instante de referencia; antes y después se borran
/// los tickets (decisión 99). Sin filtros, el orden es 12, 13, 11, 14 y 15: el 13 y el 11 se crearon a las 08:00 y el
/// 13 tiene el Id mayor (decisión 59).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class ListarTicketsEndpointTests(HelpDeskApiFactory factory, BaseDePruebasFixture baseDePruebas)
    : IClassFixture<HelpDeskApiFactory>, IAsyncLifetime
{
    private const string Ana = "1";
    private const string Fabian = "6";
    private const string Gabriela = "7";
    private const string Supervisor = "10";

    private static readonly string[] Todos = [Ticket12, Ticket13, Ticket11, Ticket14, Ticket15];

    private IReadOnlyDictionary<string, int> ids = new Dictionary<string, int>();

    public async Task InitializeAsync()
    {
        await baseDePruebas.ReiniciarTicketsAsync();
        ids = await ElencoEnBase.GuardarAsync(baseDePruebas);
    }

    public Task DisposeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    [Theory]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-04")]
    [InlineData("Abierto", Ticket11)]
    [InlineData("enprogreso", Ticket13)]
    public async Task Listar_FiltroEstado_DevuelveSoloEseEstado(string estado, string codigo)
    {
        // Decisión 58: el enum no distingue mayúsculas.
        var pagina = await ListarOkAsync(Supervisor, $"?estado={estado}");

        Assert.Equal([codigo], Codigos(pagina));
        Assert.Equal(1, pagina.GetProperty("totalItems").GetInt32());
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    public async Task Listar_FiltroPrioridad_DevuelveSoloEsaPrioridad()
    {
        var pagina = await ListarOkAsync(Supervisor, "?prioridad=Media");

        Assert.Equal([Ticket12, Ticket14], Codigos(pagina));
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-04")]
    public async Task Listar_FiltroTecnicoId_DevuelveSoloLosDeEseTecnico()
    {
        var pagina = await ListarOkAsync(Supervisor, "?tecnicoId=6");

        Assert.Equal([Ticket12, Ticket14, Ticket15], Codigos(pagina));
    }

    [Theory]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-11")]
    [InlineData("true")]
    [InlineData("TRUE")]
    public async Task Listar_SoloVencidosTrue_DevuelveSoloVencidos(string valor)
    {
        // El 15 está Incumplido y no se incluye (CA-02.3, decisión 102).
        var pagina = await ListarOkAsync(Supervisor, $"?soloVencidos={valor}");

        Assert.Equal([Ticket13, Ticket11], Codigos(pagina));
        Assert.All(Items(pagina), item => Assert.Equal("Vencido", item.GetProperty("estadoSla").GetString()));
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-11")]
    public async Task Listar_SoloVencidosFalse_EquivaleANoEnviarlo()
    {
        var conFalse = await ListarOkAsync(Supervisor, "?soloVencidos=false");
        var sinParametro = await ListarOkAsync(Supervisor, "");

        Assert.Equal(Todos, Codigos(conFalse));
        Assert.Equal(sinParametro.GetRawText(), conFalse.GetRawText());
    }

    [Fact]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-04")]
    [Trait("RN", "RN-11")]
    public async Task Listar_FiltrosCombinados_DevuelveLosQueCumplenTodos()
    {
        var pagina = await ListarOkAsync(Supervisor, "?estado=EnProgreso&prioridad=Alta&tecnicoId=7&soloVencidos=true");

        Assert.Equal([Ticket13], Codigos(pagina));
    }

    [Theory]
    [Trait("CA", "CA-02.3")]
    [Trait("RN", "RN-04")]
    [InlineData("1")]
    [InlineData("999")]
    public async Task Listar_SupervisorTecnicoIdQueNoEsTecnico_Responde200Vacio(string tecnicoId)
    {
        // Decisión 69: Ana (1) es solicitante y 999 no existe.
        var pagina = await ListarOkAsync(Supervisor, $"?tecnicoId={tecnicoId}");

        Assert.Empty(Items(pagina));
        Assert.Equal(0, pagina.GetProperty("totalItems").GetInt32());
    }

    [Fact]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    public async Task Listar_SinParametros_Responde200PaginadoConValoresPorDefecto()
    {
        var pagina = await ListarOkAsync(Supervisor, "");

        Assert.Equal(["items", "page", "pageSize", "totalItems"], pagina.EnumerateObject().Select(p => p.Name));
        Assert.Equal(1, pagina.GetProperty("page").GetInt32());
        Assert.Equal(20, pagina.GetProperty("pageSize").GetInt32());
        Assert.Equal(5, pagina.GetProperty("totalItems").GetInt32());
        Assert.Equal(Todos, Codigos(pagina));
        // Resumen de la decisión 21.
        Assert.All(Items(pagina), item => Assert.Equal(
            ["id", "codigo", "titulo", "estado", "prioridad", "tecnico", "fechaVencimiento", "estadoSla"],
            item.EnumerateObject().Select(propiedad => propiedad.Name)));
    }

    [Fact]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    public async Task Listar_SegundaPagina_DevuelveElRestoYElTotal()
    {
        var pagina = await ListarOkAsync(Supervisor, "?page=2&pageSize=2");

        Assert.Equal(2, pagina.GetProperty("page").GetInt32());
        Assert.Equal(2, pagina.GetProperty("pageSize").GetInt32());
        Assert.Equal(5, pagina.GetProperty("totalItems").GetInt32());
        Assert.Equal([Ticket11, Ticket14], Codigos(pagina));
    }

    [Theory]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    [InlineData("?page=1&pageSize=1", 1, 1)]
    [InlineData("?pageSize=100", 1, 100)]
    public async Task Listar_PaginacionEnLosLimites_Responde200(string query, int page, int pageSize)
    {
        var pagina = await ListarOkAsync(Supervisor, query);

        Assert.Equal(page, pagina.GetProperty("page").GetInt32());
        Assert.Equal(pageSize, pagina.GetProperty("pageSize").GetInt32());
        Assert.Equal(Math.Min(pageSize, 5), Items(pagina).Count);
    }

    [Theory]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    [InlineData("?page=0", "page")]
    [InlineData("?page=02", "page")]
    [InlineData("?pageSize=0", "pageSize")]
    [InlineData("?pageSize=101", "pageSize")]
    public async Task Listar_PaginacionFueraDeRango_Responde400ConSuClave(string query, string clave)
    {
        // Decisiones 9 y 105.
        var respuesta = await ListarAsync(Supervisor, query);

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
    }

    [Fact]
    [Trait("CA", "CA-02.5")]
    [Trait("RN", "RN-04")]
    [Trait("RN", "RN-11")]
    public async Task Listar_EjemploDelContrato_SolicitanteVeSoloLosSuyos()
    {
        // Contrato §2.3: Ana (1) solo ve el 11 (Vencido) y el 14 (Cumplido).
        var pagina = await ListarOkAsync(Ana, "?page=1&pageSize=20");

        Assert.Equal(1, pagina.GetProperty("page").GetInt32());
        Assert.Equal(20, pagina.GetProperty("pageSize").GetInt32());
        Assert.Equal(2, pagina.GetProperty("totalItems").GetInt32());

        var items = Items(pagina);
        Assert.Equal(2, items.Count);

        var ticket11 = items[0];
        Assert.Equal(ids[Ticket11], ticket11.GetProperty("id").GetInt32());
        Assert.Equal(Ticket11, ticket11.GetProperty("codigo").GetString());
        Assert.Equal("No abre el sistema de nómina", ticket11.GetProperty("titulo").GetString());
        Assert.Equal("Abierto", ticket11.GetProperty("estado").GetString());
        Assert.Equal("Critica", ticket11.GetProperty("prioridad").GetString());
        Assert.Equal(JsonValueKind.Null, ticket11.GetProperty("tecnico").ValueKind);
        Assert.Equal("2026-10-28T10:00:00Z", ticket11.GetProperty("fechaVencimiento").GetString());
        Assert.Equal("Vencido", ticket11.GetProperty("estadoSla").GetString());

        var ticket14 = items[1];
        Assert.Equal(ids[Ticket14], ticket14.GetProperty("id").GetInt32());
        Assert.Equal(Ticket14, ticket14.GetProperty("codigo").GetString());
        Assert.Equal("Restablecer contraseña de correo", ticket14.GetProperty("titulo").GetString());
        Assert.Equal("Resuelto", ticket14.GetProperty("estado").GetString());
        Assert.Equal("Media", ticket14.GetProperty("prioridad").GetString());
        Assert.Equal(6, ticket14.GetProperty("tecnico").GetProperty("id").GetInt32());
        Assert.Equal("Fabián Técnico", ticket14.GetProperty("tecnico").GetProperty("nombre").GetString());
        Assert.Equal("2026-10-27T18:00:00Z", ticket14.GetProperty("fechaVencimiento").GetString());
        Assert.Equal("Cumplido", ticket14.GetProperty("estadoSla").GetString());
    }

    [Theory]
    [Trait("CA", "CA-02.5")]
    [Trait("RN", "RN-04")]
    [InlineData(Fabian, new[] { Ticket12, Ticket14, Ticket15 })]
    [InlineData(Gabriela, new[] { Ticket13 })]
    public async Task Listar_Tecnico_VeSoloLosAsignados(string tecnico, string[] codigos)
    {
        var pagina = await ListarOkAsync(tecnico, "");

        Assert.Equal(codigos, Codigos(pagina));
        Assert.Equal(codigos.Length, pagina.GetProperty("totalItems").GetInt32());
    }

    [Fact]
    [Trait("CA", "CA-02.5")]
    [Trait("RN", "RN-04")]
    public async Task Listar_Supervisor_VeTodos()
    {
        var pagina = await ListarOkAsync(Supervisor, "");

        Assert.Equal(Todos, Codigos(pagina));
    }

    [Fact]
    [Trait("CA", "CA-02.5")]
    [Trait("RN", "RN-04")]
    public async Task Listar_SolicitanteConTecnicoId_FiltraDentroDeSuVisibilidad()
    {
        // Decisión 51: de los tickets de Fabián, Ana solo ve el 14.
        var pagina = await ListarOkAsync(Ana, "?tecnicoId=6");

        Assert.Equal([Ticket14], Codigos(pagina));
    }

    [Theory]
    [Trait("CA", "CA-02.7")]
    [Trait("RN", "RN-04")]
    [InlineData("?tecnicoId=7")]
    [InlineData("?tecnicoId=abc")]
    [InlineData("?tecnicoId=")]
    [InlineData("?tecnicoId=06")]
    [InlineData("?tecnicoId=6&tecnicoId=7")]
    [InlineData("?tecnicoId=7&page=0")]
    public async Task Listar_TecnicoConTecnicoIdAjeno_Responde403FiltroNoAutorizado(string query)
    {
        // Decisión 100: el 403 va antes que el 400 del validador.
        var respuesta = await ListarAsync(Fabian, query);

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, CodigosErrorAplicacion.FiltroNoAutorizado);
    }

    [Fact]
    [Trait("CA", "CA-02.7")]
    [Trait("RN", "RN-04")]
    public async Task Listar_TecnicoConSuPropioIdRepetido_Responde400()
    {
        // Decisiones 100 y 103.
        var respuesta = await ListarAsync(Fabian, "?tecnicoId=6&tecnicoId=6");

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, "tecnicoId");
    }

    [Fact]
    [Trait("CA", "CA-02.7")]
    [Trait("RN", "RN-04")]
    public async Task Listar_TecnicoConSuPropioId_Responde200()
    {
        var pagina = await ListarOkAsync(Fabian, "?tecnicoId=6");

        Assert.Equal([Ticket12, Ticket14, Ticket15], Codigos(pagina));
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [InlineData("?estado=Pendiente", "estado")]
    [InlineData("?estado=1", "estado")]
    [InlineData("?prioridad=Urgente", "prioridad")]
    [InlineData("?prioridad=0", "prioridad")]
    [InlineData("?tecnicoId=abc", "tecnicoId")]
    [InlineData("?tecnicoId=%2B2", "tecnicoId")]
    [InlineData("?tecnicoId=0", "tecnicoId")]
    [InlineData("?soloVencidos=si", "soloVencidos")]
    [InlineData("?soloVencidos=1", "soloVencidos")]
    public async Task Listar_FiltroInvalido_Responde400ConSuClave(string query, string clave)
    {
        // Decisiones 58, 102 y 105.
        var respuesta = await ListarAsync(Supervisor, query);

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [InlineData("?estado=Abierto&estado=Asignado", "estado")]
    [InlineData("?soloVencidos=true&soloVencidos=true", "soloVencidos")]
    [InlineData("?page=1&Page=2", "page")]
    public async Task Listar_ParametroRepetido_Responde400ConSuClave(string query, string clave)
    {
        // Decisión 103; con la decisión 106, page y Page son el mismo parámetro.
        var respuesta = await ListarAsync(Supervisor, query);

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [InlineData("?estado=", "estado")]
    [InlineData("?prioridad=", "prioridad")]
    [InlineData("?soloVencidos=", "soloVencidos")]
    [InlineData("?page=", "page")]
    [InlineData("?pageSize", "pageSize")]
    public async Task Listar_ParametroVacio_Responde400ConSuClave(string query, string clave)
    {
        // Decisión 104: solo un parámetro ausente toma su valor por defecto.
        var respuesta = await ListarAsync(Supervisor, query);

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
    }

    [Fact]
    [Trait("CA", "CA-02.8")]
    public async Task Listar_VariosFiltrosInvalidos_Responde400ConTodasLasClaves()
    {
        var respuesta = await ListarAsync(Supervisor, "?estado=X&prioridad=Y&page=0");

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, "estado", "prioridad", "page");
    }

    [Fact]
    public async Task Listar_ParametroDesconocido_SeIgnora()
    {
        // Decisión 106.
        var conDesconocido = await ListarOkAsync(Supervisor, "?foo=bar&solicitanteId=2");
        var sinParametros = await ListarOkAsync(Supervisor, "");

        Assert.Equal(sinParametros.GetRawText(), conDesconocido.GetRawText());
    }

    [Theory]
    [Trait("RN", "RN-05")]
    [InlineData("?Page=2&PAGESIZE=2")]
    [InlineData("?PAGE=2&pagesize=2")]
    public async Task Listar_NombreConMayusculas_FuncionaIgual(string query)
    {
        // Decisión 106: los nombres no distinguen mayúsculas.
        var conMayusculas = await ListarOkAsync(Supervisor, query);
        var canonico = await ListarOkAsync(Supervisor, "?page=2&pageSize=2");

        Assert.Equal([Ticket11, Ticket14], Codigos(canonico));
        Assert.Equal(canonico.GetRawText(), conMayusculas.GetRawText());
    }

    [Theory]
    [InlineData(null, CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("abc", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("0", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("-1", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("2147483648", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("11", CodigosErrorApi.UsuarioInexistente)]
    public async Task Listar_EncabezadoAusenteInvalidoOUsuarioInexistente_Responde401(string? usuario, string codigo)
    {
        // También con un parámetro inválido: el 401 va primero (§1.4).
        var respuesta = await ListarAsync(usuario, "?page=0");

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, codigo);
    }

    private async Task<HttpResponseMessage> ListarAsync(string? usuario, string query)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Get, TicketsController.Ruta + query);
        if (usuario is not null)
        {
            // Sin validación del cliente: el valor llega tal cual.
            Assert.True(peticion.Headers.TryAddWithoutValidation(MiddlewareUsuarioActual.Encabezado, usuario));
        }

        return await factory.CreateClient().SendAsync(peticion);
    }

    private async Task<JsonElement> ListarOkAsync(string usuario, string query)
    {
        var respuesta = await ListarAsync(usuario, query);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("application/json", respuesta.Content.Headers.ContentType?.MediaType);
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        return documento.RootElement.Clone();
    }

    private static List<JsonElement> Items(JsonElement pagina) => [.. pagina.GetProperty("items").EnumerateArray()];

    private static string[] Codigos(JsonElement pagina) =>
        [.. Items(pagina).Select(item => item.GetProperty("codigo").GetString()!)];
}
