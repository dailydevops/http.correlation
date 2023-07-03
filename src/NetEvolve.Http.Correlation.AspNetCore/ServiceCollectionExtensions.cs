namespace NetEvolve.Http.Correlation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetEvolve.Http.Correlation.Abstractions;
using System;

/// <summary>
/// <see cref="IServiceCollection"/> Extensions for <see cref="HttpCorrelationMiddleware"/>.
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

        if (services.BuildServiceProvider().GetService<IHttpCorrelationAccessor>() is not null)
        {
            throw new InvalidOperationException(
                "Services have already been added. Please check your service registration for duplicates."
            );
        }

        services
            .AddHttpContextAccessor()
            .TryAddSingleton<IHttpCorrelationAccessor, HttpCorrelationAccessor>();

        return new HttpCorrelationBuilder(services);
    }
}
