namespace NetEvolve.Http.Correlation.Ulid.Tests.Unit;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class HttpCorrelationBuilderExtensionsTests
{
    [Test]
    public async Task WithUlidGenerator_BuilderNull_Throws()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = await Assert
            .That(() => builder.WithUlidGenerator())
            .Throws<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Test]
    public async Task WithUlidGenerator_Builder_ReturnsBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TestHttpCorrelationBuilder(services);

        // Act
        _ = builder.WithUlidGenerator();

        // Assert
        _ = await Assert
            .That(
                services.Any(s =>
                    s.Lifetime == ServiceLifetime.Singleton
                    && s.ServiceType == typeof(IHttpCorrelationIdProvider)
                    && s.ImplementationType == typeof(UlidCorrelationIdProvider)
                )
            )
            .IsTrue();
    }

    private sealed class TestHttpCorrelationBuilder : IHttpCorrelationBuilder
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        public TestHttpCorrelationBuilder(IServiceCollection services) => Services = services;
    }
}
