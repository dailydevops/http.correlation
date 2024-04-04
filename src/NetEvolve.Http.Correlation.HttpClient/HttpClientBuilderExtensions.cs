namespace NetEvolve.Http.Correlation;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// <see cref="IHttpClientBuilder"/> Extensions for <see cref="HttpCorrelationIdHandler"/>.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds and enabled a <see cref="HttpCorrelationIdHandler"/> to support correlation id forwarding.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> instance.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    public static IHttpClientBuilder AddHttpCorrelation(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddTransient<HttpCorrelationIdHandler>();

        return builder.AddHttpMessageHandler<HttpCorrelationIdHandler>();
    }
}
