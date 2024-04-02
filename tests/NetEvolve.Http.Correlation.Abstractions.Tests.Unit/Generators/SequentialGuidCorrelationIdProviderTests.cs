namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit.Generators;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using NetEvolve.Extensions.XUnit;
using NetEvolve.Http.Correlation.Generators;
using NetEvolve.SequentialGuid;
using NSubstitute;
using Xunit;

[ExcludeFromCodeCoverage]
[UnitTest]
public sealed class SequentialGuidCorrelationIdProviderTests
{
    [Theory]
    [InlineData(SequentialGuidType.AsBinary)]
    [InlineData(SequentialGuidType.AsString)]
    [InlineData(SequentialGuidType.AtEnd)]
    public void GenerateId_NotEmpty(SequentialGuidType sequentialType)
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
