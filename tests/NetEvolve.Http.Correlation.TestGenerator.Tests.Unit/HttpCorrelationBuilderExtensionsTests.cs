namespace NetEvolve.Http.Correlation.TestGenerator.Tests.Unit;

using System;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public void WithTestGenerator_Builder_ReturnsBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TestHttpCorrelationBuilder(services);

        // Act
        var result = builder.WithTestGenerator();

        // Assert
        Assert.Contains(
            services,
            s =>
                s.ServiceType == typeof(IHttpCorrelationIdProvider)
                && s.Lifetime == ServiceLifetime.Singleton
                && s.ImplementationInstance is TestGeneratorCorrelationIdProvider
        );
    }

    private sealed class TestHttpCorrelationBuilder : IHttpCorrelationBuilder
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        public TestHttpCorrelationBuilder(IServiceCollection services) => Services = services;
    }
}
