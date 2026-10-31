using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Tests.Domain;

/// <summary>
/// Crea tickets en cada estado recorriendo las transiciones de RN-08 con instantes fijos (UTC).
/// </summary>
internal static class TicketsDePrueba
{
    public const string Codigo = "HD-000001";
    public const string Titulo = "No abre el sistema";
    public const string Descripcion = "El sistema de nómina no abre al iniciar sesión.";
    public const string Solucion = "Se reinició el servicio de nómina.";
    public const string Motivo = "El problema volvió a ocurrir.";
    public const int SolicitanteId = 1;
    public const int TecnicoId = 6;
    public const int OtroTecnicoId = 7;

    /// <summary>Instante de creación de los tickets de prueba.</summary>
    public static readonly DateTime Creacion = new(2026, 10, 28, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>Instante en que se ejecuta la acción bajo prueba.</summary>
    public static readonly DateTime Ahora = new(2026, 10, 28, 14, 30, 0, DateTimeKind.Utc);

    public static Ticket Nuevo(Prioridad prioridad = Prioridad.Media, Categoria categoria = Categoria.Software) =>
        Ticket.Crear(Codigo, Titulo, Descripcion, categoria, prioridad, SolicitanteId, Creacion);

    public static Ticket EnEstado(EstadoTicket estado, Prioridad prioridad = Prioridad.Media)
    {
        var ticket = Nuevo(prioridad);
        if (estado == EstadoTicket.Abierto)
        {
            return ticket;
        }

        ticket.Asignar(TecnicoId, Creacion.AddMinutes(10));
        if (estado == EstadoTicket.Asignado)
        {
            return ticket;
        }

        ticket.IniciarAtencion(Creacion.AddMinutes(20));
        if (estado == EstadoTicket.EnProgreso)
        {
            return ticket;
        }

        ticket.Resolver(Solucion, TecnicoId, Creacion.AddMinutes(30));
        if (estado == EstadoTicket.Resuelto)
        {
            return ticket;
        }

        ticket.Cerrar(Creacion.AddMinutes(40));
        return ticket;
    }

    public static string Texto(int longitud) => new('a', longitud);
}
