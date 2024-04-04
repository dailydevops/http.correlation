namespace NetEvolve.Http.Correlation.TestGenerator.Tests.Unit;

using System;
using NetEvolve.Http.Correlation.Abstractions;
using Xunit;

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
