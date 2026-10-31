namespace HelpDesk.Application.Excepciones;

/// <summary>
/// Valores del catálogo de <c>codigo</c> que producen las excepciones de Application (ADR-004, decisión 98).
/// Los <c>401</c> los decide la Api.
/// </summary>
public static class CodigosErrorAplicacion
{
    public const string ValidacionFallida = "VALIDACION_FALLIDA";
    public const string TecnicoNoValido = "TECNICO_NO_VALIDO";
    public const string MismoTecnico = "MISMO_TECNICO";
    public const string RolNoAutorizado = "ROL_NO_AUTORIZADO";
    public const string TicketAjeno = "TICKET_AJENO";
    public const string FiltroNoAutorizado = "FILTRO_NO_AUTORIZADO";
    public const string TicketNoEncontrado = "TICKET_NO_ENCONTRADO";
}
