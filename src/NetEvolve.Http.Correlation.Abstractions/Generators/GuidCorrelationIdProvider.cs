namespace NetEvolve.Http.Correlation.Generators;

using System;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc cref="IHttpCorrelationIdProvider" />
internal sealed class GuidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    /// <inheritdoc cref="IHttpCorrelationIdProvider.GenerateId" />
    public string GenerateId() => Guid.NewGuid().ToString("N");
}
