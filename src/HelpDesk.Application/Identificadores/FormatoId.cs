using System.Globalization;

namespace HelpDesk.Application.Identificadores;

/// <summary>
/// Formato de un Id en <c>X-Usuario-Id</c> y en <c>{id}</c> (decisión 95): solo dígitos ASCII, sin signo, sin
/// espacios y sin ceros a la izquierda, dentro del rango de <c>int</c> y mayor que 0.
/// </summary>
public static class FormatoId
{
    private const int LongitudMaxima = 10;

    public static bool TryLeer(string? texto, out int id)
    {
        id = 0;

        if (string.IsNullOrEmpty(texto) || texto.Length > LongitudMaxima || texto[0] == '0')
        {
            return false;
        }

        foreach (var caracter in texto)
        {
            if (!char.IsAsciiDigit(caracter))
            {
                return false;
            }
        }

        // Solo dígitos y sin cero inicial: falla únicamente por estar fuera del rango de int.
        return int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out id);
    }
}
