namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEvolve.Http.Correlation;
using NetEvolve.Http.Correlation.HttpClient;
using TUnit.Assertions.Enums;
using TUnit.Assertions.Extensions;
using TUnit.Core;
using TUnit.Mocks;

/// <summary>
/// Reproduces https://github.com/dailydevops/http.correlation/issues/858 for the Azure Functions host:
/// outgoing calls made through <c>WithHttpCorrelation()</c> from within a function invocation, with one
/// service provider shared across invocations, so <see cref="IHttpClientFactory"/> reuses its cached handler chain.
/// </summary>
public class HttpClientForwardingTests : TestBase
{
    private const string DownstreamClient = "downstream";

    [Test]
    public async Task SendAsync_SingleInvocation_ForwardsInvocationCorrelationId(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var recorder = new OutgoingRecorder();
        var provider = BuildProvider(recorder);
        await using (provider.ConfigureAwait(false))
        {
            await InvokeAsync(provider, CorrelationConstants.HeaderName1, "First", cancellationToken)
                .ConfigureAwait(false);

            _ = await Assert.That(recorder.ValuesOf(CorrelationConstants.HeaderName1)).IsEquivalentTo(["First"]);
        }
    }

    [Test]
    public async Task SendAsync_SequentialInvocations_ForwardEachInvocationsOwnCorrelationId(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var recorder = new OutgoingRecorder();
        var provider = BuildProvider(recorder);
        await using (provider.ConfigureAwait(false))
        {
            await InvokeAsync(provider, CorrelationConstants.HeaderName1, "First", cancellationToken)
                .ConfigureAwait(false);
            await InvokeAsync(provider, CorrelationConstants.HeaderName1, "Second", cancellationToken)
                .ConfigureAwait(false);

            _ = await Assert
                .That(recorder.ValuesOf(CorrelationConstants.HeaderName1))
                .IsEquivalentTo(["First", "Second"], CollectionOrdering.Matching);
        }
    }

    [Test]
    public async Task SendAsync_IncomingAlternativeHeader_ForwardsUsingAlternativeHeader(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var recorder = new OutgoingRecorder();
        var provider = BuildProvider(recorder);
        await using (provider.ConfigureAwait(false))
        {
            await InvokeAsync(provider, CorrelationConstants.HeaderName2, "Alternative", cancellationToken)
                .ConfigureAwait(false);

            using (Assert.Multiple())
            {
                _ = await Assert
                    .That(recorder.ValuesOf(CorrelationConstants.HeaderName2))
                    .IsEquivalentTo(["Alternative"]);
                _ = await Assert.That(recorder.ValuesOf(CorrelationConstants.HeaderName1)).IsEmpty();
            }
        }
    }

    [Test]
    public async Task SendAsync_OutsideOfInvocation_AddsNoCorrelationHeader(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var recorder = new OutgoingRecorder();
        var provider = BuildProvider(recorder);
        await using (provider.ConfigureAwait(false))
        {
            // e.g. a timer or queue triggered function, or a hosted service in the worker process
            using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(DownstreamClient);
            using var response = await client
                .GetAsync(new Uri("https://downstream/"), cancellationToken)
                .ConfigureAwait(false);

            var recorded = recorder.Requests.Single();

            using (Assert.Multiple())
            {
                _ = await Assert.That(recorded.ContainsKey(CorrelationConstants.HeaderName1)).IsFalse();
                _ = await Assert.That(recorded.ContainsKey(CorrelationConstants.HeaderName2)).IsFalse();
            }
        }
    }

    private static ServiceProvider BuildProvider(OutgoingRecorder recorder)
    {
        var services = new ServiceCollection().AddLogging();
        _ = services.AddHttpCorrelation();
        _ = services
            .AddHttpClient(DownstreamClient)
            .WithHttpCorrelation()
            .ConfigurePrimaryHttpMessageHandler(() => new RecordingPrimaryHandler(recorder));

        return services.BuildServiceProvider();
    }

    private static async Task InvokeAsync(
        ServiceProvider provider,
        string headerName,
        string correlationId,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var context = FunctionContext.Mock();
        _ = context.InvocationId.Returns(Guid.NewGuid().ToString("N"));

        var scope = provider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            _ = context.InstanceServices.Returns(scope.ServiceProvider);

            var features = IInvocationFeatures.Mock();
            _ = context.Features.Returns(features);

            var requestData = new TestHttpRequestData(context);
            requestData.Headers.Add(headerName, correlationId);
            SetupHttpRequestFeature(context, features, requestData);

            var middleware = new FunctionsCorrelationMiddleware(
                scope.ServiceProvider.GetRequiredService<ILogger<FunctionsCorrelationMiddleware>>()
            );

            await middleware.Invoke(context, next).ConfigureAwait(false);

            async Task next(FunctionContext functionContext)
            {
                var factory = functionContext.InstanceServices.GetRequiredService<IHttpClientFactory>();
                using var client = factory.CreateClient(DownstreamClient);
                using var response = await client
                    .GetAsync(new Uri("https://downstream/"), cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Records the correlation relevant headers of every outgoing request which reached the primary handler.
    /// </summary>
    private sealed class OutgoingRecorder
    {
        private readonly ConcurrentQueue<IReadOnlyDictionary<string, string>> _requests = new();

        public IReadOnlyList<IReadOnlyDictionary<string, string>> Requests => [.. _requests];

        public void Record(HttpRequestMessage request)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in new[] { CorrelationConstants.HeaderName1, CorrelationConstants.HeaderName2 })
            {
                if (request.Headers.TryGetValues(name, out var values))
                {
                    headers[name] = string.Join(",", values);
                }
            }

            _requests.Enqueue(headers);
        }

        public string[] ValuesOf(string headerName) =>
            [.. Requests.Where(r => r.ContainsKey(headerName)).Select(r => r[headerName])];
    }

    private sealed class RecordingPrimaryHandler : HttpMessageHandler
    {
        private readonly OutgoingRecorder _recorder;

        public RecordingPrimaryHandler(OutgoingRecorder recorder) => _recorder = recorder;

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _recorder.Record(request);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(Send(request, cancellationToken));
    }
}
