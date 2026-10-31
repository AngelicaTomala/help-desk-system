using HelpDesk.Application.Abstracciones;

namespace HelpDesk.Tests.Application.Dobles;

internal sealed class GeneradorCodigoTicketFalso : IGeneradorCodigoTicket
{
    public const string Codigo = "HD-000001";

    /// <summary>Si tiene valor, <see cref="GenerarAsync"/> la lanza (p. ej., SEQUENCE agotada, decisión 90).</summary>
    public Exception? Fallo { get; init; }

    public int VecesInvocado { get; private set; }

    public Task<string> GenerarAsync(CancellationToken cancellationToken)
    {
        VecesInvocado++;
        return Fallo is null ? Task.FromResult(Codigo) : Task.FromException<string>(Fallo);
    }
}
