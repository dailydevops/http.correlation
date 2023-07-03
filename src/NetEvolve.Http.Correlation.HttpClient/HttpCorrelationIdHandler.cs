namespace NetEvolve.Http.Correlation.HttpClient;

using NetEvolve.Http.Correlation.Abstractions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// a <see cref="DelegatingHandler"/> which implements the correlation id support,
/// based on the <see cref="IHttpCorrelationAccessor"/> values.
/// </summary>
internal sealed class HttpCorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpCorrelationAccessor _correlationAccessor;

    public HttpCorrelationIdHandler(IHttpCorrelationAccessor correlationAccessor)
    {
        ArgumentNullException.ThrowIfNull(correlationAccessor);

        _correlationAccessor = correlationAccessor;
    }

    /// <inheritdoc />
    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var correlationId = _correlationAccessor.CorrelationId;
        var correlationHeader = _correlationAccessor.HeaderName;

        if (
            !string.IsNullOrWhiteSpace(correlationId)
            && !string.IsNullOrWhiteSpace(correlationHeader)
        )
        {
            _ = request.Headers.Remove(correlationId);
            request.Headers.Add(correlationHeader, correlationId);
        }

        return base.Send(request, cancellationToken);
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var correlationId = _correlationAccessor.CorrelationId;
        var correlationHeader = _correlationAccessor.HeaderName;

        if (
            !string.IsNullOrWhiteSpace(correlationId)
            && !string.IsNullOrWhiteSpace(correlationHeader)
        )
        {
            _ = request.Headers.Remove(correlationId);
            request.Headers.Add(correlationHeader, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
