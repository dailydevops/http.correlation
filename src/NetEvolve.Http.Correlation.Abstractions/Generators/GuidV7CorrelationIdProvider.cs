#if NET9_0_OR_GREATER
namespace NetEvolve.Http.Correlation.Generators;

using System;
using NetEvolve.Http.Correlation.Abstractions;

internal sealed class GuidV7CorrelationIdProvider : IHttpCorrelationIdProvider
{
    /// <inheritdoc cref="IHttpCorrelationIdProvider.GenerateId" />
    public string GenerateId() => Guid.CreateVersion7().ToString("N");
}
#endif
