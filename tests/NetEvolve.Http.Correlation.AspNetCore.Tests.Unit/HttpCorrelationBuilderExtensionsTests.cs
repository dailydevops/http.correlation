namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;
using Xunit;

public class HttpCorrelationBuilderExtensionsTests
{
    [Fact]
    public void WithGuidGenerator_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = Assert.Throws<ArgumentNullException>("builder", () => builder.WithGuidGenerator());
    }

    [Fact]
    public void WithGuidGenerator_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new HttpCorrelationBuilder(services);

        // Act
        var result = builder.WithGuidGenerator().WithGuidGenerator();

        // Assert
        Assert.Multiple(
            () => Assert.NotNull(result),
            () =>
                Assert.Contains(
                    services,
                    s =>
                        s.ServiceType == typeof(IHttpCorrelationIdProvider)
                        && s.Lifetime == ServiceLifetime.Singleton
                        && !string.IsNullOrEmpty(s.ImplementationType!.FullName)
                        && s.ImplementationType.FullName.Equals(
                            "NetEvolve.Http.Correlation.Generators.GuidCorrelationIdProvider",
                            StringComparison.Ordinal
                        )
                ),
            () => Assert.Single(services)
        );
    }

    [Fact]
    public void WithSequentialGuidGenerator_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = Assert.Throws<ArgumentNullException>(
            "builder",
#pragma warning disable CS0618 // Obsolete
            () => builder.WithSequentialGuidGenerator()
#pragma warning restore CS0618 // Obsolete
        );
    }

    [Fact]
    public void WithSequentialGuidGenerator_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new HttpCorrelationBuilder(services);

        // Act
#pragma warning disable CS0618 // Obsolete
        var result = builder
            .WithSequentialGuidGenerator()
            .WithSequentialGuidGenerator(options =>
                options.SequentialType = SequentialGuid.SequentialGuidType.AsBinary
            );
#pragma warning restore CS0618 // Obsolete

        // Assert
        Assert.Multiple(
            () => Assert.NotNull(result),
            () =>
                Assert.Contains(
                    services,
                    s =>
                        s.ServiceType == typeof(IHttpCorrelationIdProvider)
                        && s.Lifetime == ServiceLifetime.Singleton
                        && !string.IsNullOrEmpty(s.ImplementationType!.FullName)
                        && s.ImplementationType.FullName.Equals(
                            "NetEvolve.Http.Correlation.Generators.SequentialGuidCorrelationIdProvider",
                            StringComparison.Ordinal
                        )
                ),
            () => Assert.Equal(9, services.Count)
        );
    }
}
