namespace NetEvolve.Http.Correlation;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetEvolve.Http.Correlation.Abstractions;
using NetEvolve.Http.Correlation.Generators;

/// <summary>
/// <see cref="IHttpCorrelationBuilder"/> Extensions.
/// </summary>
public static class HttpCorrelationBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="GuidCorrelationIdProvider"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpCorrelationBuilder"/> instance.</param>
    /// <returns>The <see cref="IHttpCorrelationBuilder"/> instance.</returns>
    public static IHttpCorrelationBuilder WithGuidGenerator(this IHttpCorrelationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .Services.RemoveAll<IHttpCorrelationIdProvider>()
            .TryAddSingleton<IHttpCorrelationIdProvider, GuidCorrelationIdProvider>();

        return builder;
    }

    /// <summary>
    /// Adds a <see cref="SequentialGuidCorrelationIdProvider"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpCorrelationBuilder"/> instance.</param>
    /// <param name="options">Optional parameter for selecting the sequential <see cref="Guid"/> creation mode.</param>
    /// <returns>The <see cref="IHttpCorrelationBuilder"/> instance.</returns>
    public static IHttpCorrelationBuilder WithSequentialGuidGenerator(
        this IHttpCorrelationBuilder builder,
        Action<SequentialGuidOptions>? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .Services.RemoveAll<IHttpCorrelationIdProvider>()
            .ConfigureOptions<SequentialGuidOptions>()
            .TryAddSingleton<IHttpCorrelationIdProvider, SequentialGuidCorrelationIdProvider>();

        if (options is not null)
        {
            _ = builder.Services.Configure(options);
        }

        return builder;
    }
}
