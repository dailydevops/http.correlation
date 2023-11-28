namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit;

using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Generators;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

[ExcludeFromCodeCoverage]
public class GuidCorrelationIdProviderTests
{
    [Fact]
    public void GenerateId_NotEmpty()
    {
        // Arrange
        var provider = new GuidCorrelationIdProvider();

        // Act
        var id = provider.GenerateId();

        // Assert
        Assert.NotEqual($"{Guid.Empty:N}", id);
    }
}

[ExcludeFromCodeCoverage]
public sealed class SequentialGuidCorrelationIdProviderTests
{
    [Theory]
    [InlineData(SequentialType.AsBinary)]
    [InlineData(SequentialType.AsString)]
    [InlineData(SequentialType.AtEnd)]
    public void GenerateId_NotEmpty(SequentialType sequentialType)
    {
        // Arrange
        var provider = new SequentialGuidCorrelationIdProvider(
                       new OptionsMonitorMock<SequentialGuidOptions>(
                                          new SequentialGuidOptions { SequentialType = sequentialType }
                                                     )
                              );

        // Act
        var id = provider.GenerateId();

        // Assert
        Assert.NotEqual($"{Guid.Empty:N}", id);
    }
}
