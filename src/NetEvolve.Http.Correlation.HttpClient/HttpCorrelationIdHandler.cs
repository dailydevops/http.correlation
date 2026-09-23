namespace NetEvolve.Http.Correlation.HttpClient;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// a <see cref="DelegatingHandler"/> which implements the correlation id support,
/// based on the correlation of the request currently in flight.
/// </summary>
/// <remarks>
/// The handler is built and cached by <c>IHttpClientFactory</c> in its own dependency injection scope and shared
/// across requests, so it reads the correlation per call from <see cref="CorrelationContext"/> instead of holding
/// a request scoped service. Without a request in flight it leaves the headers untouched.
/// </remarks>
internal sealed class HttpCorrelationIdHandler : DelegatingHandler
{
    /// <inheritdoc />
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlation = CorrelationContext.Current;

        SetCorrelationId(request.Headers, correlation);

        var response = base.Send(request, cancellationToken);

        SetCorrelationId(response.Headers, correlation);

        return response;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlation = CorrelationContext.Current;

        SetCorrelationId(request.Headers, correlation);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        SetCorrelationId(response.Headers, correlation);

        return response;
    }

    private static void SetCorrelationId(HttpHeaders headers, CorrelationSnapshot? correlation)
    {
        if (correlation is null || string.IsNullOrWhiteSpace(correlation.CorrelationId))
        {
            return;
        }

        if (!headers.Contains(correlation.HeaderName))
        {
            headers.Add(correlation.HeaderName, correlation.CorrelationId);
        }
    }
}
