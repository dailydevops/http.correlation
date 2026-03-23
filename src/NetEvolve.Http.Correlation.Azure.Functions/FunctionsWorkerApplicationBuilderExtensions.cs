namespace NetEvolve.Http.Correlation.Azure.Functions;

using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetEvolve.Http.Correlation.Abstractions;

/// <summary>
/// <see cref="IFunctionsWorkerApplicationBuilder"/> Extensions for <see cref="FunctionsCorrelationMiddleware"/>.
/// </summary>
public static class FunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="FunctionsCorrelationMiddleware"/> to the Azure Functions worker middleware pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IFunctionsWorkerApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="IFunctionsWorkerApplicationBuilder"/> instance.</returns>
    public static IFunctionsWorkerApplicationBuilder UseHttpCorrelation(this IFunctionsWorkerApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        using (var scopedServices = builder.Services.BuildServiceProvider().CreateScope())
        {
            if (scopedServices.ServiceProvider.GetService<IHttpCorrelationAccessor>() is null)
            {
                throw new InvalidOperationException(
                    $"The required services for this function were not found. Please run `services.{nameof(ServiceCollectionExtensions.AddHttpCorrelation)}()` in advance."
                );
            }
        }

        return builder.UseMiddleware<FunctionsCorrelationMiddleware>();
    }
}
