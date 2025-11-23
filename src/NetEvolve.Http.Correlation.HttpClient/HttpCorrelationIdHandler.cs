namespace NetEvolve.Http.Correlation;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NetEvolve.Http.Correlation.Abstractions;

/// <summary>
/// a <see cref="DelegatingHandler"/> which implements the correlation id support,
/// based on the <see cref="IHttpCorrelationAccessor"/> values.
/// </summary>
internal sealed class HttpCorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpCorrelationAccessor _correlationAccessor;

    public HttpCorrelationIdHandler(IHttpCorrelationAccessor correlationAccessor) =>
        _correlationAccessor = correlationAccessor;

    /// <inheritdoc />
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetCorrelationId(request);

        var response = base.Send(request, cancellationToken);

        SetCorrelationId(response);

        return response;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        SetCorrelationId(request);

        var respose = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        SetCorrelationId(respose);

        return respose;
    }

    private void SetCorrelationId(HttpRequestMessage request)
    {
        var correlationId = _correlationAccessor.CorrelationId;
        var correlationHeader = _correlationAccessor.HeaderName;

        if (!request.Headers.Contains(correlationHeader))
        {
            request.Headers.Add(correlationHeader, correlationId);
        }
    }

    private void SetCorrelationId(HttpResponseMessage respose)
    {
        var correlationId = _correlationAccessor.CorrelationId;
        var correlationHeader = _correlationAccessor.HeaderName;

        if (!respose.Headers.Contains(correlationHeader))
        {
            respose.Headers.Add(correlationHeader, correlationId);
        }
    }
}
