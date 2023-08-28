namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit;

using NetEvolve.Http.Correlation.Generators;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

[ExcludeFromCodeCoverage]
public class GuidCorrelationIdProviderTests
{
    // Write a xunit fact test that verifies that the generated Guid is not empty
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
