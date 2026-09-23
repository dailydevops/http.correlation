namespace NetEvolve.Http.Correlation.AspNetCore;

using Microsoft.AspNetCore.Http;
using NetEvolve.Http.Correlation.Abstractions;

/// <inheritdoc />
internal sealed class HttpCorrelationAccessor : IHttpCorrelationAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _explicitCorrelationId;

    public HttpCorrelationAccessor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public string CorrelationId
    {
        // Read live on every access: an instance that outlives its request must not keep answering for it.
        get => _explicitCorrelationId ?? _httpContextAccessor.HttpContext?.TraceIdentifier!;
        set => _explicitCorrelationId = value;
    }

    /// <inheritdoc />
    public string HeaderName { get; set; } = CorrelationConstants.HeaderName1;
}
