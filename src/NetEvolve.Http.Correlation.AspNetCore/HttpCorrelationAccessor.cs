namespace NetEvolve.Http.Correlation.AspNetCore;

using Microsoft.AspNetCore.Http;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc />
internal sealed class HttpCorrelationAccessor : IHttpCorrelationAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _correlationId;

    public HttpCorrelationAccessor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public string CorrelationId
    {
        get => _correlationId ??= _httpContextAccessor.HttpContext!.TraceIdentifier;
        set => _correlationId = value;
    }

    /// <inheritdoc />
    public string HeaderName { get; set; } = CorrelationConstants.HeaderName1;
}
