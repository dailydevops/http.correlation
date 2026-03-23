namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEvolve.Http.Correlation.Abstractions;
using NSubstitute;

/// <summary>
/// Test infrastructure for <see cref="FunctionsCorrelationMiddleware"/> integration tests.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Invokes the middleware with the given configuration and returns the captured results.
    /// </summary>
    protected static async ValueTask<TestRunResult> RunAsync(
        Action<IHttpCorrelationBuilder>? correlationBuilder = null,
        Action<IServiceCollection>? serviceBuilder = null,
        Action<TestHttpRequestData>? requestSetup = null
    )
    {
        var services = new ServiceCollection().AddLogging();
        serviceBuilder?.Invoke(services);

        var builder = services.AddHttpCorrelation();
        correlationBuilder?.Invoke(builder);

        await using var serviceProvider = services.BuildServiceProvider();

        var context = Substitute.For<FunctionContext>();
        _ = context.InvocationId.Returns("test-invocation-id");

        using var scope = serviceProvider.CreateScope();
        _ = context.InstanceServices.Returns(scope.ServiceProvider);

        if (requestSetup is not null)
        {
            var requestData = new TestHttpRequestData(context);
            requestSetup(requestData);

            var httpRequestDataFeature = Substitute.For<IHttpRequestDataFeature>();

#pragma warning disable CA2012 // NSubstitute fluent setup: ValueTask is intercepted by the substitute proxy, not actually consumed here
            _ = httpRequestDataFeature
                .GetHttpRequestDataAsync(context)
                .Returns(new ValueTask<HttpRequestData?>(requestData));
#pragma warning restore CA2012

            var features = Substitute.For<IInvocationFeatures>();
            _ = features.Get<IHttpRequestDataFeature>().Returns(httpRequestDataFeature);
            _ = context.Features.Returns(features);
        }

        var middleware = new FunctionsCorrelationMiddleware(
            scope.ServiceProvider.GetRequiredService<ILogger<FunctionsCorrelationMiddleware>>()
        );

        var nextCalled = false;
        FunctionExecutionDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await middleware.Invoke(context, next).ConfigureAwait(false);

        var functionsAccessor = scope.ServiceProvider.GetRequiredService<FunctionsCorrelationAccessor>();

        return new TestRunResult(nextCalled, functionsAccessor.CorrelationId, functionsAccessor.HeaderName);
    }
}

/// <summary>
/// Concrete <see cref="HttpRequestData"/> implementation for integration tests.
/// </summary>
public sealed class TestHttpRequestData : HttpRequestData
{
    private readonly HttpHeadersCollection _headers = new();

    public TestHttpRequestData(FunctionContext functionContext)
        : base(functionContext) { }

    public override HttpHeadersCollection Headers => _headers;

    public override IReadOnlyCollection<IHttpCookie> Cookies => [];

    public override Uri Url => new Uri("https://test.example.com/api/test");

    public override IEnumerable<ClaimsIdentity> Identities => [];

    public override string Method => "GET";

    public override Stream Body => Stream.Null;

    public override HttpResponseData CreateResponse() => new TestHttpResponseData(FunctionContext);
}

/// <summary>
/// Concrete <see cref="HttpResponseData"/> implementation for integration tests.
/// </summary>
internal sealed class TestHttpResponseData : HttpResponseData
{
    public TestHttpResponseData(FunctionContext functionContext)
        : base(functionContext) { }

    public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();

    public override Stream Body { get; set; } = Stream.Null;

    public override HttpCookies Cookies => null!;
}

/// <summary>
/// The result of a test run through <see cref="FunctionsCorrelationMiddleware"/>.
/// </summary>
public sealed record TestRunResult(bool NextCalled, string CorrelationId, string HeaderName);
