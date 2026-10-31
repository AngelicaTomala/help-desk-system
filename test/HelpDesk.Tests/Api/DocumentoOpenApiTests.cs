using System.Net;
using System.Text.Json;
using HelpDesk.Api.Identidad;
using HelpDesk.Api.Tickets;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Integracion;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Documento <c>/openapi/v1.json</c> de las operaciones de RF-01 y RF-02 (RNF-09): encabezado <c>X-Usuario-Id</c>,
/// códigos de respuesta del contrato con sus esquemas y tipos de contenido, parámetros de la URL del listado con sus
/// valores y su formato (decisiones 101 a 105), y enums como texto. Solo se leen las rutas reales de
/// <see cref="TicketsController"/>, no las de <c>PruebasController</c>. No comprueban un CA concreto: van sin Trait.
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class DocumentoOpenApiTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    private const string Documento = "openapi/v1.json";
    private const string RutaCrear = "/" + TicketsController.Ruta;
    private const string RutaReclasificar = "/" + TicketsController.Ruta + "/{id}/reclasificar";
    private const string RutaObtener = "/" + TicketsController.Ruta + "/{id}";
    private const string RutaListar = "/" + TicketsController.Ruta;
    private const string Get = "get";
    private const string Post = "post";
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

    public static TheoryData<string, string> Operaciones => new()
    {
        { RutaCrear, Post },
        { RutaReclasificar, Post },
        { RutaObtener, Get },
        { RutaListar, Get },
    };

    [Fact]
    public async Task ObtenerDocumento_SinEncabezado_Responde200()
    {
        var respuesta = await factory.CreateClient().GetAsync(Documento);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(Json, respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [MemberData(nameof(Operaciones))]
    public async Task Operacion_EncabezadoUsuario_ObligatorioConFormato(string ruta, string metodo)
    {
        using var documento = await LeerDocumentoAsync();

        var encabezados = Operacion(documento, ruta, metodo).GetProperty("parameters").EnumerateArray()
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
    [InlineData(RutaCrear, Post, new[] { "201", "400", "401", "403", "415", "500" })]
    [InlineData(RutaReclasificar, Post, new[] { "200", "400", "401", "403", "404", "409", "415", "500" })]
    [InlineData(RutaObtener, Get, new[] { "200", "401", "403", "404", "500" })]
    [InlineData(RutaListar, Get, new[] { "200", "400", "401", "403", "500" })]
    public async Task Operacion_Respuestas_DeclaraCodigosDelContrato(string ruta, string metodo, string[] codigos)
    {
        using var documento = await LeerDocumentoAsync();

        var declarados = Respuestas(documento, ruta, metodo).EnumerateObject().Select(respuesta => respuesta.Name).Order();

        Assert.Equal(codigos.Order(), declarados);
    }

    [Theory]
    [InlineData(RutaCrear, Post, "201")]
    [InlineData(RutaReclasificar, Post, "200")]
    [InlineData(RutaObtener, Get, "200")]
    public async Task Operacion_RespuestaCorrecta_DevuelveDetalleJson(string ruta, string metodo, string codigo)
    {
        using var documento = await LeerDocumentoAsync();

        var contenido = Respuestas(documento, ruta, metodo).GetProperty(codigo).GetProperty("content");

        var tipo = Assert.Single(contenido.EnumerateObject());
        Assert.Equal(Json, tipo.Name);
        Assert.Equal("#/components/schemas/TicketDetalleDto", tipo.Value.GetProperty("schema").GetProperty("$ref").GetString());
    }

    [Fact]
    public async Task Crear_Respuesta201_DeclaraLocation()
    {
        using var documento = await LeerDocumentoAsync();

        var location = Respuestas(documento, RutaCrear, Post).GetProperty("201").GetProperty("headers").GetProperty("Location");

        Assert.True(TieneTipo(location.GetProperty("schema"), "string"));
    }

    [Theory]
    [MemberData(nameof(Operaciones))]
    public async Task Operacion_Errores_UsanProblemDetails(string ruta, string metodo)
    {
        using var documento = await LeerDocumentoAsync();

        var errores = Respuestas(documento, ruta, metodo).EnumerateObject().Where(respuesta => respuesta.Name[0] is '4' or '5');

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

    [Fact]
    public async Task Listar_Respuesta200_DevuelvePaginaDeResumenes()
    {
        using var documento = await LeerDocumentoAsync();

        var contenido = Respuestas(documento, RutaListar, Get).GetProperty("200").GetProperty("content");

        var tipo = Assert.Single(contenido.EnumerateObject());
        Assert.Equal(Json, tipo.Name);
        var propiedades = Resolver(documento, tipo.Value.GetProperty("schema")).GetProperty("properties");
        Assert.Equal(["items", "page", "pageSize", "totalItems"], propiedades.EnumerateObject().Select(campo => campo.Name).Order());

        var items = propiedades.GetProperty("items");
        Assert.True(TieneTipo(items, "array"));
        Assert.Equal("#/components/schemas/TicketResumenDto", items.GetProperty("items").GetProperty("$ref").GetString());
        foreach (var campo in new[] { "page", "pageSize", "totalItems" })
        {
            Assert.True(TieneTipo(propiedades.GetProperty(campo), "integer"), $"{campo} no es entero.");
        }
    }

    [Fact]
    public async Task TicketResumen_EnumsYFecha_ComoTexto()
    {
        using var documento = await LeerDocumentoAsync();
        var propiedades = Esquema(documento, "TicketResumenDto").GetProperty("properties");

        Assert.Equal(
            ["codigo", "estado", "estadoSla", "fechaVencimiento", "id", "prioridad", "tecnico", "titulo"],
            propiedades.EnumerateObject().Select(campo => campo.Name).Order());
        AfirmarEnum<EstadoTicket>(Resolver(documento, propiedades.GetProperty("estado")));
        AfirmarEnum<Prioridad>(Resolver(documento, propiedades.GetProperty("prioridad")));
        AfirmarEnum<EstadoSla>(Resolver(documento, propiedades.GetProperty("estadoSla")));

        var vencimiento = propiedades.GetProperty("fechaVencimiento");
        Assert.True(TieneTipo(vencimiento, "string"));
        Assert.Equal("date-time", vencimiento.GetProperty("format").GetString());
    }

    [Fact]
    public async Task Listar_ParametrosQuery_SonLosDelContrato()
    {
        using var documento = await LeerDocumentoAsync();

        var parametros = ParametrosQuery(documento);

        Assert.Equal(
            ["estado", "page", "pageSize", "prioridad", "soloVencidos", "tecnicoId"],
            parametros.Select(parametro => parametro.GetProperty("name").GetString()).Order());
        Assert.All(parametros, parametro =>
        {
            // Ausente: valor por defecto o sin filtro (decisión 104).
            Assert.False(parametro.TryGetProperty("required", out var requerido) && requerido.GetBoolean());
            Assert.Contains("400 VALIDACION_FALLIDA", parametro.GetProperty("description").GetString());
        });
    }

    [Fact]
    public async Task Listar_ParametrosEnum_ConValores()
    {
        using var documento = await LeerDocumentoAsync();

        AfirmarEnum<EstadoTicket>(ParametroQuery(documento, "estado").GetProperty("schema"));
        AfirmarEnum<Prioridad>(ParametroQuery(documento, "prioridad").GetProperty("schema"));
    }

    [Theory]
    [InlineData("tecnicoId", 1, int.MaxValue, null)]
    [InlineData("page", 1, null, 1)]
    [InlineData("pageSize", 1, 100, 20)]
    public async Task Listar_ParametrosNumericos_ConFormatoYLimites(string nombre, int minimo, int? maximo, int? porDefecto)
    {
        using var documento = await LeerDocumentoAsync();
        var parametro = ParametroQuery(documento, nombre);
        var esquema = parametro.GetProperty("schema");

        Assert.True(TieneTipo(esquema, "integer"));
        Assert.Equal("int32", esquema.GetProperty("format").GetString());
        Assert.Equal(minimo, esquema.GetProperty("minimum").GetInt32());
        Assert.Equal(maximo, esquema.TryGetProperty("maximum", out var maximoDeclarado) ? maximoDeclarado.GetInt32() : null);
        Assert.Equal(porDefecto, esquema.TryGetProperty("default", out var porDefectoDeclarado) ? porDefectoDeclarado.GetInt32() : null);
        Assert.Contains("sin ceros a la izquierda", parametro.GetProperty("description").GetString());
    }

    [Fact]
    public async Task Listar_SoloVencidos_Booleano()
    {
        using var documento = await LeerDocumentoAsync();
        var parametro = ParametroQuery(documento, "soloVencidos");
        var esquema = parametro.GetProperty("schema");

        Assert.True(TieneTipo(esquema, "boolean"));
        Assert.False(esquema.TryGetProperty("default", out _));
        Assert.Contains("sin distinguir mayúsculas", parametro.GetProperty("description").GetString());
    }

    private async Task<JsonDocument> LeerDocumentoAsync()
    {
        var respuesta = await factory.CreateClient().GetAsync(Documento);
        respuesta.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
    }

    private static JsonElement Operacion(JsonDocument documento, string ruta, string metodo) =>
        documento.RootElement.GetProperty("paths").GetProperty(ruta).GetProperty(metodo);

    private static JsonElement Respuestas(JsonDocument documento, string ruta, string metodo) =>
        Operacion(documento, ruta, metodo).GetProperty("responses");

    private static List<JsonElement> ParametrosQuery(JsonDocument documento) =>
        [.. Operacion(documento, RutaListar, Get).GetProperty("parameters").EnumerateArray()
            .Where(parametro => parametro.GetProperty("in").GetString() == "query")];

    private static JsonElement ParametroQuery(JsonDocument documento, string nombre) =>
        Assert.Single(ParametrosQuery(documento), parametro => parametro.GetProperty("name").GetString() == nombre);

    private static JsonElement Cuerpo(JsonDocument documento, string ruta) =>
        Resolver(documento, Operacion(documento, ruta, Post)
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
