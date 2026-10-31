namespace HelpDesk.Domain.Excepciones;

/// <summary>
/// Invariante de la entidad violada (RN-01, RN-02, RN-09 y RN-10). Si llega a la API indica que falta un validador.
/// </summary>
public sealed class InvarianteVioladaException : Exception
{
    public InvarianteVioladaException(string mensaje)
        : base(mensaje)
    {
    }

    public string Codigo => CodigosError.ErrorInterno;
}
