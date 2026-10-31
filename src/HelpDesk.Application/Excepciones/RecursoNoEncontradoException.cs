namespace HelpDesk.Application.Excepciones;

/// <summary>
/// El recurso de la ruta no existe (<c>404</c>, decisión 98). Solo aplica al ticket de la ruta (decisión 4).
/// </summary>
public sealed class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje)
        : base(mensaje)
    {
    }

    public string Codigo => CodigosErrorAplicacion.TicketNoEncontrado;
}
