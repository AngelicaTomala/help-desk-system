namespace HelpDesk.Application.Tickets.Listar;

/// <summary>
/// Límites de la paginación (RN-05, decisiones 9 y 34): viven en Application, junto a <see cref="ListarTicketsValidator"/>.
/// </summary>
public static class Paginacion
{
    public const int PaginaMinima = 1;
    public const int PaginaPorDefecto = 1;
    public const int TamanoMinimo = 1;
    public const int TamanoMaximo = 100;
    public const int TamanoPorDefecto = 20;
}
