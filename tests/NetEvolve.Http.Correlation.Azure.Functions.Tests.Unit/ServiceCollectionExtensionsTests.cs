namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Unit;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation;
using NetEvolve.Http.Correlation.Abstractions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public async Task AddHttpCorrelation_ServicesNull_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act / Assert
        _ = await Assert
            .That(() => services.AddHttpCorrelation())
            .Throws<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Test]
    public async Task AddHttpCorrelation_Services_Expected()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHttpCorrelation();

        // Assert
        using (Assert.Multiple())
        {
            _ = await Assert.That(result).IsTypeOf<HttpCorrelationBuilder>();
            _ = await Assert.That(services.Count).IsEqualTo(2);
            _ = await Assert
                .That(
                    services.Any(s =>
                        s.ServiceType == typeof(IHttpCorrelationAccessor)
                        && s.Lifetime == ServiceLifetime.Scoped
                        && s.ImplementationFactory is not null
                    )
                )
                .IsTrue();
            _ = await Assert
                .That(
                    services.Any(s =>
                        s.ServiceType == typeof(FunctionsCorrelationAccessor)
                        && s.Lifetime == ServiceLifetime.Scoped
                        && s.ImplementationType == typeof(FunctionsCorrelationAccessor)
                    )
                )
                .IsTrue();
        }
    }
}
