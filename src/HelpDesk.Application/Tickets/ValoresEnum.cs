namespace HelpDesk.Application.Tickets;

/// <summary>
/// Convierte el texto de un enum de entrada (decisiones 58 y 96): solo acepta el nombre exacto de un valor, sin
/// distinguir mayúsculas. Rechaza los textos numéricos, las combinaciones (<c>"Red,Software"</c>) y los espacios.
/// </summary>
public static class ValoresEnum
{
    public static bool TryParse<TEnum>(string? texto, out TEnum valor)
        where TEnum : struct, Enum
    {
        var nombre = Enum.GetNames<TEnum>()
            .FirstOrDefault(candidato => string.Equals(candidato, texto, StringComparison.OrdinalIgnoreCase));

        if (nombre is null)
        {
            valor = default;
            return false;
        }

        valor = Enum.Parse<TEnum>(nombre);
        return true;
    }

    public static bool EsValido<TEnum>(string? texto)
        where TEnum : struct, Enum =>
        TryParse<TEnum>(texto, out _);
}
