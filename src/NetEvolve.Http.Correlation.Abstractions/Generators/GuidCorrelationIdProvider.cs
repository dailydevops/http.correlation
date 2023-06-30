namespace NetEvolve.Http.Correlation.Generators;

using NetEvolve.Http.Correlation.Abstractions;
using System;

/// <inheritdoc />
internal sealed class GuidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    /// <inheritdoc />
    public string GenerateId() => Guid.NewGuid().ToString("N");
}
