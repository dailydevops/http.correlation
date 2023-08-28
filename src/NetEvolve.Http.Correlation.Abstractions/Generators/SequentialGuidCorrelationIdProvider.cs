namespace NetEvolve.Http.Correlation.Generators;

using NetEvolve.Http.Correlation.Abstractions;
using System;
using System.Security.Cryptography;

/// <inheritdoc cref="IHttpCorrelationIdProvider" />
internal sealed class SequentialGuidCorrelationIdProvider : IHttpCorrelationIdProvider
{
    private readonly SequentialType _sequentialType;
    private static readonly RandomNumberGenerator _randomNumberGenerator =
        RandomNumberGenerator.Create();

    public SequentialGuidCorrelationIdProvider(SequentialType sequentialType) =>
        _sequentialType = sequentialType;

    /// <inheritdoc />
    public string GenerateId()
    {
        var timeStamp = DateTime.UtcNow.Ticks / 10000L;
        Span<byte> timeStampBytes = BitConverter.GetBytes(timeStamp);

        if (BitConverter.IsLittleEndian)
        {
            timeStampBytes.Reverse();
        }

        Span<byte> guidBytes = stackalloc byte[16];
        _randomNumberGenerator.GetBytes(guidBytes);

        switch (_sequentialType)
        {
            case SequentialType.Sequantial1:
            case SequentialType.Sequantial2:
            {
                timeStampBytes.Slice(2, 6).CopyTo(guidBytes.Slice(0, 6));

                if (_sequentialType == SequentialType.Sequantial2 && BitConverter.IsLittleEndian)
                {
                    guidBytes.Slice(0, 4).Reverse();
                    guidBytes.Slice(4, 2).Reverse();
                }

                break;
            }
            case SequentialType.Sequantial3:
            {
                timeStampBytes.Slice(2, 6).CopyTo(guidBytes.Slice(10, 6));
                break;
            }
        }

        return new Guid(guidBytes).ToString("N");
    }
}
