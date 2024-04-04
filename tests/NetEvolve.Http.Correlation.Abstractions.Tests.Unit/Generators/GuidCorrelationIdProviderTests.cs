namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit.Generators;

using System;
using NetEvolve.Http.Correlation.Generators;
using Xunit;

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
