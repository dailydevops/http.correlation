namespace NetEvolve.Http.Correlation;

using Microsoft.Extensions.DependencyInjection.Extensions;
using NetEvolve.Http.Correlation.Abstractions;
using System;

/// <summary>
/// <see cref="IHttpCorrelationBuilder"/> Extensions.
/// </summary>
public static class HttpCorrelationBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="TestGeneratorCorrelationIdProvider"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpCorrelationBuilder"/> instance.</param>
    /// <param name="generatedTestId"></param>
    /// <returns>The <see cref="IHttpCorrelationBuilder"/> instance.</returns>
    public static IHttpCorrelationBuilder WithTestGenerator(
        this IHttpCorrelationBuilder builder,
        string? generatedTestId = default
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .RemoveAll<IHttpCorrelationIdProvider>()
            .TryAddSingleton<IHttpCorrelationIdProvider>(
                new TestGeneratorCorrelationIdProvider(generatedTestId)
            );

        return builder;
    }
}
