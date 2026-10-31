using System.Text.Json;
using System.Text.Json.Serialization;

namespace HelpDesk.Api.Configuracion;

/// <summary>
/// Opciones JSON del contrato (§1.6 a §1.8). Se aplican igual a MVC y a los Problem Details.
/// </summary>
public static class ConfiguracionJson
{
    public static void Aplicar(JsonSerializerOptions options)
    {
        // Campos en camelCase (decisión 47).
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        // Enums como texto con el nombre del dominio; sin valores numéricos (decisiones 48 y 58).
        options.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));

        // Fechas en UTC con sufijo Z (decisión 60).
        options.Converters.Add(new ConversorFechaUtcJson());

        // Campos no previstos del cuerpo: se ignoran (decisión 66).
        options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;

        // Un número escrito como texto es un tipo JSON incorrecto (decisión 93); los valores web lo aceptarían.
        options.NumberHandling = JsonNumberHandling.Strict;
    }
}
