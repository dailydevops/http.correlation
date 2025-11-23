namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public async Task AddHttpCorrelation_BuilderNull_ThrowsArgumentNullException()
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
    public async Task AddHttpCorrelation_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHttpCorrelation();

        // Assert
        _ = await Assert.That(result).IsTypeOf<HttpCorrelationBuilder>();
        _ = await Assert.That(services.Count).IsEqualTo(2);
        _ = await Assert
            .That(
                services.Any(s =>
                    s.ServiceType == typeof(IHttpCorrelationAccessor)
                    && s.Lifetime == ServiceLifetime.Scoped
                    && s.ImplementationType == typeof(HttpCorrelationAccessor)
                )
            )
            .IsTrue();
        _ = await Assert
            .That(
                services.Any(s =>
                    s.ServiceType == typeof(IHttpContextAccessor)
                    && s.Lifetime == ServiceLifetime.Singleton
                    && s.ImplementationType == typeof(HttpContextAccessor)
                )
            )
            .IsTrue();
    }
}
