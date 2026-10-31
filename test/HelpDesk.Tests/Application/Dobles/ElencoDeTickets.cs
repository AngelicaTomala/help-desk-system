using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Tests.Application.Dobles;

/// <summary>
/// Elenco de tickets de los ejemplos del contrato (§1.10), llevado a cada estado con los métodos de Domain. El instante
/// de referencia es <see cref="Ahora"/>.
/// </summary>
internal static class ElencoDeTickets
{
    public const int AnaId = 1;
    public const int BrunoId = 2;
    public const int CarlaId = 3;
    public const int DiegoId = 4;
    public const int FabianId = 6;
    public const int GabrielaId = 7;
    public const int JorgeId = 10;

    public static readonly DateTime Ahora = new(2026, 10, 28, 14, 30, 0, DateTimeKind.Utc);

    private static readonly DateTime Dia27 = new(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Dia28 = new(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Siembra los tickets 11 a 16 en el repositorio falso.
    /// </summary>
    public static void SembrarTodos(TicketRepositorioFalso tickets)
    {
        // 11 · Abierto, Ana, Critica, 08:00 → vence 10:00: Vencido.
        tickets.Sembrar(
            Ticket.Crear("HD-000011", "No abre el sistema de nómina", "El sistema de nómina no abre al iniciar sesión.",
                Categoria.Software, Prioridad.Critica, AnaId, Dia28.AddHours(8)),
            11);

        // 12 · Asignado a Fabián, Bruno, Media, 09:00 → vence 17:00: EnTiempo.
        var ticket12 = Ticket.Crear("HD-000012", "Monitor sin imagen",
            "El monitor del puesto 4 no muestra imagen al encender.", Categoria.Hardware, Prioridad.Media, BrunoId,
            Dia28.AddHours(9));
        ticket12.Asignar(FabianId, Dia28.AddHours(9.5));
        tickets.Sembrar(ticket12, 12);

        // 13 · EnProgreso con Gabriela, Carla, Alta, 08:00 → vence 12:00: Vencido.
        var ticket13 = Ticket.Crear("HD-000013", "Sin acceso a la red Wi-Fi",
            "La laptop no se conecta a la red Wi-Fi de la oficina.", Categoria.Red, Prioridad.Alta, CarlaId,
            Dia28.AddHours(8));
        ticket13.Asignar(GabrielaId, Dia28.AddHours(8.5));
        ticket13.IniciarAtencion(Dia28.AddHours(10));
        tickets.Sembrar(ticket13, 13);

        // 14 · Resuelto por Fabián, Ana, Media, 27 10:00 → vence 18:00, resuelto 16:00: Cumplido.
        var ticket14 = Ticket.Crear("HD-000014", "Restablecer contraseña de correo",
            "No recuerdo la contraseña de mi cuenta de correo.", Categoria.Accesos, Prioridad.Media, AnaId,
            Dia27.AddHours(10));
        ticket14.Asignar(FabianId, Dia27.AddHours(11));
        ticket14.IniciarAtencion(Dia27.AddHours(12));
        ticket14.Resolver("Se restableció la contraseña y se envió el enlace de acceso.", FabianId, Dia27.AddHours(16));
        tickets.Sembrar(ticket14, 14);

        // 15 · Cerrado, Diego, Baja, 26 09:00 → vence 27 09:00, resuelto 27 11:00: Incumplido.
        var ticket15 = Ticket.Crear("HD-000015", "Error al imprimir facturas",
            "La impresora devuelve un error al imprimir facturas.", Categoria.Software, Prioridad.Baja, DiegoId,
            Dia27.AddHours(-15));
        ticket15.Asignar(FabianId, Dia27.AddHours(-14));
        ticket15.IniciarAtencion(Dia27.AddHours(-13));
        ticket15.Resolver("Se reinició el servicio de impresión.", FabianId, Dia27.AddHours(11));
        ticket15.Cerrar(Dia27.AddHours(15));
        tickets.Sembrar(ticket15, 15);

        // 16 · Abierto, Ana, Alta, 14:30 → vence 18:30: EnTiempo.
        tickets.Sembrar(
            Ticket.Crear("HD-000016", "No puedo instalar el antivirus",
                "El instalador muestra un error de permisos al iniciar.", Categoria.Software, Prioridad.Alta, AnaId,
                Ahora),
            16);
    }
}
