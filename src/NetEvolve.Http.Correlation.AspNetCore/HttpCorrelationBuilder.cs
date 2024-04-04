namespace NetEvolve.Http.Correlation;

using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;

internal sealed class HttpCorrelationBuilder : IHttpCorrelationBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    public HttpCorrelationBuilder(IServiceCollection services) => Services = services;
}
