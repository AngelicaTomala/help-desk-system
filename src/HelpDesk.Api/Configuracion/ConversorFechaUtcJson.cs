using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HelpDesk.Api.Configuracion;

/// <summary>
/// Serializa las fechas en ISO 8601 UTC con sufijo <c>Z</c>; los segundos fraccionarios solo si no son cero
/// (decisión 60). Una fecha <see cref="DateTimeKind.Unspecified"/> se toma como UTC, porque todas lo son (S-04).
/// </summary>
public sealed class ConversorFechaUtcJson : JsonConverter<DateTime>
{
    private const string Formato = "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        AUtc(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(AUtc(value).ToString(Formato, CultureInfo.InvariantCulture));

    private static DateTime AUtc(DateTime fecha) => fecha.Kind switch
    {
        DateTimeKind.Utc => fecha,
        DateTimeKind.Local => fecha.ToUniversalTime(),
        _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc),
    };
}
