namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Integration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

/// <summary>
/// Records the state of every <see cref="ILogger.BeginScope{TState}(TState)"/> call made by the correlation middleware.
/// </summary>
internal sealed class ScopeCapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<IReadOnlyDictionary<string, object>> _scopes = new();

    public IReadOnlyList<IReadOnlyDictionary<string, object>> Scopes => [.. _scopes];

    public ILogger CreateLogger(string categoryName) =>
        categoryName.EndsWith("CorrelationMiddleware", StringComparison.Ordinal)
            ? new CapturingLogger(_scopes)
            : Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

    public void Dispose() { }

    private sealed class CapturingLogger(ConcurrentQueue<IReadOnlyDictionary<string, object>> scopes) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            if (state is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                scopes.Enqueue(pairs.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal));
            }

            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) { }
    }
}
