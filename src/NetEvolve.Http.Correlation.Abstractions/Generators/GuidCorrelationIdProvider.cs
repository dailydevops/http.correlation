namespace NetEvolve.Http.Correlation.Generators;

using System;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc />
internal sealed class GuidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    /// <inheritdoc />
    public string GenerateId() => Guid.NewGuid().ToString("N");
}
