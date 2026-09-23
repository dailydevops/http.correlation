namespace NetEvolve.Http.Correlation;

/// <summary>
/// Immutable correlation of one request: the resolved identifier and the header dialect it arrived with.
/// </summary>
/// <param name="CorrelationId">The correlation identifier of the request.</param>
/// <param name="HeaderName">The header name used by the request, see <see cref="CorrelationConstants"/>.</param>
internal sealed record CorrelationSnapshot(string CorrelationId, string HeaderName);
