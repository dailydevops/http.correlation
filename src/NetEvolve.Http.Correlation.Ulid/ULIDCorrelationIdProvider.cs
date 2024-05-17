namespace NetEvolve.Http.Correlation;

using System;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc />
internal sealed class UlidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    /// <inheritdoc />
    public string GenerateId() => Ulid.NewUlid().ToString();
}
