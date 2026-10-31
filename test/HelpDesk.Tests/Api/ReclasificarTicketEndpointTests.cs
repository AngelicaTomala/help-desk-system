using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HelpDesk.Api.Errores;
using HelpDesk.Api.Identidad;
using HelpDesk.Api.Tickets;
using HelpDesk.Application.Excepciones;
using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using HelpDesk.Tests.Domain;
using HelpDesk.Tests.Integracion;
using Microsoft.EntityFrameworkCore;
using static HelpDesk.Tests.Api.AfirmacionesProblemDetails;

namespace HelpDesk.Tests.Api;

/// <summary>
/// <c>POST /api/v1/tickets/{id}/reclasificar</c> de extremo a extremo con la base de pruebas (contrato §2.4). Cada
/// prueba prepara su ticket con Domain (<see cref="TicketsDePrueba"/>: categoría <c>Software</c>, solicitante 1 y
/// técnico 6) antes de la petición; antes y después se borran los tickets (decisión 99).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class ReclasificarTicketEndpointTests(HelpDeskApiFactory factory, BaseDePruebasFixture baseDePruebas)
    : IClassFixture<HelpDeskApiFactory>, IAsyncLifetime
{
    private const string Solicitante = "1";
    private const string TecnicoAsignado = "6";
    private const string OtroTecnico = "7";
    private const string Supervisor = "10";

    public Task InitializeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    public Task DisposeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    [Fact]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    public async Task Reclasificar_TecnicoAsignado_Responde200ConCategoriaNueva()
    {
        var preparado = TicketsDePrueba.EnEstado(EstadoTicket.Asignado);
        var id = await baseDePruebas.GuardarAsync(preparado);

        var respuesta = await ReclasificarAsync(TecnicoAsignado, id.ToString(), Cuerpo("Otros"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("application/json", respuesta.Content.Headers.ContentType?.MediaType);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal(id, ticket.GetProperty("id").GetInt32());
        Assert.Equal(preparado.Codigo, ticket.GetProperty("codigo").GetString());
        Assert.Equal("Otros", ticket.GetProperty("categoria").GetString());
        Assert.Equal("Media", ticket.GetProperty("prioridad").GetString());
        Assert.Equal("Asignado", ticket.GetProperty("estado").GetString());
        Assert.Equal("Ana Solicitante", ticket.GetProperty("solicitante").GetProperty("nombre").GetString());
        Assert.Equal(6, ticket.GetProperty("tecnico").GetProperty("id").GetInt32());
        Assert.Equal("Fabián Técnico", ticket.GetProperty("tecnico").GetProperty("nombre").GetString());
        Assert.Equal("2026-10-28T08:00:00Z", ticket.GetProperty("fechaCreacion").GetString());
        Assert.Equal("2026-10-28T14:30:00Z", ticket.GetProperty("fechaActualizacion").GetString());
        Assert.Equal("2026-10-28T16:00:00Z", ticket.GetProperty("fechaVencimiento").GetString());
        Assert.Equal("EnTiempo", ticket.GetProperty("estadoSla").GetString());
        Assert.Empty(ticket.GetProperty("comentarios").EnumerateArray());

        var guardado = await LeerAsync(id);
        Assert.Equal(Categoria.Otros, guardado.Categoria);
        Assert.Equal(TicketsDePrueba.Ahora, guardado.FechaActualizacion);
        Assert.Equal(EstadoTicket.Asignado, guardado.Estado);
    }

    [Fact]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    public async Task Reclasificar_SupervisorTicketAbierto_Responde200ConCategoriaNueva()
    {
        // Un ticket Abierto no tiene técnico: solo lo reclasifica un supervisor (hallazgo 11).
        var id = await baseDePruebas.GuardarAsync(TicketsDePrueba.Nuevo());

        var respuesta = await ReclasificarAsync(Supervisor, id.ToString(), Cuerpo("Otros"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal("Otros", ticket.GetProperty("categoria").GetString());
        Assert.Equal("Abierto", ticket.GetProperty("estado").GetString());

        var guardado = await LeerAsync(id);
        Assert.Equal(Categoria.Otros, guardado.Categoria);
        Assert.Equal(TicketsDePrueba.Ahora, guardado.FechaActualizacion);
    }

    [Fact]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    public async Task Reclasificar_MismaCategoria_Responde200SinCambiarFechaActualizacion()
    {
        // Decisión 67: sin cambios no se modifica fechaActualizacion.
        var preparado = TicketsDePrueba.EnEstado(EstadoTicket.Asignado);
        var id = await baseDePruebas.GuardarAsync(preparado);

        var respuesta = await ReclasificarAsync(TecnicoAsignado, id.ToString(), Cuerpo("software"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var ticket = await LeerJsonAsync(respuesta);
        Assert.Equal("Software", ticket.GetProperty("categoria").GetString());
        Assert.Equal("2026-10-28T08:10:00Z", ticket.GetProperty("fechaActualizacion").GetString());

        await AfirmarSinCambiosAsync(preparado);
    }

    [Theory]
    [Trait("CA", "CA-01.7")]
    [Trait("RN", "RN-13")]
    [InlineData(EstadoTicket.Resuelto, "Otros")]
    [InlineData(EstadoTicket.Cerrado, "Otros")]
    [InlineData(EstadoTicket.Resuelto, "Software")]
    [InlineData(EstadoTicket.Cerrado, "Software")]
    public async Task Reclasificar_TicketResueltoOCerrado_Responde409YNoCambia(EstadoTicket estado, string categoria)
    {
        // Decisión 70: también con la categoría actual.
        var preparado = TicketsDePrueba.EnEstado(estado);
        await baseDePruebas.GuardarAsync(preparado);

        var respuesta = await ReclasificarAsync(Supervisor, preparado.Id.ToString(), Cuerpo(categoria));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Conflict, CodigosError.TicketNoModificable);
        await AfirmarSinCambiosAsync(preparado);
    }

    [Fact]
    [Trait("CA", "CA-01.8")]
    [Trait("RN", "RN-13")]
    public async Task Reclasificar_Solicitante_Responde403RolNoAutorizado()
    {
        // Ana (1) es la dueña del ticket y aun así no puede reclasificarlo.
        var preparado = TicketsDePrueba.EnEstado(EstadoTicket.Asignado);
        await baseDePruebas.GuardarAsync(preparado);

        var respuesta = await ReclasificarAsync(Solicitante, preparado.Id.ToString(), Cuerpo("Otros"));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, CodigosErrorAplicacion.RolNoAutorizado);
        await AfirmarSinCambiosAsync(preparado);
    }

    [Fact]
    [Trait("CA", "CA-01.8")]
    [Trait("RN", "RN-13")]
    public async Task Reclasificar_TecnicoNoAsignado_Responde403TicketAjeno()
    {
        var preparado = TicketsDePrueba.EnEstado(EstadoTicket.Asignado);
        await baseDePruebas.GuardarAsync(preparado);

        var respuesta = await ReclasificarAsync(OtroTecnico, preparado.Id.ToString(), Cuerpo("Otros"));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Forbidden, CodigosErrorAplicacion.TicketAjeno);
        await AfirmarSinCambiosAsync(preparado);
    }

    [Theory]
    [Trait("CA", "CA-01.9")]
    [Trait("RN", "RN-02")]
    [Trait("RN", "RN-13")]
    [InlineData("""{"categoria": "Impresoras"}""")]
    [InlineData("""{"categoria": ""}""")]
    [InlineData("""{"categoria": "3"}""")]
    [InlineData("""{"categoria": 3}""")]
    [InlineData("{}")]
    public async Task Reclasificar_CategoriaInvalida_Responde400YNoCambia(string json)
    {
        var preparado = TicketsDePrueba.EnEstado(EstadoTicket.Asignado);
        await baseDePruebas.GuardarAsync(preparado);

        var respuesta = await ReclasificarAsync(TecnicoAsignado, preparado.Id.ToString(), Json(json));

        var cuerpo = await AfirmarProblemaAsync(respuesta, HttpStatusCode.BadRequest, CodigosErrorAplicacion.ValidacionFallida);
        AfirmarClavesErrores(cuerpo, "categoria");
        await AfirmarSinCambiosAsync(preparado);
    }

    [Fact]
    [Trait("RN", "RN-13")]
    public async Task Reclasificar_SupervisorTicketInexistenteYCategoriaInvalida_Responde404()
    {
        // Decisión 92: la existencia del ticket (404) va antes que el validador (400).
        var respuesta = await ReclasificarAsync(Supervisor, int.MaxValue.ToString(), Cuerpo("Impresoras"));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.NotFound, CodigosErrorAplicacion.TicketNoEncontrado);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("012")]
    [InlineData("+12")]
    [InlineData("2147483648")]
    public async Task Reclasificar_IdNoValido_Responde404TicketNoEncontrado(string id)
    {
        // Decisiones 65 y 95.
        var respuesta = await ReclasificarAsync(Supervisor, id, Cuerpo("Otros"));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.NotFound, CodigosErrorAplicacion.TicketNoEncontrado);
    }

    [Theory]
    [InlineData(null, CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("abc", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("0", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("-1", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("2147483648", CodigosErrorApi.UsuarioNoIdentificado)]
    [InlineData("11", CodigosErrorApi.UsuarioInexistente)]
    public async Task Reclasificar_EncabezadoAusenteInvalidoOUsuarioInexistente_Responde401YNoCambia(string? usuario, string codigo)
    {
        var preparado = TicketsDePrueba.EnEstado(EstadoTicket.Asignado);
        await baseDePruebas.GuardarAsync(preparado);

        var respuesta = await ReclasificarAsync(usuario, preparado.Id.ToString(), Cuerpo("Otros"));

        await AfirmarProblemaAsync(respuesta, HttpStatusCode.Unauthorized, codigo);
        await AfirmarSinCambiosAsync(preparado);
    }

    private async Task<HttpResponseMessage> ReclasificarAsync(string? usuario, string id, HttpContent contenido)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Post, $"{TicketsController.Ruta}/{id}/reclasificar")
        {
            Content = contenido,
        };
        if (usuario is not null)
        {
            // Sin validación del cliente: el valor llega tal cual.
            Assert.True(peticion.Headers.TryAddWithoutValidation(MiddlewareUsuarioActual.Encabezado, usuario));
        }

        return await factory.CreateClient().SendAsync(peticion);
    }

    private async Task<Ticket> LeerAsync(int id)
    {
        await using var context = baseDePruebas.CrearContexto();
        return await context.Tickets.AsNoTracking().SingleAsync(ticket => ticket.Id == id);
    }

    // RNF-04: una operación rechazada no modifica datos.
    private async Task AfirmarSinCambiosAsync(Ticket preparado)
    {
        var guardado = await LeerAsync(preparado.Id);
        Assert.Equal(preparado.Categoria, guardado.Categoria);
        Assert.Equal(preparado.Estado, guardado.Estado);
        Assert.Equal(preparado.FechaActualizacion, guardado.FechaActualizacion);
    }

    private static JsonContent Cuerpo(string categoria) => JsonContent.Create(new { categoria });

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage respuesta)
    {
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        return documento.RootElement.Clone();
    }
}
