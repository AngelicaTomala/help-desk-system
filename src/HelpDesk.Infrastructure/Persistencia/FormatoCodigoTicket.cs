namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Convierte el valor de la SEQUENCE en el código del ticket: <c>HD-</c> + 6 dígitos (RN-03).
/// <c>HD-999999</c> es el límite conocido del MVP (decisión 35).
/// </summary>
public static class FormatoCodigoTicket
{
    public const int NumeroMaximo = 999999;

    public static string Formatear(int numero)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numero, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numero, NumeroMaximo);

        return $"HD-{numero:D6}";
    }
}
