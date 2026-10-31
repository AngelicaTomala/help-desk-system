using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HelpDesk.Tests.Api.Dobles;

/// <summary>
/// Guarda en memoria los registros del log de la Api para comprobarlos en las pruebas.
/// </summary>
public sealed class RegistroDeLogs : ILoggerProvider
{
    private readonly ConcurrentQueue<EntradaLog> _entradas = new();

    public IReadOnlyCollection<EntradaLog> Entradas => _entradas.ToArray();

    public ILogger CreateLogger(string categoryName) => new Registrador(categoryName, _entradas);

    public void Dispose()
    {
    }

    public sealed record EntradaLog(string Categoria, LogLevel Nivel, Exception? Excepcion, string Mensaje);

    private sealed class Registrador(string categoria, ConcurrentQueue<EntradaLog> entradas) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            entradas.Enqueue(new EntradaLog(categoria, logLevel, exception, formatter(state, exception)));
    }
}
