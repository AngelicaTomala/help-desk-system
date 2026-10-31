using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HelpDesk.Api.Errores;
using HelpDesk.Api.Identidad;
using HelpDesk.Api.Tickets;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using HelpDesk.Tests.Domain;
using HelpDesk.Tests.Integracion;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;

namespace HelpDesk.Tests.Api;

/// <summary>
/// <c>POST /api/v1/tickets</c> de extremo a extremo con la base de pruebas (contrato §2.1). Antes y después de cada
/// prueba se borran los tickets y la SEQUENCE vuelve a 1 (decisión 99).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class CrearTicketEndpointTests(HelpDeskApiFactory factory, BaseDePruebasFixture baseDePruebas)
    : IClassFixture<HelpDeskApiFactory>, IAsyncLifetime
{
    private const string Ruta = TicketsController.Ruta;
    private const string Solicitante = "1";
    private const string Tecnico = "6";
    private const string Supervisor = "10";

    private const string TituloValido = "No puedo instalar el antivirus";
    private const string DescripcionValida = "El instalador muestra un error de permisos al iniciar.";

    public Task InitializeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    public Task DisposeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    [Fact]
    [Trait("CA", "CA-01.1")]
    [Trait("RN", "RN-01")]
    [Trait("RN", "RN-02")]
    [Trait("RN", "RN-03")]
    public async Task Crear_DatosValidos_Responde201ConDetalleYLocationRelativo()
    {
        var respuesta = await CrearAsync(Solicitante, Cuerpo());

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.Equal("application/json", respuesta.Content.Headers.ContentType?.MediaType);
        var ticket = await LeerJsonAsync(respuesta);
        var id = ticket.GetProperty("id").GetInt32();

        Assert.NotNull(respuesta.Headers.Location);
        Assert.False(respuesta.Headers.Location.IsAbsoluteUri);
        Assert.Equal($"/api/v1/tickets/{id}", respuesta.Headers.Location.OriginalString);

        Assert.Equal("HD-000001", ticket.GetProperty("codigo").GetString());
        Assert.Equal(TituloValido, ticket.GetProperty("titulo").GetString());
        Assert.Equal(DescripcionValida, ticket.GetProperty("descripcion").GetString());
        Assert.Equal("Software", ticket.GetProperty("categoria").GetString());
        Assert.Equal("Alta", ticket.GetProperty("prioridad").GetString());
        Assert.Equal("Abierto", ticket.GetProperty("estado").GetString());
        Assert.Equal(1, ticket.GetProperty("solicitante").GetProperty("id").GetInt32());
        Assert.Equal("Ana Solicitante", ticket.GetProperty("solicitante").GetProperty("nombre").GetString());
        Assert.Equal(JsonValueKind.Null, ticket.GetProperty("tecnico").ValueKind);
        Assert.Equal("2026-10-28T14:30:00Z", ticket.GetProperty("fechaCreacion").GetString());
        Assert.Equal("2026-10-28T14:30:00Z", ticket.GetProperty("fechaActualizacion").GetString());
        Assert.Equal("2026-10-28T18:30:00Z", ticket.GetProperty("fechaVencimiento").GetString());
        Assert.Equal(JsonValueKind.Null, ticket.GetProperty("fechaResolucion").ValueKind);
        Assert.Equal(JsonValueKind.Null, ticket.GetProperty("fechaCierre").ValueKind);
        Assert.Equal("EnTiempo", ticket.GetProperty("estadoSla").GetString());
        Assert.Empty(ticket.GetProperty("comentarios").EnumerateArray());

        await using var context = baseDePruebas.CrearContexto();
        var guardado = await context.Tickets.AsNoTracking().SingleAsync();
        Assert.Equal(id, guardado.Id);
        Assert.Equal("HD-000001", guardado.Codigo);
        Assert.Equal(EstadoTicket.Abierto, guardado.Estado);
        Assert.Equal(1, guardado.SolicitanteId);
        Assert.Null(guardado.TecnicoId);
    }

    [Fact]
    [Trait("CA", "CA-01.1")]
    [Trait("RN", "RN-01")]
    public async Task Crear_TextosConEspacios_GuardaTextosRecortados()
    {
        var respuesta = await CrearAsync(Solicitante, Cuerpo(titulo: $"  {TituloValido}\t", descripcion: $"\n{DescripcionValida}  "));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal(TituloValido, ticket.GetProperty("titulo").GetString());
        Assert.Equal(DescripcionValida, ticket.GetProperty("descripcion").GetString());

        await using var context = baseDePruebas.CrearContexto();
        var guardado = await context.Tickets.AsNoTracking().SingleAsync();
        Assert.Equal(TituloValido, guardado.Titulo);
        Assert.Equal(DescripcionValida, guardado.Descripcion);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task Crear_SolicitanteIdEnCuerpo_UsaElDelEncabezado()
    {
        // Decisiones 54 y 66: el campo no previsto se ignora.
        var json = $$"""
            {"titulo": "{{TituloValido}}", "descripcion": "{{DescripcionValida}}", "categoria": "Software", "prioridad": "Alta", "solicitanteId": 2}
            """;

        var respuesta = await CrearAsync(Solicitante, Json(json));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal(1, ticket.GetProperty("solicitante").GetProperty("id").GetInt32());

        await using var context = baseDePruebas.CrearContexto();
        Assert.Equal(1, (await context.Tickets.AsNoTracking().SingleAsync()).SolicitanteId);
    }

    [Fact]
    [Trait("CA", "CA-01.2")]
    [Trait("RN", "RN-10")]
    public async Task Crear_PrioridadAlta_VenceCuatroHorasDespues()
    {
        var respuesta = await CrearAsync(Solicitante, Cuerpo(prioridad: "Alta"));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal("2026-10-28T14:30:00Z", ticket.GetProperty("fechaCreacion").GetString());
        Assert.Equal("2026-10-28T18:30:00Z", ticket.GetProperty("fechaVencimiento").GetString());

        await using var context = baseDePruebas.CrearContexto();
        var guardado = await context.Tickets.AsNoTracking().SingleAsync();
        Assert.Equal(guardado.FechaCreacion.AddHours(4), guardado.FechaVencimiento);
    }

    [Theory]
    [Trait("CA", "CA-05.1")]
    [Trait("RN", "RN-10")]
    [InlineData("Critica", "2026-10-28T16:30:00Z")]
    [InlineData("Alta", "2026-10-28T18:30:00Z")]
    [InlineData("Media", "2026-10-28T22:30:00Z")]
    [InlineData("Baja", "2026-10-29T14:30:00Z")]
    public async Task Crear_CadaPrioridad_VenceSegunSuSla(string prioridad, string vencimiento)
    {
        var respuesta = await CrearAsync(Solicitante, Cuerpo(prioridad: prioridad));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal(prioridad, ticket.GetProperty("prioridad").GetString());
        Assert.Equal(vencimiento, ticket.GetProperty("fechaVencimiento").GetString());

        await using var context = baseDePruebas.CrearContexto();
        var guardado = await context.Tickets.AsNoTracking().SingleAsync();
        Assert.Equal(DateTime.Parse(vencimiento).ToUniversalTime(), guardado.FechaVencimiento);
    }

    [Theory]
    [Trait("CA", "CA-01.3")]
    [Trait("RN", "RN-01")]
    [InlineData(4, 10, "titulo")]
    [InlineData(151, 10, "titulo")]
    [InlineData(5, 9, "descripcion")]
    [InlineData(5, 2001, "descripcion")]
    public async Task Crear_TextoFueraDeRango_Responde400YNoCrea(int longitudTitulo, int longitudDescripcion, string clave)
    {
        var respuesta = await CrearAsync(
            Solicitante,
            Cuerpo(titulo: TicketsDePrueba.Texto(longitudTitulo), descripcion: TicketsDePrueba.Texto(longitudDescripcion)));

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
        await AfirmarSinTicketsAsync();
    }

    [Theory]
    [Trait("CA", "CA-01.3")]
    [Trait("RN", "RN-01")]
    [InlineData("     ", DescripcionValida, "titulo")]
    [InlineData(TituloValido, " \t\n ", "descripcion")]
    public async Task Crear_TextoEnBlanco_Responde400YNoCrea(string titulo, string descripcion, string clave)
    {
        var respuesta = await CrearAsync(Solicitante, Cuerpo(titulo: titulo, descripcion: descripcion));

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
        await AfirmarSinTicketsAsync();
    }

    [Theory]
    [Trait("CA", "CA-01.3")]
    [Trait("RN", "RN-01")]
    [InlineData(5, 10)]
    [InlineData(150, 2000)]
    public async Task Crear_TextosEnLosLimites_Responde201(int longitudTitulo, int longitudDescripcion)
    {
        var respuesta = await CrearAsync(
            Solicitante,
            Cuerpo(titulo: TicketsDePrueba.Texto(longitudTitulo), descripcion: TicketsDePrueba.Texto(longitudDescripcion)));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        await using var context = baseDePruebas.CrearContexto();
        Assert.Equal(1, await context.Tickets.CountAsync());
    }

    [Fact]
    [Trait("CA", "CA-01.3")]
    [Trait("CA", "CA-01.4")]
    [Trait("RN", "RN-01")]
    [Trait("RN", "RN-02")]
    public async Task Crear_EjemploDelContrato_Responde400ConTresClaves()
    {
        // Contrato §4.1.
        var respuesta = await CrearAsync(Solicitante, Cuerpo(titulo: "Red", descripcion: "     ", categoria: "Impresoras"));

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, "titulo", "descripcion", "categoria");
        await AfirmarSinTicketsAsync();
    }

    [Theory]
    [Trait("CA", "CA-01.4")]
    [Trait("RN", "RN-02")]
    [InlineData("""{"titulo": "No puedo instalar el antivirus", "descripcion": "El instalador muestra un error.", "categoria": "Impresoras", "prioridad": "Alta"}""", "categoria")]
    [InlineData("""{"titulo": "No puedo instalar el antivirus", "descripcion": "El instalador muestra un error.", "categoria": "Software", "prioridad": "Urgente"}""", "prioridad")]
    [InlineData("""{"titulo": "No puedo instalar el antivirus", "descripcion": "El instalador muestra un error.", "prioridad": "Alta"}""", "categoria")]
    [InlineData("""{"titulo": "No puedo instalar el antivirus", "descripcion": "El instalador muestra un error.", "categoria": "Software"}""", "prioridad")]
    [InlineData("""{"titulo": "No puedo instalar el antivirus", "descripcion": "El instalador muestra un error.", "categoria": 3, "prioridad": "Alta"}""", "categoria")]
    [InlineData("""{"titulo": "No puedo instalar el antivirus", "descripcion": "El instalador muestra un error.", "categoria": "Software", "prioridad": "2"}""", "prioridad")]
    public async Task Crear_CategoriaOPrioridadInvalida_Responde400YNoCrea(string json, string clave)
    {
        var respuesta = await CrearAsync(Solicitante, Json(json));

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, clave);
        await AfirmarSinTicketsAsync();
    }

    [Fact]
    [Trait("RN", "RN-02")]
    public async Task Crear_EnumsEnMinusculas_Responde201ConGrafiaCanonica()
    {
        // Decisión 58: la entrada no distingue mayúsculas; la respuesta usa la grafía canónica.
        var respuesta = await CrearAsync(Solicitante, Cuerpo(categoria: "software", prioridad: "CRITICA"));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal("Software", ticket.GetProperty("categoria").GetString());
        Assert.Equal("Critica", ticket.GetProperty("prioridad").GetString());
    }

    [Fact]
    [Trait("CA", "CA-01.5")]
    public async Task Crear_UsuarioInexistente_Responde401YNoCrea()
    {
        var respuesta = await CrearAsync("11", Cuerpo());

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioInexistente);
        await AfirmarSinTicketsAsync();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public async Task Crear_EncabezadoAusenteOInvalido_Responde401YNoCrea(string? encabezado)
    {
        var respuesta = await CrearAsync(encabezado, Cuerpo());

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, CodigosErrorApi.UsuarioNoIdentificado);
        await AfirmarSinTicketsAsync();
    }

    [Theory]
    [Trait("CA", "CA-01.10")]
    [Trait("RN", "RN-03")]
    [InlineData(Tecnico)]
    [InlineData(Supervisor)]
    public async Task Crear_TecnicoOSupervisor_Responde403YNoCrea(string usuario)
    {
        var respuesta = await CrearAsync(usuario, Cuerpo());

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, CodigosErrorAplicacion.RolNoAutorizado);
        await AfirmarSinTicketsAsync();
    }

    [Fact]
    [Trait("CA", "CA-01.10")]
    [Trait("RN", "RN-03")]
    public async Task Crear_TecnicoConTituloInvalido_Responde403YNoCrea()
    {
        // Decisión 92: el permiso (403) va antes que el validador (400).
        var respuesta = await CrearAsync(Tecnico, Cuerpo(titulo: "Red"));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, CodigosErrorAplicacion.RolNoAutorizado);
        await AfirmarSinTicketsAsync();
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task Crear_SecuenciaAgotada_Responde500YNoCreaElTicket()
    {
        // Decisión 90: tras HD-999999 la SEQUENCE falla y el ticket no se crea.
        await baseDePruebas.ReiniciarSecuenciaEnMaximoAsync();

        var primera = await CrearAsync(Solicitante, Cuerpo());
        Assert.Equal(HttpStatusCode.Created, primera.StatusCode);
        Assert.Equal("HD-999999", (await LeerJsonAsync(primera)).GetProperty("codigo").GetString());

        var segunda = await CrearAsync(Solicitante, Cuerpo(titulo: "Segundo ticket sin código"));

        var cuerpo = await AfirmarProblemaAsync(segunda, HttpStatusCode.InternalServerError, CodigosError.ErrorInterno);
        Assert.DoesNotContain("SqlException", cuerpo.GetRawText());
        Assert.Contains(factory.Logs.Entradas, entrada =>
            entrada.Categoria == typeof(ManejadorExcepciones).FullName
            && entrada.Nivel == LogLevel.Error
            && entrada.Excepcion is SqlException);

        await using var context = baseDePruebas.CrearContexto();
        var guardado = await context.Tickets.AsNoTracking().SingleAsync();
        Assert.Equal("HD-999999", guardado.Codigo);
        Assert.Equal(TituloValido, guardado.Titulo);
    }

    private async Task<HttpResponseMessage> CrearAsync(string? usuario, HttpContent contenido)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Post, Ruta) { Content = contenido };
        if (usuario is not null)
        {
            // Sin validación del cliente: el valor llega tal cual.
            Assert.True(peticion.Headers.TryAddWithoutValidation(MiddlewareUsuarioActual.Encabezado, usuario));
        }

        return await factory.CreateClient().SendAsync(peticion);
    }

    private async Task AfirmarSinTicketsAsync()
    {
        await using var context = baseDePruebas.CrearContexto();
        Assert.Equal(0, await context.Tickets.CountAsync());
    }

    private static JsonContent Cuerpo(
        string titulo = TituloValido,
        string descripcion = DescripcionValida,
        string categoria = "Software",
        string prioridad = "Alta") =>
        JsonContent.Create(new { titulo, descripcion, categoria, prioridad });

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage respuesta)
    {
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        return documento.RootElement.Clone();
    }
}
