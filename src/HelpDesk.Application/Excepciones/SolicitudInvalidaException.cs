using FluentValidation.Results;

namespace HelpDesk.Application.Excepciones;

/// <summary>
/// Dato de entrada inválido (<c>400</c>, decisión 98): <c>VALIDACION_FALLIDA</c> con <see cref="Errores"/> por campo,
/// <c>TECNICO_NO_VALIDO</c> o <c>MISMO_TECNICO</c>.
/// </summary>
public sealed class SolicitudInvalidaException : Exception
{
    public SolicitudInvalidaException(string codigo, string mensaje)
        : this(codigo, mensaje, new Dictionary<string, string[]>())
    {
    }

    private SolicitudInvalidaException(string codigo, string mensaje, IReadOnlyDictionary<string, string[]> errores)
        : base(mensaje)
    {
        Codigo = codigo;
        Errores = errores;
    }

    public string Codigo { get; }

    /// <summary>
    /// Mensajes por campo; las claves son los nombres de campo en camelCase (decisión 61).
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    /// <summary>
    /// <c>VALIDACION_FALLIDA</c> con todos los errores del validador agrupados por campo (RNF-05).
    /// </summary>
    public static SolicitudInvalidaException DesdeValidacion(ValidationResult resultado)
    {
        var errores = resultado.Errors
            .GroupBy(error => ACamelCase(error.PropertyName))
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(error => error.ErrorMessage).ToArray());

        return new SolicitudInvalidaException(
            CodigosErrorAplicacion.ValidacionFallida,
            "Uno o más datos de entrada no son válidos.",
            errores);
    }

    private static string ACamelCase(string nombre) =>
        string.IsNullOrEmpty(nombre) ? nombre : char.ToLowerInvariant(nombre[0]) + nombre[1..];
}
