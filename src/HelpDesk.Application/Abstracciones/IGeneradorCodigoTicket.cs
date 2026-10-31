namespace HelpDesk.Application.Abstracciones;

/// <summary>
/// Genera el código único y creciente de un ticket nuevo, con formato <c>HD-</c> + 6 dígitos
/// (RN-03, decisiones 13 y 35). Se obtiene antes de llamar a <c>Ticket.Crear</c> (decisión 78).
/// </summary>
public interface IGeneradorCodigoTicket
{
    Task<string> GenerarAsync(CancellationToken cancellationToken);
}
