#if NET9_0_OR_GREATER
namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit.Generators;

using System;
using NetEvolve.Http.Correlation.Generators;
using Xunit;

public sealed class GuidV7CorrelationIdProviderTests
{
    [Fact]
    public void GenerateId_NotEmpty()
    {
        // Arrange
        var provider = new GuidV7CorrelationIdProvider();
        // Act
        var id = provider.GenerateId();
        // Assert
        Assert.NotEqual($"{Guid.Empty:N}", id);
    }
}
#endif
