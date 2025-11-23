namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System;
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
        using (Assert.Multiple())
        {
            _ = await Assert.That(result).IsNotNull();
            _ = await Assert
                .That(services)
                .Contains(s =>
                    s.ServiceType == typeof(IHttpCorrelationIdProvider)
                    && s.Lifetime == ServiceLifetime.Singleton
                    && !string.IsNullOrEmpty(s.ImplementationType!.FullName)
                    && s.ImplementationType.FullName.Equals(
                        "NetEvolve.Http.Correlation.Generators.GuidCorrelationIdProvider",
                        StringComparison.Ordinal
                    )
                );
            _ = await Assert.That(services.Count).IsEqualTo(1);
        }
    }

#if NET9_0_OR_GREATER
    [Test]
    public async Task WithGuidV7Generator_BuilderNull_ThrowsArgumentNullException()
    {
        // Arrange
        IHttpCorrelationBuilder builder = null!;

        // Act / Assert
        _ = await Assert
            .That(() => builder.WithGuidV7Generator())
            .Throws<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Test]
    public async Task WithGuidV7Generator_Builder_Expected()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new HttpCorrelationBuilder(services);

        // Act
        var result = builder.WithGuidV7Generator().WithGuidV7Generator();

        // Assert
        using (Assert.Multiple())
        {
            _ = await Assert.That(result).IsNotNull();
            _ = await Assert
                .That(services)
                .Contains(s =>
                    s.ServiceType == typeof(IHttpCorrelationIdProvider)
                    && s.Lifetime == ServiceLifetime.Singleton
                    && !string.IsNullOrEmpty(s.ImplementationType!.FullName)
                    && s.ImplementationType.FullName.Equals(
                        "NetEvolve.Http.Correlation.Generators.GuidV7CorrelationIdProvider",
                        StringComparison.Ordinal
                    )
                );
            _ = await Assert.That(services.Count).IsEqualTo(1);
        }
    }
#endif
}
