namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Integration;

using System;
using System.Linq;
using System.Threading.Tasks;
using NetEvolve.SequentialGuid;
using Xunit;

public class HttpCorrelationMiddlewareTests : TestBase
{
    [Fact]
    public async Task UseHttpCorrelation_WithoutGenerator_Expected()
    {
        var result = await RunAsync();

        Assert.True(result.Headers.Contains(CorrelationConstants.HeaderName1));
        Assert.NotEmpty(result.Headers.GetValues(CorrelationConstants.HeaderName1));
    }

    [Fact]
    public async Task UseHttpCorrelation_WithGenerator_Expected()
    {
        var result = await RunAsync(correlationBuilder: builder => builder.WithGuidGenerator());

        Assert.True(result.Headers.Contains(CorrelationConstants.HeaderName1));

        var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
        Assert.NotEmpty(values);
        Assert.True(Guid.TryParse(values.First(), out _));
    }

#if NET9_0_OR_GREATER
    [Fact]
    public async Task UseHttpCorrelation_WithGuidV7Generator_Expected()
    {
        var result = await RunAsync(correlationBuilder: builder => builder.WithGuidV7Generator());
        Assert.True(result.Headers.Contains(CorrelationConstants.HeaderName1));
        var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
        Assert.NotEmpty(values);
        Assert.True(Guid.TryParse(values.First(), out _));
    }
#endif

    [Theory]
    [InlineData(SequentialGuidType.AsBinary)]
    [InlineData(SequentialGuidType.AsString)]
    [InlineData(SequentialGuidType.AtEnd)]
    public async Task UseHttpCorrelation_WithSequentialGuidGenerator_Expected(SequentialGuidType sequentialGuidType)
    {
        var result = await RunAsync(
#pragma warning disable CS0618 // Obsolete
            correlationBuilder: builder =>
            builder.WithSequentialGuidGenerator(options => options.SequentialType = sequentialGuidType)
#pragma warning restore CS0618 // Obsolete
        );
        Assert.True(result.Headers.Contains(CorrelationConstants.HeaderName1));
        var values = result.Headers.GetValues(CorrelationConstants.HeaderName1);
        Assert.NotEmpty(values);
        Assert.True(Guid.TryParse(values.First(), out _));
    }

    [Fact]
    public async Task UseHttpCorrelation_WithHeaderName1_Expected()
    {
        var testCorrelationId = Guid.NewGuid().ToString("N");
        var result = await RunAsync(
            clientConfiguration: client =>
                client.DefaultRequestHeaders.Add(CorrelationConstants.HeaderName1, testCorrelationId),
            requestPath: InvokePath
        );

        Assert.True(result.Headers.Contains(CorrelationConstants.HeaderName1));
        Assert.Equal(testCorrelationId, result.Headers.GetValues(CorrelationConstants.HeaderName1).FirstOrDefault());

        Assert.Equal(testCorrelationId, await result.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UseHttpCorrelation_WithHeaderName2_Expected()
    {
        var testCorrelationId = Guid.NewGuid().ToString("N");
        var result = await RunAsync(clientConfiguration: client =>
            client.DefaultRequestHeaders.Add(CorrelationConstants.HeaderName2, testCorrelationId)
        );

        Assert.True(result.Headers.Contains(CorrelationConstants.HeaderName2));
        Assert.Equal(testCorrelationId, result.Headers.GetValues(CorrelationConstants.HeaderName2).FirstOrDefault());
    }
}
