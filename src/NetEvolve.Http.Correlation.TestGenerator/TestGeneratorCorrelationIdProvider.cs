namespace NetEvolve.Http.Correlation;

using NetEvolve.Http.Correlation.Abstractions;
using System;

/// <inheritdoc />
internal sealed class TestGeneratorCorrelationIdProvider : IHttpCorrelationIdProvider
{
    private readonly string _generatedId;
    private const string GeneratedTestId = "Generated_Test_Id";

    public TestGeneratorCorrelationIdProvider(string? generatedId)
    {
        if (string.IsNullOrWhiteSpace(generatedId))
        {
            generatedId = GeneratedTestId;
        }

        _generatedId = generatedId;
    }

    /// <inheritdoc />
    public string GenerateId() => _generatedId;
}
