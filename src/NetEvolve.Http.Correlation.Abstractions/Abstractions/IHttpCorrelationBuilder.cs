namespace NetEvolve.Http.Correlation.Abstractions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Specifies the contract for the Http Correlation Builder.
/// </summary>
public interface IHttpCorrelationBuilder
{
    /// <summary>
    /// Specifies the contract for a collection of service descriptors.
    /// </summary>
    IServiceCollection Services { get; }
}
