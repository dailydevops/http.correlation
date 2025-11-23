namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class HttpCorrelationBuilderExtensionsTests
{
    [Test]
    public async Task WithGuidGenerator_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = await Assert
            .That(() => builder.WithGuidGenerator())
            .Throws<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Test]
    public async Task WithGuidGenerator_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new HttpCorrelationBuilder(services);

        // Act
        var result = builder.WithGuidGenerator().WithGuidGenerator();

        // Assert
        _ = await Assert.That(result).IsNotNull();
        _ = await Assert
            .That(
                services.Any(s =>
                    s.ServiceType == typeof(IHttpCorrelationIdProvider)
                    && s.Lifetime == ServiceLifetime.Singleton
                    && !string.IsNullOrEmpty(s.ImplementationType!.FullName)
                    && s.ImplementationType.FullName.Equals(
                        "NetEvolve.Http.Correlation.Generators.GuidCorrelationIdProvider",
                        StringComparison.Ordinal
                    )
                )
            )
            .IsTrue();
        _ = await Assert.That(services.Count).IsEqualTo(1);
    }

    [Test]
    public async Task WithSequentialGuidGenerator_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = await Assert
            .That(
#pragma warning disable CS0618 // Obsolete
                () => builder.WithSequentialGuidGenerator()
#pragma warning restore CS0618 // Obsolete
            )
            .Throws<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Test]
    public async Task WithSequentialGuidGenerator_Builder_Expected()
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
        _ = await Assert.That(result).IsNotNull();
        _ = await Assert
            .That(
                services.Any(s =>
                    s.ServiceType == typeof(IHttpCorrelationIdProvider)
                    && s.Lifetime == ServiceLifetime.Singleton
                    && !string.IsNullOrEmpty(s.ImplementationType!.FullName)
                    && s.ImplementationType.FullName.Equals(
                        "NetEvolve.Http.Correlation.Generators.SequentialGuidCorrelationIdProvider",
                        StringComparison.Ordinal
                    )
                )
            )
            .IsTrue();
        _ = await Assert.That(services.Count).IsEqualTo(9);
    }
}
