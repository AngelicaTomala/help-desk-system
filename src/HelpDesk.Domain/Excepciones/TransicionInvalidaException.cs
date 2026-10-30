namespace HelpDesk.Domain.Excepciones;

/// <summary>
/// Regla de estado violada: transición no permitida por RN-08 o ticket no modificable (RN-12, RN-13).
/// </summary>
public sealed class TransicionInvalidaException : Exception
{
    public TransicionInvalidaException(string codigo, string mensaje)
        : base(mensaje)
    {
        Codigo = codigo;
    }

    public string Codigo { get; }
}
