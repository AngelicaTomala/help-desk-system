using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;

namespace HelpDesk.Domain.Entidades;

/// <summary>
/// Raíz de agregado. Protege las invariantes de RN-01, RN-02, RN-09 y RN-10 y las reglas de estado de RN-07, RN-08, RN-12 y RN-13.
/// La hora llega siempre en el parámetro <c>ahora</c> (UTC); Domain no lee el reloj (decisión 76).
/// </summary>
public class Ticket
{
    public const int TituloLongitudMinima = 5;
    public const int TituloLongitudMaxima = 150;
    public const int DescripcionLongitudMinima = 10;
    public const int DescripcionLongitudMaxima = 2000;

    private readonly List<Comentario> _comentarios = [];

    private Ticket()
    {
    }

    public int Id { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public string Titulo { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public Categoria Categoria { get; private set; }

    public Prioridad Prioridad { get; private set; }

    public EstadoTicket Estado { get; private set; }

    public int SolicitanteId { get; private set; }

    public int? TecnicoId { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public DateTime FechaActualizacion { get; private set; }

    public DateTime FechaVencimiento { get; private set; }

    public DateTime? FechaResolucion { get; private set; }

    public DateTime? FechaCierre { get; private set; }

    public IReadOnlyCollection<Comentario> Comentarios => _comentarios.AsReadOnly();

    /// <summary>
    /// Crea un ticket <c>Abierto</c> y sin técnico (RN-03). No valida el formato de <paramref name="codigo"/> (decisión 78).
    /// </summary>
    public static Ticket Crear(
        string codigo,
        string titulo,
        string descripcion,
        Categoria categoria,
        Prioridad prioridad,
        int solicitanteId,
        DateTime ahora)
    {
        ValidarTexto(titulo, "título", TituloLongitudMinima, TituloLongitudMaxima);
        ValidarTexto(descripcion, "descripción", DescripcionLongitudMinima, DescripcionLongitudMaxima);
        ValidarCategoria(categoria);
        ValidarPrioridad(prioridad);

        return new Ticket
        {
            Codigo = codigo,
            Titulo = titulo,
            Descripcion = descripcion,
            Categoria = categoria,
            Prioridad = prioridad,
            Estado = EstadoTicket.Abierto,
            SolicitanteId = solicitanteId,
            FechaCreacion = ahora,
            FechaActualizacion = ahora,
            FechaVencimiento = ahora.AddHours(PoliticaSla.HorasPara(prioridad)),
        };
    }

    public void Asignar(int tecnicoId, DateTime ahora)
    {
        ExigirEstado("asignar", EstadoTicket.Abierto);

        TecnicoId = tecnicoId;
        Estado = EstadoTicket.Asignado;
        FechaActualizacion = ahora;
    }

    /// <summary>
    /// Reasignar al mismo técnico no se rechaza aquí: <c>MISMO_TECNICO</c> se valida en Application (decisión 81).
    /// </summary>
    public void Reasignar(int tecnicoId, DateTime ahora)
    {
        ExigirEstado("reasignar", EstadoTicket.Asignado, EstadoTicket.EnProgreso);

        TecnicoId = tecnicoId;
        Estado = EstadoTicket.Asignado;
        FechaActualizacion = ahora;
    }

    public void IniciarAtencion(DateTime ahora)
    {
        ExigirEstado("iniciar la atención de", EstadoTicket.Asignado);

        Estado = EstadoTicket.EnProgreso;
        FechaActualizacion = ahora;
    }

    public void Resolver(string solucion, int autorId, DateTime ahora)
    {
        ValidarTexto(solucion, "solución", 1, Comentario.TextoLongitudMaxima);
        ExigirEstado("resolver", EstadoTicket.EnProgreso);

        Estado = EstadoTicket.Resuelto;
        FechaResolucion = ahora;
        FechaActualizacion = ahora;
        _comentarios.Add(new Comentario(TipoComentario.Solucion, solucion, autorId, ahora));
    }

    public void Cerrar(DateTime ahora)
    {
        ExigirEstado("cerrar", EstadoTicket.Resuelto);

        Estado = EstadoTicket.Cerrado;
        FechaCierre = ahora;
        FechaActualizacion = ahora;
    }

    /// <summary>
    /// Vuelve a <c>EnProgreso</c> con el mismo técnico y conserva el vencimiento original (RN-09).
    /// </summary>
    public void Reabrir(string motivo, int autorId, DateTime ahora)
    {
        ValidarTexto(motivo, "motivo", 1, Comentario.TextoLongitudMaxima);
        ExigirEstado("reabrir", EstadoTicket.Resuelto);

        Estado = EstadoTicket.EnProgreso;
        FechaResolucion = null;
        FechaActualizacion = ahora;
        _comentarios.Add(new Comentario(TipoComentario.Reapertura, motivo, autorId, ahora));
    }

    /// <summary>
    /// Recalcula el vencimiento desde la fecha de creación (RN-12). Con la prioridad actual no cambia nada (decisión 67).
    /// </summary>
    public void CambiarPrioridad(Prioridad prioridad, DateTime ahora)
    {
        ValidarPrioridad(prioridad);
        ExigirAbiertoParaCambios("cambiar la prioridad de");

        if (prioridad == Prioridad)
        {
            return;
        }

        Prioridad = prioridad;
        FechaVencimiento = FechaCreacion.AddHours(PoliticaSla.HorasPara(prioridad));
        FechaActualizacion = ahora;
    }

    /// <summary>
    /// Con la categoría actual no cambia nada (decisión 67).
    /// </summary>
    public void Reclasificar(Categoria categoria, DateTime ahora)
    {
        ValidarCategoria(categoria);
        ExigirAbiertoParaCambios("reclasificar");

        if (categoria == Categoria)
        {
            return;
        }

        Categoria = categoria;
        FechaActualizacion = ahora;
    }

    /// <summary>
    /// Estado del SLA calculado, no persistido (RN-11). "Resuelto" incluye <c>Resuelto</c> y <c>Cerrado</c>.
    /// </summary>
    public EstadoSla CalcularEstadoSla(DateTime ahora)
    {
        if (Estado is EstadoTicket.Resuelto or EstadoTicket.Cerrado && FechaResolucion is { } resolucion)
        {
            return resolucion <= FechaVencimiento ? EstadoSla.Cumplido : EstadoSla.Incumplido;
        }

        return ahora <= FechaVencimiento ? EstadoSla.EnTiempo : EstadoSla.Vencido;
    }

    private bool EstaAbiertoParaCambios() => Estado is not (EstadoTicket.Resuelto or EstadoTicket.Cerrado);

    private void ExigirAbiertoParaCambios(string accion)
    {
        if (!EstaAbiertoParaCambios())
        {
            throw new TransicionInvalidaException(
                CodigosError.TicketNoModificable,
                $"No se puede {accion} un ticket en estado {Estado}.");
        }
    }

    private void ExigirEstado(string accion, params EstadoTicket[] permitidos)
    {
        if (!permitidos.Contains(Estado))
        {
            throw new TransicionInvalidaException(
                CodigosError.TransicionInvalida,
                $"No se puede {accion} un ticket en estado {Estado}.");
        }
    }

    /// <summary>
    /// Exige el texto ya recortado (decisiones 79 y 83): "espacio" es cualquier carácter que quita <see cref="string.Trim()"/>.
    /// </summary>
    private static void ValidarTexto(string? texto, string campo, int longitudMinima, int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new InvarianteVioladaException($"El campo {campo} está en blanco.");
        }

        if (texto.Length != texto.Trim().Length)
        {
            throw new InvarianteVioladaException($"El campo {campo} tiene espacios al inicio o al final.");
        }

        if (texto.Length < longitudMinima || texto.Length > longitudMaxima)
        {
            throw new InvarianteVioladaException(
                $"El campo {campo} debe tener entre {longitudMinima} y {longitudMaxima} caracteres.");
        }
    }

    private static void ValidarCategoria(Categoria categoria)
    {
        if (!Enum.IsDefined(categoria))
        {
            throw new InvarianteVioladaException($"La categoría {(int)categoria} no está definida.");
        }
    }

    private static void ValidarPrioridad(Prioridad prioridad)
    {
        if (!Enum.IsDefined(prioridad))
        {
            throw new InvarianteVioladaException($"La prioridad {(int)prioridad} no está definida.");
        }
    }
}
