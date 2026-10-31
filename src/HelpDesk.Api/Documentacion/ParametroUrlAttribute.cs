using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace HelpDesk.Api.Documentacion;

/// <summary>
/// Declara en el documento OpenAPI un parámetro de la URL que la acción lee de <c>Request.Query</c> como texto
/// (decisión 101, RNF-09). Solo es documentación: la lectura y la validación no cambian. Lo aplica
/// <see cref="TransformadorParametrosUrl"/>.
/// </summary>
public abstract class ParametroUrlAttribute(string nombre) : Attribute
{
    public const string RepetidoOVacio = "Repetido o presente con valor vacío: 400 VALIDACION_FALLIDA (decisiones 103 y 104).";

    public string Nombre { get; } = nombre;

    /// <summary>
    /// Texto propio del parámetro, antes de las reglas comunes.
    /// </summary>
    public string? Detalle { get; init; }

    public abstract OpenApiSchema CrearEsquema();

    public string Descripcion() =>
        string.Join(' ', new[] { Detalle, DescripcionFormato, RepetidoOVacio }.Where(texto => texto is not null));

    protected virtual string? DescripcionFormato => null;
}

/// <summary>
/// Parámetro de texto con los valores de un enum (decisiones 58 y 101).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ParametroEnumAttribute(string nombre, Type tipoEnum) : ParametroUrlAttribute(nombre)
{
    public Type TipoEnum { get; } = tipoEnum;

    protected override string DescripcionFormato => TransformadorEsquemas.DescripcionEnum;

    public override OpenApiSchema CrearEsquema() => new()
    {
        Type = JsonSchemaType.String,
        Enum = [.. Enum.GetNames(TipoEnum).Select(valor => (JsonNode)valor)],
    };
}

/// <summary>
/// Parámetro entero con el formato de la decisión 105 y los rangos de RN-05. <see cref="Maximo"/> y
/// <see cref="PorDefecto"/> valen 0 cuando no aplican: los atributos no admiten <c>int?</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ParametroEnteroAttribute(string nombre, int minimo) : ParametroUrlAttribute(nombre)
{
    public int Minimo { get; } = minimo;

    public int Maximo { get; init; }

    public int PorDefecto { get; init; }

    protected override string DescripcionFormato =>
        "Solo dígitos ASCII, sin signo, sin espacios y sin ceros a la izquierda; otro formato o fuera de rango: 400 " +
        "VALIDACION_FALLIDA (decisión 105).";

    public override OpenApiSchema CrearEsquema() => new()
    {
        Type = JsonSchemaType.Integer,
        Format = "int32",
        Minimum = Minimo.ToString(CultureInfo.InvariantCulture),
        Maximum = Maximo == 0 ? null : Maximo.ToString(CultureInfo.InvariantCulture),
        Default = PorDefecto == 0 ? null : JsonValue.Create(PorDefecto),
    };
}

/// <summary>
/// Parámetro booleano: solo <c>true</c> o <c>false</c>, sin distinguir mayúsculas (decisión 102).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ParametroBooleanoAttribute(string nombre) : ParametroUrlAttribute(nombre)
{
    protected override string DescripcionFormato =>
        "true o false, sin distinguir mayúsculas; false equivale a no enviarlo; otro valor: 400 VALIDACION_FALLIDA " +
        "(decisión 102).";

    public override OpenApiSchema CrearEsquema() => new() { Type = JsonSchemaType.Boolean };
}
