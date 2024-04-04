namespace NetEvolve.Http.Correlation;

using Microsoft.AspNetCore.Http;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc />
internal sealed class HttpCorrelationAccessor : IHttpCorrelationAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCorrelationAccessor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public string CorrelationId => _httpContextAccessor.HttpContext!.TraceIdentifier;

    /// <inheritdoc />
    public string? HeaderName { get; set; }
}
