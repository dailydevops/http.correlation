namespace NetEvolve.Http.Correlation.HttpClient.Tests.Unit;

using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class HttpClientBuilderExtensionsTests
{
    [Fact]
    public void AddHttpCorrelation_BuilderNull_ThrowsArgumentNullException()
    {
        IHttpClientBuilder builder = null!;

        _ = Assert.Throws<ArgumentNullException>(() => builder.AddHttpCorrelation());
    }

    [Fact]
    public void AddHttpCorrelation_Builder_Expected()
    {
        var services = new ServiceCollection();
        _ = services.AddHttpClient("test").AddHttpCorrelation();

        Assert.Contains(
            services,
            s =>
                s.Lifetime == ServiceLifetime.Transient
                && s.ImplementationType == typeof(HttpCorrelationIdHandler)
                && s.ServiceType == typeof(HttpCorrelationIdHandler)
        );
    }
}
