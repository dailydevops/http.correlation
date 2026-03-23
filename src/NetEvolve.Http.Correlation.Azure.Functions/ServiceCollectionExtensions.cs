namespace NetEvolve.Http.Correlation.Azure.Functions;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetEvolve.Http.Correlation.Abstractions;

/// <summary>
/// <see cref="IServiceCollection"/> Extensions for <see cref="FunctionsCorrelationMiddleware"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enables the Http Correlation services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <returns>An <see cref="IHttpCorrelationBuilder"/> instance.</returns>
    public static IHttpCorrelationBuilder AddHttpCorrelation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddScoped<IHttpCorrelationAccessor>(sp => sp.GetRequiredService<FunctionsCorrelationAccessor>())
            .TryAddScoped<FunctionsCorrelationAccessor>();

        return new HttpCorrelationBuilder(services);
    }
}
