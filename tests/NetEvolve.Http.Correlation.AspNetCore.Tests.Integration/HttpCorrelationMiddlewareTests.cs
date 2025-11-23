namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Integration;

using System;
using System.Linq;
using System.Threading.Tasks;
using NetEvolve.SequentialGuid;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class HttpCorrelationMiddlewareTests : TestBase
{
    [Test]
    public async Task UseHttpCorrelation_WithoutGenerator_Expected()
    {
        var result = await RunAsync();

        _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();
        _ = await Assert.That(result.Headers.GetValues(CorrelationConstants.HeaderName1)).IsNotEmpty();
    }

    [Test]
    public async Task UseHttpCorrelation_WithGenerator_Expected()
    {
        var result = await RunAsync(correlationBuilder: builder => builder.WithGuidGenerator());

        _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();

        var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
        _ = await Assert.That(values).IsNotEmpty();
        _ = await Assert.That(Guid.TryParse(values.First(), out _)).IsTrue();
    }

#if NET9_0_OR_GREATER
    [Test]
    public async Task UseHttpCorrelation_WithGuidV7Generator_Expected()
    {
        var result = await RunAsync(correlationBuilder: builder => builder.WithGuidV7Generator());
        _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();
        var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
        _ = await Assert.That(values).IsNotEmpty();
        _ = await Assert.That(Guid.TryParse(values.First(), out _)).IsTrue();
    }
#endif

    [Test]
    [Arguments(SequentialGuidType.AsBinary)]
    [Arguments(SequentialGuidType.AsString)]
    [Arguments(SequentialGuidType.AtEnd)]
    public async Task UseHttpCorrelation_WithSequentialGuidGenerator_Expected(SequentialGuidType sequentialGuidType)
    {
        var result = await RunAsync(
#pragma warning disable CS0618 // Obsolete
            correlationBuilder: builder =>
            builder.WithSequentialGuidGenerator(options => options.SequentialType = sequentialGuidType)
#pragma warning restore CS0618 // Obsolete
        );
        _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();
        var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
        _ = await Assert.That(values).IsNotEmpty();
        _ = await Assert.That(Guid.TryParse(values.First(), out _)).IsTrue();
    }

    [Test]
    public async Task UseHttpCorrelation_WithHeaderName1_Expected()
    {
        var testCorrelationId = Guid.NewGuid().ToString("N");
        var result = await RunAsync(
            clientConfiguration: client =>
                client.DefaultRequestHeaders.Add(CorrelationConstants.HeaderName1, testCorrelationId),
            requestPath: InvokePath
        );

        _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName1)).IsTrue();
        _ = await Assert
            .That(result.Headers.GetValues(CorrelationConstants.HeaderName1).FirstOrDefault())
            .IsEqualTo(testCorrelationId);

        _ = await Assert.That(await result.Content.ReadAsStringAsync()).IsEqualTo(testCorrelationId);
    }

    [Test]
    public async Task UseHttpCorrelation_WithHeaderName2_Expected()
    {
        var testCorrelationId = Guid.NewGuid().ToString("N");
        var result = await RunAsync(clientConfiguration: client =>
            client.DefaultRequestHeaders.Add(CorrelationConstants.HeaderName2, testCorrelationId)
        );

        _ = await Assert.That(result.Headers.Contains(CorrelationConstants.HeaderName2)).IsTrue();
        _ = await Assert
            .That(result.Headers.GetValues(CorrelationConstants.HeaderName2).FirstOrDefault())
            .IsEqualTo(testCorrelationId);
    }
}
