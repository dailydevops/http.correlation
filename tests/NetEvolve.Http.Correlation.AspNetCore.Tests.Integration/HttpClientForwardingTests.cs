namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Integration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetEvolve.Http.Correlation;
using NetEvolve.Http.Correlation.AspNetCore;
using NetEvolve.Http.Correlation.HttpClient;
using TUnit.Assertions.Enums;
using TUnit.Assertions.Extensions;
using TUnit.Core;

/// <summary>
/// Reproduces https://github.com/dailydevops/http.correlation/issues/858 and
/// https://github.com/dailydevops/http.correlation/issues/859: outgoing calls made through
/// <c>WithHttpCorrelation()</c> from within incoming requests, with one host kept alive for several requests,
/// so the handler chain cached by <see cref="IHttpClientFactory"/> is reused exactly as in production.
/// </summary>
public class HttpClientForwardingTests
{
    private const string DownstreamClient = "downstream";
    private const string OutgoingAsyncPath = "/outgoing-async";
    private const string OutgoingSyncPath = "/outgoing-sync";

    [Test]
    public async Task SendAsync_SequentialRequests_ForwardEachRequestsOwnCorrelationId(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var fixture = await ForwardingHost.StartAsync(cancellationToken).ConfigureAwait(false);

        await fixture
            .SendAsync(OutgoingAsyncPath, CorrelationConstants.HeaderName1, "First", cancellationToken)
            .ConfigureAwait(false);
        await fixture
            .SendAsync(OutgoingAsyncPath, CorrelationConstants.HeaderName1, "Second", cancellationToken)
            .ConfigureAwait(false);

        var forwarded = fixture.Recorder.ValuesOf(CorrelationConstants.HeaderName1);

        _ = await Assert.That(forwarded).IsEquivalentTo(["First", "Second"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Send_SequentialRequests_ForwardEachRequestsOwnCorrelationId(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var fixture = await ForwardingHost.StartAsync(cancellationToken).ConfigureAwait(false);

        await fixture
            .SendAsync(OutgoingSyncPath, CorrelationConstants.HeaderName1, "First", cancellationToken)
            .ConfigureAwait(false);
        await fixture
            .SendAsync(OutgoingSyncPath, CorrelationConstants.HeaderName1, "Second", cancellationToken)
            .ConfigureAwait(false);

        var forwarded = fixture.Recorder.ValuesOf(CorrelationConstants.HeaderName1);

        _ = await Assert.That(forwarded).IsEquivalentTo(["First", "Second"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task SendAsync_ConcurrentRequests_ForwardEachRequestsOwnCorrelationId(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var fixture = await ForwardingHost.StartAsync(cancellationToken).ConfigureAwait(false);

        var incoming = Enumerable.Range(0, 16).Select(i => $"Request-{i:D2}").ToArray();

        await Task.WhenAll(
                incoming.Select(id =>
                    fixture.SendAsync(OutgoingAsyncPath, CorrelationConstants.HeaderName1, id, cancellationToken)
                )
            )
            .ConfigureAwait(false);

        var forwarded = fixture.Recorder.ValuesOf(CorrelationConstants.HeaderName1);

        _ = await Assert.That(forwarded).IsEquivalentTo(incoming);
    }

    [Test]
    public async Task SendAsync_IncomingAlternativeHeader_ForwardsUsingAlternativeHeader(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var fixture = await ForwardingHost.StartAsync(cancellationToken).ConfigureAwait(false);

        await fixture
            .SendAsync(OutgoingAsyncPath, CorrelationConstants.HeaderName2, "Alternative", cancellationToken)
            .ConfigureAwait(false);

        using (Assert.Multiple())
        {
            _ = await Assert
                .That(fixture.Recorder.ValuesOf(CorrelationConstants.HeaderName2))
                .IsEquivalentTo(["Alternative"]);
            _ = await Assert.That(fixture.Recorder.ValuesOf(CorrelationConstants.HeaderName1)).IsEmpty();
        }
    }

    [Test]
    public async Task SendAsync_MixedHeaderDialects_ForwardEachRequestsOwnHeaderAndId(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var fixture = await ForwardingHost.StartAsync(cancellationToken).ConfigureAwait(false);

        await fixture
            .SendAsync(OutgoingAsyncPath, CorrelationConstants.HeaderName1, "Primary", cancellationToken)
            .ConfigureAwait(false);
        await fixture
            .SendAsync(OutgoingAsyncPath, CorrelationConstants.HeaderName2, "Alternative", cancellationToken)
            .ConfigureAwait(false);

        using (Assert.Multiple())
        {
            _ = await Assert
                .That(fixture.Recorder.ValuesOf(CorrelationConstants.HeaderName1))
                .IsEquivalentTo(["Primary"]);
            _ = await Assert
                .That(fixture.Recorder.ValuesOf(CorrelationConstants.HeaderName2))
                .IsEquivalentTo(["Alternative"]);
        }
    }

    [Test]
    public async Task SendAsync_OutsideOfRequest_AddsNoCorrelationHeader(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var fixture = await ForwardingHost.StartAsync(cancellationToken).ConfigureAwait(false);

        // e.g. a hosted/background service: no HttpContext is in flight
        var factory = fixture.Services.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient(DownstreamClient);
        using var response = await client
            .GetAsync(new Uri("https://downstream/"), cancellationToken)
            .ConfigureAwait(false);

        var recorded = fixture.Recorder.Requests.Single();

        using (Assert.Multiple())
        {
            _ = await Assert.That(recorded.ContainsKey(CorrelationConstants.HeaderName1)).IsFalse();
            _ = await Assert.That(recorded.ContainsKey(CorrelationConstants.HeaderName2)).IsFalse();
        }
    }

    [Test]
    public async Task SendAsync_OutsideOfRequestAfterRequest_DoesNotLeakPreviousCorrelationId(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var fixture = await ForwardingHost.StartAsync(cancellationToken).ConfigureAwait(false);

        await fixture
            .SendAsync(OutgoingAsyncPath, CorrelationConstants.HeaderName1, "Request", cancellationToken)
            .ConfigureAwait(false);

        var factory = fixture.Services.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient(DownstreamClient);
        using var response = await client
            .GetAsync(new Uri("https://downstream/"), cancellationToken)
            .ConfigureAwait(false);

        var background = fixture.Recorder.Requests[^1];

        _ = await Assert.That(background.ContainsKey(CorrelationConstants.HeaderName1)).IsFalse();
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

    /// <summary>
    /// One host for the whole test, so that <see cref="IHttpClientFactory"/> reuses its cached handler chain.
    /// </summary>
    private sealed class ForwardingHost : IDisposable
    {
        private readonly IHost _host;
        private readonly TestServer _server;
        private readonly System.Net.Http.HttpClient _client;

        private ForwardingHost(IHost host)
        {
            _host = host;
            _server = host.GetTestServer();
            _client = _server.CreateClient();
        }

        public IServiceProvider Services => _host.Services;

        public OutgoingRecorder Recorder => _host.Services.GetRequiredService<OutgoingRecorder>();

        public static async Task<ForwardingHost> StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    _ = services.AddSingleton(new OutgoingRecorder()).AddRouting().AddHttpCorrelation();
                    _ = services
                        .AddHttpClient(DownstreamClient)
                        .WithHttpCorrelation()
                        .ConfigurePrimaryHttpMessageHandler(sp => new RecordingPrimaryHandler(
                            sp.GetRequiredService<OutgoingRecorder>()
                        ));
                })
                .ConfigureWebHost(webBuilder =>
                    webBuilder
                        .UseTestServer()
                        .Configure(app =>
                            app.UseHttpCorrelation()
                                .UseRouting()
                                .UseEndpoints(endpoints =>
                                {
                                    _ = endpoints.MapGet(OutgoingAsyncPath, CallDownstreamAsync);
                                    _ = endpoints.MapGet(OutgoingSyncPath, CallDownstreamSync);
                                })
                        )
                )
                .Build();

            await host.StartAsync(cancellationToken).ConfigureAwait(false);

            return new ForwardingHost(host);
        }

        public async Task SendAsync(
            string path,
            string headerName,
            string correlationId,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
            request.Headers.Add(headerName, correlationId);

            using var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
            _host.Dispose();
        }

        private static async Task CallDownstreamAsync(HttpContext context, IHttpClientFactory factory)
        {
            using var client = factory.CreateClient(DownstreamClient);
            using var response = await client
                .GetAsync(new Uri("https://downstream/"), context.RequestAborted)
                .ConfigureAwait(false);

            context.Response.StatusCode = (int)response.StatusCode;
        }

        private static Task CallDownstreamSync(HttpContext context, IHttpClientFactory factory)
        {
            using var client = factory.CreateClient(DownstreamClient);
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://downstream/"));
#pragma warning disable CA1849,S6966,VSTHRD103 // Deliberately exercising the synchronous Send path
            using var response = client.Send(request, context.RequestAborted);
#pragma warning restore CA1849,S6966,VSTHRD103

            context.Response.StatusCode = (int)response.StatusCode;
            return Task.CompletedTask;
        }
    }
}
