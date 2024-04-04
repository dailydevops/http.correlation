namespace NetEvolve.Http.Correlation.TestGenerator.Tests.Unit;

using System;
using System.Diagnostics.CodeAnalysis;
using NetEvolve.Extensions.XUnit;
using NetEvolve.Http.Correlation.Abstractions;
using Xunit;

[ExcludeFromCodeCoverage]
[UnitTest]
public class HttpCorrelationBuilderExtensionsTests
{
    [Fact]
    public void WithTestGenerator_BuilderNull_Throws()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = Assert.Throws<ArgumentNullException>("builder", () => builder.WithTestGenerator());
    }
}
