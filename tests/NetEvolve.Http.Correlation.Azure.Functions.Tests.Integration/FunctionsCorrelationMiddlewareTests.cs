namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEvolve.Http.Correlation;
using NSubstitute;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class FunctionsCorrelationMiddlewareTests : TestBase
{
    [Test]
    public async Task Invoke_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddHttpCorrelation()
            .Services.BuildServiceProvider();

        var middleware = new FunctionsCorrelationMiddleware(
            provider.GetRequiredService<ILogger<FunctionsCorrelationMiddleware>>()
        );

        FunctionContext context = null!;

        // Act / Assert
        _ = await Assert
            .That(async () => await middleware.Invoke(context, _ => Task.CompletedTask))
            .Throws<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Test]
    public async Task UseHttpCorrelation_WithNonHttpFunction_CallsNext()
    {
        // Arrange / Act — no requestSetup means GetHttpRequestDataAsync() returns null → passes through
        var result = await RunAsync();

        // Assert
        _ = await Assert.That(result.NextCalled).IsTrue();
    }

    [Test]
    public async Task UseHttpCorrelation_WithNonHttpFunction_WithGenerator_CallsNext()
    {
        // Arrange / Act
        var result = await RunAsync(correlationBuilder: b => b.WithGuidGenerator());

        // Assert
        _ = await Assert.That(result.NextCalled).IsTrue();
    }

    [Test]
    public async Task UseHttpCorrelation_WithHeaderName1_Expected()
    {
        // Arrange
        var testCorrelationId = Guid.NewGuid().ToString("N");

        // Act
        var result = await RunAsync(requestSetup: req =>
            req.Headers.Add(CorrelationConstants.HeaderName1, testCorrelationId)
        );

        // Assert
        using (Assert.Multiple())
        {
            _ = await Assert.That(result.NextCalled).IsTrue();
            _ = await Assert.That(result.CorrelationId).IsEqualTo(testCorrelationId);
            _ = await Assert.That(result.HeaderName).IsEqualTo(CorrelationConstants.HeaderName1);
        }
    }

    [Test]
    public async Task UseHttpCorrelation_WithHeaderName2_Expected()
    {
        // Arrange
        var testCorrelationId = Guid.NewGuid().ToString("N");

        // Act
        var result = await RunAsync(requestSetup: req =>
            req.Headers.Add(CorrelationConstants.HeaderName2, testCorrelationId)
        );

        // Assert
        using (Assert.Multiple())
        {
            _ = await Assert.That(result.NextCalled).IsTrue();
            _ = await Assert.That(result.CorrelationId).IsEqualTo(testCorrelationId);
            _ = await Assert.That(result.HeaderName).IsEqualTo(CorrelationConstants.HeaderName2);
        }
    }

    [Test]
    public async Task UseHttpCorrelation_WithoutGenerator_FallsBackToInvocationId()
    {
        // Arrange / Act — no correlation header, no generator → falls back to InvocationId
        var result = await RunAsync(requestSetup: _ => { });

        // Assert
        using (Assert.Multiple())
        {
            _ = await Assert.That(result.NextCalled).IsTrue();
            _ = await Assert.That(result.CorrelationId).IsEqualTo("test-invocation-id");
        }
    }

    [Test]
    public async Task UseHttpCorrelation_WithGenerator_GeneratesId()
    {
        // Arrange / Act — no correlation header, but generator registered → uses generated ID
        var result = await RunAsync(correlationBuilder: b => b.WithGuidGenerator(), requestSetup: _ => { });

        // Assert
        using (Assert.Multiple())
        {
            _ = await Assert.That(result.NextCalled).IsTrue();
            _ = await Assert.That(result.CorrelationId).IsNotEmpty();
            _ = await Assert.That(Guid.TryParse(result.CorrelationId, out _)).IsTrue();
        }
    }

#if NET9_0_OR_GREATER
    [Test]
    public async Task UseHttpCorrelation_WithGuidV7Generator_GeneratesId()
    {
        // Arrange / Act
        var result = await RunAsync(correlationBuilder: b => b.WithGuidV7Generator(), requestSetup: _ => { });

        // Assert
        using (Assert.Multiple())
        {
            _ = await Assert.That(result.NextCalled).IsTrue();
            _ = await Assert.That(result.CorrelationId).IsNotEmpty();
            _ = await Assert.That(Guid.TryParse(result.CorrelationId, out _)).IsTrue();
        }
    }
#endif
}
