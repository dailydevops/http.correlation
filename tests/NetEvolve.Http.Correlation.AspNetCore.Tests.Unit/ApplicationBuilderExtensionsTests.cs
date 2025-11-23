namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class ApplicationBuilderExtensionsTests
{
    [Test]
    public async Task UseHttpCorrelation_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IApplicationBuilder builder = null!;

        // Act / Assert
        _ = await Assert
            .That(() => builder.UseHttpCorrelation())
            .Throws<ArgumentNullException>()
            .WithParameterName("app");
    }

    [Test]
    public async Task UseHttpCorrelation_ServicesNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var builder = new ApplicationBuilder(provider);

        // Act / Assert
        _ = await Assert.That(() => builder.UseHttpCorrelation()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task UseHttpCorrelation_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddHttpCorrelation().WithGuidGenerator();
        var provider = services.BuildServiceProvider();
        var builder = new ApplicationBuilder(provider);

        // Act
        var result = builder.UseHttpCorrelation();

        // Assert
        _ = await Assert.That(result).IsNotNull();
    }
}
