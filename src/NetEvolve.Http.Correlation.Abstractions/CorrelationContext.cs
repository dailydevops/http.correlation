namespace NetEvolve.Http.Correlation;

using System.Threading;

/// <summary>
/// Ambient correlation of the request currently in flight, published by the incoming middleware and read by
/// outgoing handlers.
/// </summary>
/// <remarks>
/// <c>IHttpClientFactory</c> builds and caches message handlers in its own dependency injection scope, so a
/// handler must not capture request scoped services. The value flows with the <see cref="ExecutionContext"/>
/// instead: set inside the middleware, it is visible to everything the request awaits and invisible once the
/// middleware returned.
/// </remarks>
internal static class CorrelationContext
{
    private static readonly AsyncLocal<CorrelationSnapshot?> _current = new();

    /// <summary>
    /// Gets the correlation of the request currently in flight, or <see langword="null"/> if there is none.
    /// </summary>
    public static CorrelationSnapshot? Current => _current.Value;

    /// <summary>
    /// Publishes the correlation for the current asynchronous flow.
    /// </summary>
    /// <param name="snapshot">The correlation of the current request, or <see langword="null"/> to clear it.</param>
    public static void Set(CorrelationSnapshot? snapshot) => _current.Value = snapshot;
}
