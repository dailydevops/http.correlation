namespace NetEvolve.Http.Correlation.Ulid.Tests.Unit;

using System;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;
using Xunit;

public class HttpCorrelationBuilderExtensionsTests
{
    [Fact]
    public void WithUlidGenerator_BuilderNull_Throws()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = Assert.Throws<ArgumentNullException>("builder", () => builder.WithUlidGenerator());
    }

    [Fact]
    public void WithUlidGenerator_Builder_ReturnsBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TestHttpCorrelationBuilder(services);

        // Act
        _ = builder.WithUlidGenerator();

        // Assert
        Assert.Contains(
            services,
            s =>
                s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(IHttpCorrelationIdProvider)
                && s.ImplementationType == typeof(UlidCorrelationIdProvider)
        );
    }

    private sealed class TestHttpCorrelationBuilder : IHttpCorrelationBuilder
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        public TestHttpCorrelationBuilder(IServiceCollection services) => Services = services;
    }
}
