namespace NetEvolve.Http.Correlation.Abstractions;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides Access to the current
/// </summary>
public interface IHttpCorrelationAccessor
{
    /// <summary>
    /// Gets the current Correlation Id from the <see cref="HttpContext"/>.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the header name, which is used.
    /// </summary>
    string? HeaderName { get; internal set; }
}
