namespace HelpDesk.Application.Excepciones;

/// <summary>
/// El rol o el dueño no corresponden a la operación (<c>403</c>, decisión 98): <c>ROL_NO_AUTORIZADO</c>,
/// <c>TICKET_AJENO</c> o <c>FILTRO_NO_AUTORIZADO</c>.
/// </summary>
public sealed class AccesoDenegadoException : Exception
{
    public AccesoDenegadoException(string codigo, string mensaje)
        : base(mensaje)
    {
        Codigo = codigo;
    }

    public string Codigo { get; }
}
