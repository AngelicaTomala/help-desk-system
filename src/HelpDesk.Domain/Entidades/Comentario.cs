using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Domain.Entidades;

/// <summary>
/// Comentario de solución o de reapertura. Solo lo crean <see cref="Ticket.Resolver"/> y <see cref="Ticket.Reabrir"/> (RN-09).
/// </summary>
public class Comentario
{
    public const int TextoLongitudMaxima = 2000;

    internal Comentario(TipoComentario tipo, string texto, int autorId, DateTime fechaCreacion)
    {
        Tipo = tipo;
        Texto = texto;
        AutorId = autorId;
        FechaCreacion = fechaCreacion;
    }

    private Comentario()
    {
    }

    public int Id { get; private set; }

    public int TicketId { get; private set; }

    public int AutorId { get; private set; }

    public TipoComentario Tipo { get; private set; }

    public string Texto { get; private set; } = string.Empty;

    public DateTime FechaCreacion { get; private set; }
}
