namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;
using Xunit;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHttpCorrelation_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act / Assert
        _ = Assert.Throws<ArgumentNullException>("services", () => services.AddHttpCorrelation());
    }

    [Fact]
    public void AddHttpCorrelation_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHttpCorrelation();

        // Assert
        _ = Assert.IsType<HttpCorrelationBuilder>(result);
        Assert.Equal(2, services.Count);
        Assert.Contains(
            services,
            s =>
                s.ServiceType == typeof(IHttpCorrelationAccessor)
                && s.Lifetime == ServiceLifetime.Scoped
                && s.ImplementationType == typeof(HttpCorrelationAccessor)
        );
        Assert.Contains(
            services,
            s =>
                s.ServiceType == typeof(IHttpContextAccessor)
                && s.Lifetime == ServiceLifetime.Singleton
                && s.ImplementationType == typeof(HttpContextAccessor)
        );
    }
}
