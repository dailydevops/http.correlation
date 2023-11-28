namespace NetEvolve.Http.Correlation;

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation;
using NetEvolve.Http.Correlation.Abstractions;

/// <summary>
/// <see cref="IApplicationBuilder"/> Extensions for <see cref="HttpCorrelationMiddleware"/>.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="HttpCorrelationMiddleware"/> to the application's request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseHttpCorrelation(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using (var scopedServices = app.ApplicationServices.CreateScope())
        {
            if (scopedServices.ServiceProvider.GetService<IHttpCorrelationAccessor>() is null)
            {
                throw new InvalidOperationException(
                    $"The required services for this function were not found. Please run `services.{nameof(ServiceCollectionExtensions.AddHttpCorrelation)}()` in advance."
                );
            }
        }

        return app.UseMiddleware<HttpCorrelationMiddleware>();
    }
}
