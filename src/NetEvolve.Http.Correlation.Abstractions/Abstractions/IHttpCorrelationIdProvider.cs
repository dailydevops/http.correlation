namespace NetEvolve.Http.Correlation.Abstractions;

/// <summary>
/// Contract for an Http Correlation Id Provider.
/// </summary>
public interface IHttpCorrelationIdProvider
{
    /// <summary>
    /// Generates a Http Correlation Id.
    /// </summary>
    /// <returns>Http Correlation Id.</returns>
    string GenerateId();
}
