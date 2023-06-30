namespace NetEvolve.Http.Correlation.AspNetCore;

using Microsoft.AspNetCore.Http;
using NetEvolve.Http.Correlation.Abstractions;
using System;

/// <inheritdoc />
internal sealed class HttpCorrelationAccessor : IHttpCorrelationAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCorrelationAccessor(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor is null)
        {
            throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string GetCorrelationId() => _httpContextAccessor.HttpContext!.TraceIdentifier;
}
