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
    /// <returns>Correlation Id</returns>
    string GetCorrelationId();
}
