namespace NetEvolve.Http.Correlation;

using System;
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

#if NET9_0_OR_GREATER
    /// <summary>
    /// Adds a <see cref="GuidV7CorrelationIdProvider"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpCorrelationBuilder"/> instance.</param>
    /// <returns>The <see cref="IHttpCorrelationBuilder"/> instance.</returns>
    public static IHttpCorrelationBuilder WithGuidV7Generator(this IHttpCorrelationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .Services.RemoveAll<IHttpCorrelationIdProvider>()
            .TryAddSingleton<IHttpCorrelationIdProvider, GuidV7CorrelationIdProvider>();

        return builder;
    }
#endif
}
