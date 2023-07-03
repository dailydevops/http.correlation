namespace NetEvolve.Http.Correlation;

using Microsoft.Extensions.DependencyInjection;

internal sealed class HttpCorrelationBuilder : IHttpCorrelationBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    public HttpCorrelationBuilder(IServiceCollection services) => Services = services;
}
