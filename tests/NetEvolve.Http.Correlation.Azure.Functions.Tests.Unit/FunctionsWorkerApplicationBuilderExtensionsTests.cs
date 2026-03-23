namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Unit;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation;
using NSubstitute;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class FunctionsWorkerApplicationBuilderExtensionsTests
{
    [Test]
    public async Task UseHttpCorrelation_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IFunctionsWorkerApplicationBuilder builder = null!;

        // Act / Assert
        _ = await Assert
            .That(() => builder.UseHttpCorrelation())
            .Throws<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Test]
    public async Task UseHttpCorrelation_ServicesNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = Substitute.For<IFunctionsWorkerApplicationBuilder>();
        _ = builder.Services.Returns(services);

        // Act / Assert
        _ = await Assert.That(() => builder.UseHttpCorrelation()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task UseHttpCorrelation_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddHttpCorrelation().WithGuidGenerator();
        var builder = Substitute.For<IFunctionsWorkerApplicationBuilder>();
        _ = builder.Services.Returns(services);

        // Act
        var result = builder.UseHttpCorrelation();

        // Assert
        _ = await Assert.That(result).IsNotNull();
    }
}
