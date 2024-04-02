namespace NetEvolve.Http.Correlation.Generators;

using Microsoft.Extensions.Options;
using NetEvolve.Http.Correlation.Abstractions;
using NetEvolve.SequentialGuid;

/// <inheritdoc cref="IHttpCorrelationIdProvider" />
internal sealed class SequentialGuidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    private readonly IOptionsMonitor<SequentialGuidOptions> _options;

    public SequentialGuidCorrelationIdProvider(IOptionsMonitor<SequentialGuidOptions> options) =>
        _options = options;

    /// <inheritdoc />
    public string GenerateId() =>
        SequentialGuidFactory.NewGuid(_options.CurrentValue.SequentialType).ToString("N");
}
