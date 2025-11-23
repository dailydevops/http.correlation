namespace NetEvolve.Http.Correlation.HttpClient.Tests.Unit;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class HttpClientBuilderExtensionsTests
{
    [Test]
    public async Task AddHttpCorrelation_BuilderNull_ThrowsArgumentNullException()
    {
        IHttpClientBuilder builder = null!;

        _ = await Assert.That(() => builder.AddHttpCorrelation()).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddHttpCorrelation_Builder_Expected()
    {
        var services = new ServiceCollection();
        _ = services.AddHttpClient("test").AddHttpCorrelation();

        _ = await Assert
            .That(
                services.Any(s =>
                    s.Lifetime == ServiceLifetime.Transient
                    && s.ImplementationType == typeof(HttpCorrelationIdHandler)
                    && s.ServiceType == typeof(HttpCorrelationIdHandler)
                )
            )
            .IsTrue();
    }
}
