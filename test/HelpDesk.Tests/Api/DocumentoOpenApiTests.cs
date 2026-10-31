using System.Net;
using System.Text.Json;
using HelpDesk.Api.Identidad;
using HelpDesk.Api.Tickets;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Integracion;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Documento <c>/openapi/v1.json</c> de las operaciones de RF-01 (RNF-09): encabezado <c>X-Usuario-Id</c>, códigos de
/// respuesta del contrato con sus esquemas y tipos de contenido, y enums como texto. Solo se leen las rutas reales de
/// <see cref="TicketsController"/>, no las de <c>PruebasController</c>. No comprueban un CA concreto: van sin Trait.
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class DocumentoOpenApiTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private const string Documento = "openapi/v1.json";
    private const string RutaCrear = "/" + TicketsController.Ruta;
    private const string RutaReclasificar = "/" + TicketsController.Ruta + "/{id}/reclasificar";
    private const string Json = "application/json";
    private const string ProblemJson = "application/problem+json";

    // Catálogo de codigo del contrato §1.3 (ADR-004, Enmienda 3).
    private static readonly string[] Catalogo =
    [
        "VALIDACION_FALLIDA", "TECNICO_NO_VALIDO", "MISMO_TECNICO",
        "USUARIO_NO_IDENTIFICADO", "USUARIO_INEXISTENTE",
        "ROL_NO_AUTORIZADO", "TICKET_AJENO", "FILTRO_NO_AUTORIZADO",
        "TICKET_NO_ENCONTRADO",
        "TRANSICION_INVALIDA", "TICKET_NO_MODIFICABLE",
        "ERROR_INTERNO",
    ];

    public static TheoryData<string> Operaciones => [RutaCrear, RutaReclasificar];

    [Fact]
    public async Task ObtenerDocumento_SinEncabezado_Responde200()
    {
        var respuesta = await factory.CreateClient().GetAsync(Documento);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(Json, respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [MemberData(nameof(Operaciones))]
    public async Task Operacion_EncabezadoUsuario_ObligatorioConFormato(string ruta)
    {
        using var documento = await LeerDocumentoAsync();

        var encabezados = Operacion(documento, ruta).GetProperty("parameters").EnumerateArray()
            .Where(parametro => parametro.GetProperty("in").GetString() == "header"
                && parametro.GetProperty("name").GetString() == MiddlewareUsuarioActual.Encabezado)
            .ToList();

        var encabezado = Assert.Single(encabezados);
        Assert.True(encabezado.GetProperty("required").GetBoolean());
        var esquema = encabezado.GetProperty("schema");
        Assert.True(TieneTipo(esquema, "string"));
        Assert.Equal("^[1-9][0-9]{0,9}$", esquema.GetProperty("pattern").GetString());
        Assert.Contains("2147483647", encabezado.GetProperty("description").GetString());
    }

    [Theory]
    [InlineData(RutaCrear, new[] { "201", "400", "401", "403", "415", "500" })]
    [InlineData(RutaReclasificar, new[] { "200", "400", "401", "403", "404", "409", "415", "500" })]
    public async Task Operacion_Respuestas_DeclaraCodigosDelContrato(string ruta, string[] codigos)
    {
        using var documento = await LeerDocumentoAsync();

        var declarados = Respuestas(documento, ruta).EnumerateObject().Select(respuesta => respuesta.Name).Order();

        Assert.Equal(codigos.Order(), declarados);
    }

    [Theory]
    [InlineData(RutaCrear, "201")]
    [InlineData(RutaReclasificar, "200")]
    public async Task Operacion_RespuestaCorrecta_DevuelveDetalleJson(string ruta, string codigo)
    {
        using var documento = await LeerDocumentoAsync();

        var contenido = Respuestas(documento, ruta).GetProperty(codigo).GetProperty("content");

        var tipo = Assert.Single(contenido.EnumerateObject());
        Assert.Equal(Json, tipo.Name);
        Assert.Equal("#/components/schemas/TicketDetalleDto", tipo.Value.GetProperty("schema").GetProperty("$ref").GetString());
    }

    [Fact]
    public async Task Crear_Respuesta201_DeclaraLocation()
    {
        using var documento = await LeerDocumentoAsync();

        var location = Respuestas(documento, RutaCrear).GetProperty("201").GetProperty("headers").GetProperty("Location");

        Assert.True(TieneTipo(location.GetProperty("schema"), "string"));
    }

    [Theory]
    [MemberData(nameof(Operaciones))]
    public async Task Operacion_Errores_UsanProblemDetails(string ruta)
    {
        using var documento = await LeerDocumentoAsync();

        var errores = Respuestas(documento, ruta).EnumerateObject().Where(respuesta => respuesta.Name[0] is '4' or '5');

        Assert.All(errores, error =>
        {
            var tipo = Assert.Single(error.Value.GetProperty("content").EnumerateObject());
            Assert.Equal(ProblemJson, tipo.Name);

            var esperado = error.Name == "400" ? "HttpValidationProblemDetails" : "ProblemDetails";
            Assert.Equal($"#/components/schemas/{esperado}", tipo.Value.GetProperty("schema").GetProperty("$ref").GetString());
        });
    }

    [Theory]
    [InlineData("ProblemDetails")]
    [InlineData("HttpValidationProblemDetails")]
    public async Task ProblemDetails_Esquema_DeclaraCodigoYTraceId(string nombre)
    {
        using var documento = await LeerDocumentoAsync();
        var esquema = Esquema(documento, nombre);
        var propiedades = esquema.GetProperty("properties");

        foreach (var campo in new[] { "type", "title", "status", "traceId" })
        {
            Assert.True(propiedades.TryGetProperty(campo, out _), $"Falta {campo} en {nombre}.");
        }

        var codigo = propiedades.GetProperty("codigo");
        Assert.True(TieneTipo(codigo, "string"));
        Assert.Equal(Catalogo, Valores(codigo));

        // codigo no es obligatorio: las respuestas de la decisión 94 no lo llevan.
        Assert.False(esquema.TryGetProperty("required", out var requeridos)
            && requeridos.EnumerateArray().Any(requerido => requerido.GetString() == "codigo"));

        if (nombre == "HttpValidationProblemDetails")
        {
            Assert.Equal("object", propiedades.GetProperty("errors").GetProperty("type").GetString());
        }
    }

    [Fact]
    public async Task TicketDetalle_Enums_ComoTextoConValores()
    {
        using var documento = await LeerDocumentoAsync();
        var propiedades = Esquema(documento, "TicketDetalleDto").GetProperty("properties");
        var comentario = Resolver(documento, propiedades.GetProperty("comentarios").GetProperty("items"));

        AfirmarEnum<EstadoTicket>(Resolver(documento, propiedades.GetProperty("estado")));
        AfirmarEnum<Prioridad>(Resolver(documento, propiedades.GetProperty("prioridad")));
        AfirmarEnum<Categoria>(Resolver(documento, propiedades.GetProperty("categoria")));
        AfirmarEnum<EstadoSla>(Resolver(documento, propiedades.GetProperty("estadoSla")));
        AfirmarEnum<TipoComentario>(Resolver(documento, comentario.GetProperty("properties").GetProperty("tipo")));
    }

    [Fact]
    public async Task TicketDetalle_Fechas_ComoTextoDateTime()
    {
        using var documento = await LeerDocumentoAsync();
        var propiedades = Esquema(documento, "TicketDetalleDto").GetProperty("properties");

        foreach (var campo in new[] { "fechaCreacion", "fechaActualizacion", "fechaVencimiento", "fechaResolucion", "fechaCierre" })
        {
            var fecha = propiedades.GetProperty(campo);
            Assert.True(TieneTipo(fecha, "string"), $"{campo} no es texto.");
            Assert.Equal("date-time", fecha.GetProperty("format").GetString());
        }

        Assert.True(TieneTipo(propiedades.GetProperty("fechaResolucion"), "null"));
        Assert.True(TieneTipo(propiedades.GetProperty("fechaCierre"), "null"));
    }

    [Fact]
    public async Task Crear_Cuerpo_EnumsConValores()
    {
        using var documento = await LeerDocumentoAsync();
        var propiedades = Cuerpo(documento, RutaCrear).GetProperty("properties");

        AfirmarEnum<Categoria>(propiedades.GetProperty("categoria"));
        AfirmarEnum<Prioridad>(propiedades.GetProperty("prioridad"));
    }

    [Fact]
    public async Task Reclasificar_Cuerpo_CategoriaConValores()
    {
        using var documento = await LeerDocumentoAsync();
        var propiedades = Cuerpo(documento, RutaReclasificar).GetProperty("properties");

        AfirmarEnum<Categoria>(propiedades.GetProperty("categoria"));
    }

    private async Task<JsonDocument> LeerDocumentoAsync()
    {
        var respuesta = await factory.CreateClient().GetAsync(Documento);
        respuesta.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
    }

    private static JsonElement Operacion(JsonDocument documento, string ruta) =>
        documento.RootElement.GetProperty("paths").GetProperty(ruta).GetProperty("post");

    private static JsonElement Respuestas(JsonDocument documento, string ruta) =>
        Operacion(documento, ruta).GetProperty("responses");

    private static JsonElement Cuerpo(JsonDocument documento, string ruta) =>
        Resolver(documento, Operacion(documento, ruta)
            .GetProperty("requestBody").GetProperty("content").GetProperty(Json).GetProperty("schema"));

    private static JsonElement Esquema(JsonDocument documento, string nombre) =>
        documento.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(nombre);

    private static JsonElement Resolver(JsonDocument documento, JsonElement esquema) =>
        esquema.TryGetProperty("$ref", out var referencia)
            ? Esquema(documento, referencia.GetString()!.Split('/')[^1])
            : esquema;

    private static bool TieneTipo(JsonElement esquema, string tipo) =>
        esquema.TryGetProperty("type", out var tipos) && (tipos.ValueKind == JsonValueKind.Array
            ? tipos.EnumerateArray().Any(valor => valor.GetString() == tipo)
            : tipos.GetString() == tipo);

    private static string?[] Valores(JsonElement esquema) =>
        [.. esquema.GetProperty("enum").EnumerateArray().Select(valor => valor.GetString())];

    private static void AfirmarEnum<TEnum>(JsonElement esquema)
        where TEnum : struct, Enum
    {
        Assert.True(TieneTipo(esquema, "string"), $"{typeof(TEnum).Name} no es texto.");
        Assert.Equal(Enum.GetNames<TEnum>(), Valores(esquema));
    }
}
