namespace NetEvolve.Http.Correlation.HttpClient.Tests.Unit;

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NetEvolve.Http.Correlation.Abstractions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class HttpCorrelationIdHandlerTests
{
    [Test]
    public async Task Send_AddsCorrelationIdToRequest()
    {
        var correlationId = "test-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor) { InnerHandler = new TestMessageHandler() };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

#pragma warning disable CA1849,S6966,VSTHRD103 // Call async methods when in an async method
        using var response = client.Send(request);
#pragma warning restore CA1849,S6966,VSTHRD103 // Call async methods when in an async method

        _ = await Assert.That(request.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(request.Headers.GetValues(headerName)).Contains(correlationId);
    }

    [Test]
    public async Task Send_AddsCorrelationIdToResponse()
    {
        var correlationId = "test-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor) { InnerHandler = new TestMessageHandler() };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

#pragma warning disable CA1849, S6966, VSTHRD103 // Call async methods when in an async method
        using var response = client.Send(request);
#pragma warning restore CA1849,S6966,VSTHRD103 // Call async methods when in an async method

        _ = await Assert.That(response.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(response.Headers.GetValues(headerName)).Contains(correlationId);
    }

    [Test]
    public async Task Send_DoesNotOverrideExistingRequestHeader()
    {
        var correlationId = "test-correlation-id";
        var existingCorrelationId = "existing-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor) { InnerHandler = new TestMessageHandler() };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
        request.Headers.Add(headerName, existingCorrelationId);

#pragma warning disable CA1849,S6966,VSTHRD103 // Call async methods when in an async method
        using var response = client.Send(request);
#pragma warning restore CA1849,S6966,VSTHRD103 // Call async methods when in an async method

        _ = await Assert.That(request.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(request.Headers.GetValues(headerName)).Contains(existingCorrelationId);
        _ = await Assert.That(request.Headers.GetValues(headerName)).DoesNotContain(correlationId);
    }

    [Test]
    public async Task Send_DoesNotOverrideExistingResponseHeader()
    {
        var correlationId = "test-correlation-id";
        var existingCorrelationId = "existing-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor)
        {
            InnerHandler = new TestMessageHandler(existingCorrelationId, headerName),
        };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

#pragma warning disable CA1849,S6966,VSTHRD103 // Call async methods when in an async method
        using var response = client.Send(request);
#pragma warning restore CA1849,S6966,VSTHRD103 // Call async methods when in an async method

        _ = await Assert.That(response.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(response.Headers.GetValues(headerName)).Contains(existingCorrelationId);
        _ = await Assert.That(response.Headers.GetValues(headerName)).DoesNotContain(correlationId);
    }

    [Test]
    public async Task SendAsync_AddsCorrelationIdToRequest()
    {
        var correlationId = "test-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor) { InnerHandler = new TestMessageHandler() };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        using var response = await client.SendAsync(request);

        _ = await Assert.That(request.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(request.Headers.GetValues(headerName)).Contains(correlationId);
    }

    [Test]
    public async Task SendAsync_AddsCorrelationIdToResponse()
    {
        var correlationId = "test-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor) { InnerHandler = new TestMessageHandler() };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        using var response = await client.SendAsync(request);

        _ = await Assert.That(response.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(response.Headers.GetValues(headerName)).Contains(correlationId);
    }

    [Test]
    public async Task SendAsync_DoesNotOverrideExistingRequestHeader()
    {
        var correlationId = "test-correlation-id";
        var existingCorrelationId = "existing-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor) { InnerHandler = new TestMessageHandler() };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
        request.Headers.Add(headerName, existingCorrelationId);

        using var response = await client.SendAsync(request);

        _ = await Assert.That(request.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(request.Headers.GetValues(headerName)).Contains(existingCorrelationId);
        _ = await Assert.That(request.Headers.GetValues(headerName)).DoesNotContain(correlationId);
    }

    [Test]
    public async Task SendAsync_DoesNotOverrideExistingResponseHeader()
    {
        var correlationId = "test-correlation-id";
        var existingCorrelationId = "existing-correlation-id";
        var headerName = "X-Correlation-ID";
        var accessor = new TestCorrelationAccessor(correlationId, headerName);

        using var handler = new HttpCorrelationIdHandler(accessor)
        {
            InnerHandler = new TestMessageHandler(existingCorrelationId, headerName),
        };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        using var response = await client.SendAsync(request);

        _ = await Assert.That(response.Headers.Contains(headerName)).IsTrue();
        _ = await Assert.That(response.Headers.GetValues(headerName)).Contains(existingCorrelationId);
        _ = await Assert.That(response.Headers.GetValues(headerName)).DoesNotContain(correlationId);
    }

    private sealed class TestCorrelationAccessor : IHttpCorrelationAccessor
    {
        public string CorrelationId { get; set; }
        public string HeaderName { get; set; }

        public TestCorrelationAccessor(string correlationId, string headerName)
        {
            CorrelationId = correlationId;
            HeaderName = headerName;
        }
    }

    private sealed class TestMessageHandler : HttpMessageHandler
    {
        private readonly string? _correlationId;
        private readonly string? _headerName;

        public TestMessageHandler(string? correlationId = null, string? headerName = null)
        {
            _correlationId = correlationId;
            _headerName = headerName;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            if (_correlationId is not null && _headerName is not null)
            {
                response.Headers.Add(_headerName, _correlationId);
            }

            return response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            if (_correlationId is not null && _headerName is not null)
            {
                response.Headers.Add(_headerName, _correlationId);
            }

            return Task.FromResult(response);
        }
    }
}
