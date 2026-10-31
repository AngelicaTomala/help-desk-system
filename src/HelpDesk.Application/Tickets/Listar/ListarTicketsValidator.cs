using System.Linq.Expressions;
using FluentValidation;
using HelpDesk.Application.Identificadores;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets.Listar;

/// <summary>
/// Parámetros de la URL del listado (RN-05, CA-02.4, CA-02.8, decisiones 101 a 105). Cada parámetro presente debe
/// aparecer una sola vez y no estar vacío; después se comprueba su formato y, en <c>page</c> y <c>pageSize</c>, el
/// rango de RN-05 con las constantes de <see cref="Paginacion"/>. Un parámetro ausente no se valida.
/// </summary>
public sealed class ListarTicketsValidator : AbstractValidator<ListarTicketsConsulta>
{
    public ListarTicketsValidator()
    {
        Parametro(consulta => consulta.Estado, "estado",
            (ValoresEnum.EsValido<EstadoTicket>, "El estado no es válido."));

        Parametro(consulta => consulta.Prioridad, "prioridad",
            (ValoresEnum.EsValido<Prioridad>, "La prioridad no es válida."));

        Parametro(consulta => consulta.TecnicoId, "tecnicoId",
            (EsId, "El tecnicoId debe ser un entero positivo."));

        // false equivale a no enviarlo (decisión 102).
        Parametro(consulta => consulta.SoloVencidos, "soloVencidos",
            (EsBooleano, "El parámetro soloVencidos debe ser true o false."));

        Parametro(consulta => consulta.Page, "page",
            (EsId, "El parámetro page debe ser un entero positivo."),
            (valor => LeerEntero(valor) >= Paginacion.PaginaMinima,
                $"El parámetro page debe ser mayor o igual que {Paginacion.PaginaMinima}."));

        Parametro(consulta => consulta.PageSize, "pageSize",
            (EsId, "El parámetro pageSize debe ser un entero positivo."),
            (valor => LeerEntero(valor) is >= Paginacion.TamanoMinimo and <= Paginacion.TamanoMaximo,
                $"El parámetro pageSize debe estar entre {Paginacion.TamanoMinimo} y {Paginacion.TamanoMaximo}."));
    }

    /// <summary>
    /// <c>true</c> o <c>false</c>, sin distinguir mayúsculas (decisión 102).
    /// </summary>
    public static bool EsBooleano(string valor) =>
        string.Equals(valor, bool.TrueString, StringComparison.OrdinalIgnoreCase)
        || string.Equals(valor, bool.FalseString, StringComparison.OrdinalIgnoreCase);

    private static bool EsId(string valor) => FormatoId.TryLeer(valor, out _);

    private static int LeerEntero(string valor) => FormatoId.TryLeer(valor, out var entero) ? entero : 0;

    /// <summary>
    /// Regla de un parámetro presente: un solo valor (decisión 103), no vacío (decisión 104) y después las reglas de
    /// formato y rango sobre ese valor, deteniéndose en el primer error.
    /// </summary>
    private void Parametro(
        Expression<Func<ListarTicketsConsulta, IReadOnlyList<string>?>> parametro,
        string nombre,
        params (Func<string, bool> Cumple, string Mensaje)[] reglas)
    {
        var leer = parametro.Compile();

        var regla = RuleFor(parametro)
            .Cascade(CascadeMode.Stop)
            .Must(valores => valores!.Count == 1).WithMessage($"El parámetro {nombre} está repetido.")
            .Must(valores => !string.IsNullOrEmpty(valores![0])).WithMessage($"El parámetro {nombre} está vacío.");

        foreach (var (cumple, mensaje) in reglas)
        {
            regla = regla.Must(valores => cumple(valores![0])).WithMessage(mensaje);
        }

        regla.When(consulta => ListarTicketsConsulta.EstaPresente(leer(consulta)));
    }
}
