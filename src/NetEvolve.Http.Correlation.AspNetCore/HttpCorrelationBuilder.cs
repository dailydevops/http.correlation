namespace NetEvolve.Http.Correlation.Abstractions;

using Microsoft.Extensions.DependencyInjection;

internal sealed class HttpCorrelationBuilder : IHttpCorrelationBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    public HttpCorrelationBuilder(IServiceCollection services) => Services = services;
}
