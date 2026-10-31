using System.Net;
using System.Text.Json;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Comprobaciones comunes de Problem Details (RNF-03, decisiones 29, 62 y 94).
/// </summary>
internal static class AfirmacionesProblemDetails
{
    /// <summary>
    /// <c>Content-Type</c>, <c>status</c>, <c>type</c>, <c>title</c>, <c>traceId</c> y <c>codigo</c>
    /// (<see langword="null"/>: sin <c>codigo</c>), y la ausencia de <c>detail</c> e <c>instance</c>.
    /// </summary>
    public static async Task<JsonElement> AfirmarProblemaAsync(HttpResponseMessage respuesta, HttpStatusCode status, string? codigo)
    {
        Assert.Equal(status, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);

        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var cuerpo = documento.RootElement.Clone();

        Assert.Equal((int)status, cuerpo.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("type").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("traceId").GetString()));
        Assert.False(cuerpo.TryGetProperty("detail", out _));
        Assert.False(cuerpo.TryGetProperty("instance", out _));

        if (codigo is null)
        {
            Assert.False(cuerpo.TryGetProperty("codigo", out _));
        }
        else
        {
            Assert.Equal(codigo, cuerpo.GetProperty("codigo").GetString());
        }

        return cuerpo;
    }

    /// <summary>
    /// Las claves de <c>errors</c> son exactamente las indicadas, cada una con al menos un mensaje.
    /// </summary>
    public static void AfirmarClavesErrores(JsonElement cuerpo, params string[] claves)
    {
        var errores = cuerpo.GetProperty("errors");
        var clavesRecibidas = errores.EnumerateObject().Select(propiedad => propiedad.Name).Order().ToArray();

        Assert.Equal(claves.Order().ToArray(), clavesRecibidas);
        Assert.All(errores.EnumerateObject(), propiedad => Assert.NotEmpty(propiedad.Value.EnumerateArray()));
    }
}
