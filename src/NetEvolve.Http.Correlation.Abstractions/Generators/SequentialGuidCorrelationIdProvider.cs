namespace NetEvolve.Http.Correlation.Generators;

using System;
using Microsoft.Extensions.Options;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc cref="IHttpCorrelationIdProvider" />
internal sealed class SequentialGuidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    private static readonly Random _random = Random.Shared;
    private readonly IOptionsMonitor<SequentialGuidOptions> _options;

    public SequentialGuidCorrelationIdProvider(IOptionsMonitor<SequentialGuidOptions> options) =>
        _options = options;

    /// <inheritdoc />
    public string GenerateId()
    {
        var timeStamp = DateTime.UtcNow.Ticks / 10000L;
        var timeStampBytes = BitConverter.GetBytes(timeStamp).AsSpan();

        if (BitConverter.IsLittleEndian)
        {
            timeStampBytes.Reverse();
        }

        Span<byte> guidBytes = stackalloc byte[16];
#pragma warning disable CA5394 // Do not use insecure randomness
        _random.NextBytes(guidBytes);
#pragma warning restore CA5394 // Do not use insecure randomness

        var option = _options.CurrentValue;

        switch (option.SequentialType)
        {
            case SequentialType.AsBinary:
            case SequentialType.AsString:
                timeStampBytes[2..8].CopyTo(guidBytes[..6]);

                if (option.SequentialType == SequentialType.AsString && BitConverter.IsLittleEndian)
                {
                    guidBytes[..4].Reverse();
                    guidBytes[4..6].Reverse();
                }

                break;
            case SequentialType.AtEnd:
                timeStampBytes[2..8].CopyTo(guidBytes[10..16]);
                break;
        }

        return new Guid(guidBytes).ToString("N");
    }
}
