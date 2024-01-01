namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit.Generators;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using NetEvolve.Extensions.XUnit;
using NetEvolve.Http.Correlation.Generators;
using NSubstitute;
using Xunit;

[ExcludeFromCodeCoverage]
[UnitTest]
public sealed class SequentialGuidCorrelationIdProviderTests
{
    [Theory]
    [InlineData(SequentialType.AsBinary)]
    [InlineData(SequentialType.AsString)]
    [InlineData(SequentialType.AtEnd)]
    public void GenerateId_NotEmpty(SequentialType sequentialType)
    {
        // Arrange
        var optionsMonitorMock = Substitute.For<IOptionsMonitor<SequentialGuidOptions>>();
        _ = optionsMonitorMock.CurrentValue.Returns(
            new SequentialGuidOptions { SequentialType = sequentialType }
        );
        var provider = new SequentialGuidCorrelationIdProvider(optionsMonitorMock);

        // Act
        var id = provider.GenerateId();

        // Assert
        Assert.NotEqual($"{Guid.Empty:N}", id);
    }
}
