using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Cuenta las consultas que EF Core envía a la base, para comprobar que una lectura no hace una por cada fila, y
/// guarda su texto y los valores de sus parámetros, para comprobar qué se resuelve en SQL Server.
/// </summary>
public sealed class ContadorDeComandos : DbCommandInterceptor
{
    private readonly List<ComandoEjecutado> _comandos = [];

    public int Consultas { get; private set; }

    public IReadOnlyList<ComandoEjecutado> Comandos => _comandos;

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Consultas++;
        _comandos.Add(new ComandoEjecutado(
            command.CommandText,
            command.Parameters.Cast<DbParameter>().Select(p => p.Value).ToList()));
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}

public sealed record ComandoEjecutado(string Texto, IReadOnlyList<object?> Valores);
