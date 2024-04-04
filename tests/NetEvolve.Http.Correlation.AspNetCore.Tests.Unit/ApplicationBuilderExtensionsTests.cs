namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void UseHttpCorrelation_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IApplicationBuilder builder = null!;

        // Act / Assert
        _ = Assert.Throws<ArgumentNullException>("app", () => builder.UseHttpCorrelation());
    }

    [Fact]
    public void UseHttpCorrelation_ServicesNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var builder = new ApplicationBuilder(provider);

        // Act / Assert
        _ = Assert.Throws<InvalidOperationException>(() => builder.UseHttpCorrelation());
    }

    [Fact]
    public void UseHttpCorrelation_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddHttpCorrelation().WithGuidGenerator();
        var provider = services.BuildServiceProvider();
        var builder = new ApplicationBuilder(provider);

        // Act
        var result = builder.UseHttpCorrelation();

        // Assert
        Assert.NotNull(result);
    }
}
