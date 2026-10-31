using FluentValidation;
using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Excepciones;
using HelpDesk.Application.Identificadores;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets.Listar;

/// <summary>
/// Lista tickets (RF-02, RN-04, RN-05). Orden de la decisión 92: permiso (<c>403</c> <c>FILTRO_NO_AUTORIZADO</c>,
/// decisión 100) y validador (<c>400</c>); no hay recurso de la ruta ni Domain. Después aplica la visibilidad de RN-04.
/// </summary>
public class ListarTicketsHandler(
    IUsuarioActual usuarioActual,
    IValidator<ListarTicketsConsulta> validador,
    IConsultasTicket consultas,
    TimeProvider timeProvider)
{
    public async Task<PaginaDto<TicketResumenDto>> ManejarAsync(
        ListarTicketsConsulta consulta,
        CancellationToken cancellationToken)
    {
        // Un técnico solo puede filtrar por su propio Id: cualquier otro valor, incluido uno mal formado o vacío,
        // es 403 sin consultar la base. Repetido con todos los valores iguales a su Id llega al validador (decisión 100).
        if (usuarioActual.Rol == Rol.Tecnico
            && ListarTicketsConsulta.EstaPresente(consulta.TecnicoId)
            && consulta.TecnicoId!.Any(valor => !FormatoId.TryLeer(valor, out var id) || id != usuarioActual.Id))
        {
            throw new AccesoDenegadoException(
                CodigosErrorAplicacion.FiltroNoAutorizado,
                "Un técnico solo puede filtrar por su propio Id.");
        }

        var resultado = await validador.ValidateAsync(consulta, cancellationToken);
        if (!resultado.IsValid)
        {
            throw SolicitudInvalidaException.DesdeValidacion(resultado);
        }

        var filtro = AplicarVisibilidad(new FiltroListadoTickets(
            SolicitanteId: null,
            TecnicoId: LeerId(consulta.TecnicoId),
            Estado: LeerEnum<EstadoTicket>(consulta.Estado),
            Prioridad: LeerEnum<Prioridad>(consulta.Prioridad),
            SoloVencidos: string.Equals(Valor(consulta.SoloVencidos), bool.TrueString, StringComparison.OrdinalIgnoreCase),
            Page: LeerId(consulta.Page) ?? Paginacion.PaginaPorDefecto,
            PageSize: LeerId(consulta.PageSize) ?? Paginacion.TamanoPorDefecto));

        // Una sola lectura del reloj por operación: filtra soloVencidos y calcula estadoSla (RNF-08, decisión 76).
        var ahora = timeProvider.GetUtcNow().UtcDateTime;

        return await consultas.ListarAsync(filtro, ahora, cancellationToken);
    }

    /// <summary>
    /// RN-04: el solicitante solo ve los suyos, aun con el filtro <c>tecnicoId</c> (decisión 51); el técnico, los
    /// asignados a él; el supervisor, todos. Un <c>tecnicoId</c> que no es de un técnico da una lista vacía (decisión 69).
    /// </summary>
    private FiltroListadoTickets AplicarVisibilidad(FiltroListadoTickets filtro) =>
        usuarioActual.Rol switch
        {
            Rol.Solicitante => filtro with { SolicitanteId = usuarioActual.Id },
            Rol.Tecnico => filtro with { TecnicoId = usuarioActual.Id },
            _ => filtro,
        };

    private static string? Valor(IReadOnlyList<string>? valores) =>
        ListarTicketsConsulta.EstaPresente(valores) ? valores![0] : null;

    private static int? LeerId(IReadOnlyList<string>? valores) =>
        FormatoId.TryLeer(Valor(valores), out var id) ? id : null;

    private static TEnum? LeerEnum<TEnum>(IReadOnlyList<string>? valores)
        where TEnum : struct, Enum =>
        ValoresEnum.TryParse<TEnum>(Valor(valores), out var valor) ? valor : null;
}
