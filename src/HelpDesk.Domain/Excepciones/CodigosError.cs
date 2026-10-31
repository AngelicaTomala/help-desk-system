namespace HelpDesk.Domain.Excepciones;

/// <summary>
/// Valores del catálogo de <c>codigo</c> que producen las excepciones de dominio (ADR-004, decisiones 40 y 80).
/// </summary>
public static class CodigosError
{
    public const string TransicionInvalida = "TRANSICION_INVALIDA";
    public const string TicketNoModificable = "TICKET_NO_MODIFICABLE";
    public const string ErrorInterno = "ERROR_INTERNO";
}
