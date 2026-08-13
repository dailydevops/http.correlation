namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Integration;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetEvolve.Http.Correlation;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class HttpCorrelationMiddlewareTests : TestBase
{
    [Test]
    public async Task UseHttpCorrelation_WithoutGenerator_Expected(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await RunAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        using (Assert.Multiple())
        {
            _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();
            _ = await Assert.That(result.Headers.GetValues(CorrelationConstants.HeaderName1)).IsNotEmpty();
        }
    }

    [Test]
    public async Task UseHttpCorrelation_WithGenerator_Expected(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await RunAsync(
                correlationBuilder: builder => builder.WithGuidGenerator(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        using (Assert.Multiple())
        {
            _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();

            var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
            _ = await Assert.That(values).IsNotEmpty();
            _ = await Assert.That(Guid.TryParse(values.First(), out _)).IsTrue();
        }
    }

#if NET9_0_OR_GREATER
    [Test]
    public async Task UseHttpCorrelation_WithGuidV7Generator_Expected(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await RunAsync(
                correlationBuilder: builder => builder.WithGuidV7Generator(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        using (Assert.Multiple())
        {
            _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();
            var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
            _ = await Assert.That(values).IsNotEmpty();
            _ = await Assert.That(Guid.TryParse(values.First(), out _)).IsTrue();
        }
    }
#endif

    [Test]
    public async Task UseHttpCorrelation_WithHeaderName1_Expected(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var testCorrelationId = Guid.NewGuid().ToString("N");
        var result = await RunAsync(
                clientConfiguration: client =>
                    client.DefaultRequestHeaders.Add(CorrelationConstants.HeaderName1, testCorrelationId),
                requestPath: InvokePath,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        using (Assert.Multiple())
        {
            _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();
            _ = await Assert
                .That(result.Headers.GetValues(CorrelationConstants.HeaderName1).FirstOrDefault())
                .IsEqualTo(testCorrelationId);

            var correlationResult = await result
                .Content.ReadAsStringAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            _ = await Assert.That(correlationResult).IsEqualTo(testCorrelationId);
        }
    }

    [Test]
    public async Task UseHttpCorrelation_WithHeaderName2_Expected(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var testCorrelationId = Guid.NewGuid().ToString("N");
        var result = await RunAsync(
                clientConfiguration: client =>
                    client.DefaultRequestHeaders.Add(CorrelationConstants.HeaderName2, testCorrelationId),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        using (Assert.Multiple())
        {
            _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName2)).IsTrue();
            _ = await Assert
                .That(result.Headers.GetValues(CorrelationConstants.HeaderName2).FirstOrDefault())
                .IsEqualTo(testCorrelationId);
        }
    }
}
