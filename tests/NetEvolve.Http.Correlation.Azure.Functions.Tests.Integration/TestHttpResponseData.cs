namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

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
