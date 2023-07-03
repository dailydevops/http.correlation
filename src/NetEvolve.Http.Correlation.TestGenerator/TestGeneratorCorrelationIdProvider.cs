namespace NetEvolve.Http.Correlation;

using NetEvolve.Http.Correlation.Abstractions;
using System;

/// <inheritdoc />
internal sealed class TestGeneratorCorrelationIdProvider : IHttpCorrelationIdProvider
{
    private readonly string _generatedId;
    public const string GeneratedTestId = "Generated_Test_Id";

    public TestGeneratorCorrelationIdProvider(string generatedId)
    {
        ArgumentNullException.ThrowIfNull(generatedId);
        if (string.IsNullOrWhiteSpace(generatedId))
        {
            throw new ArgumentException(null, nameof(generatedId));
        }

        _generatedId = generatedId;
    }

    /// <inheritdoc />
    public string GenerateId() => _generatedId;
}
