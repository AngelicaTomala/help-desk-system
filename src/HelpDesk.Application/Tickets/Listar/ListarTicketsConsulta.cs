namespace HelpDesk.Application.Tickets.Listar;

/// <summary>
/// Parámetros de la URL del listado, como texto y sin convertir (decisión 101). Cada parámetro trae todos los valores
/// con que aparece en la URL: <c>null</c> (o una lista vacía) si está ausente, más de un valor si está repetido
/// (decisión 103) y <c>""</c> si está presente con valor vacío (decisión 104). Los nombres de las propiedades dan las
/// claves de <c>errors</c> en camelCase (decisión 61).
/// </summary>
public sealed record ListarTicketsConsulta(
    IReadOnlyList<string>? Estado = null,
    IReadOnlyList<string>? Prioridad = null,
    IReadOnlyList<string>? TecnicoId = null,
    IReadOnlyList<string>? SoloVencidos = null,
    IReadOnlyList<string>? Page = null,
    IReadOnlyList<string>? PageSize = null)
{
    public static bool EstaPresente(IReadOnlyList<string>? valores) => valores is { Count: > 0 };
}
