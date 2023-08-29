namespace NetEvolve.Http.Correlation.Generators;

using Microsoft.Extensions.Options;
using NetEvolve.Http.Correlation.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;

/// <inheritdoc cref="IHttpCorrelationIdProvider" />
internal sealed class SequentialGuidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    private static readonly Random _random = Random.Shared;
    private readonly IOptionsMonitor<SequentialGuidOptions> _options;

    public SequentialGuidCorrelationIdProvider(IOptionsMonitor<SequentialGuidOptions> options)
        => _options = options;

    /// <inheritdoc />
    [SuppressMessage(
        "Security",
        "CA5394:Do not use insecure randomness",
        Justification = "As designed."
    )]
    public string GenerateId()
    {
        var timeStamp = DateTime.UtcNow.Ticks / 10000L;
        Span<byte> timeStampBytes = BitConverter.GetBytes(timeStamp);

        if (BitConverter.IsLittleEndian)
        {
            timeStampBytes.Reverse();
        }

        Span<byte> guidBytes = stackalloc byte[16];
        _random.NextBytes(guidBytes);

        var option = _options.CurrentValue;

        switch (option.SequentialType)
        {
            case SequentialType.Sequantial1:
            case SequentialType.Sequantial2:
                timeStampBytes[2..8].CopyTo(guidBytes[..6]);

                if (
                    option.SequentialType == SequentialType.Sequantial2
                    && BitConverter.IsLittleEndian
                )
                {
                    guidBytes[..4].Reverse();
                    guidBytes[4..6].Reverse();
                }

                break;
            case SequentialType.Sequantial3:
                timeStampBytes[2..8].CopyTo(guidBytes[10..16]);
                break;
        }

        return new Guid(guidBytes).ToString("N");
    }
}
