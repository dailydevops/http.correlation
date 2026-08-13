namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

/// <summary>
/// Concrete <see cref="HttpRequestData"/> implementation for integration tests.
/// </summary>
public sealed class TestHttpRequestData : HttpRequestData
{
    public TestHttpRequestData(FunctionContext functionContext)
        : base(functionContext) { }

    public override HttpHeadersCollection Headers { get; } = [];

    public override IReadOnlyCollection<IHttpCookie> Cookies => [];

    public override Uri Url => new Uri("https://test.example.com/api/test");

    public override IEnumerable<ClaimsIdentity> Identities => [];

    public override string Method => "GET";

    public override Stream Body => Stream.Null;

    public override HttpResponseData CreateResponse() => new TestHttpResponseData(FunctionContext);
}
