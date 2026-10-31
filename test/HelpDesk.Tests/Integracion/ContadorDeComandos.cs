using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Cuenta las consultas que EF Core envía a la base, para comprobar que una lectura no hace una por cada fila.
/// </summary>
public sealed class ContadorDeComandos : DbCommandInterceptor
{
    public int Consultas { get; private set; }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Consultas++;
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
