namespace NetEvolve.Http.Correlation.AspNetCore;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NetEvolve.Http.Correlation.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static CorrelationConstants;

internal sealed class HttpCorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpCorrelationMiddleware> _logger;

    public HttpCorrelationMiddleware(
        RequestDelegate next,
        ILogger<HttpCorrelationMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? correlationId = null;
        if (
            GetCorrelationIdFromHeader(context, out var correlationIdValues, out var usedHeaderName)
        )
        {
            correlationId = correlationIdValues.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            var correlationIdGenerator =
                context.RequestServices.GetService<IHttpCorrelationIdProvider>();

            correlationId = correlationIdGenerator is null
                ? context.TraceIdentifier
                : correlationIdGenerator.GenerateId();
        }

        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(usedHeaderName))
            {
                context.Response.Headers.Add(usedHeaderName, correlationId);
            }

            return Task.CompletedTask;
        });

        var scopeProperties = new Dictionary<string, object> { { usedHeaderName, correlationId } };

        using (_logger.BeginScope(scopeProperties))
        {
            await _next(context).ConfigureAwait(false);
        }
    }

    private static bool GetCorrelationIdFromHeader(
        HttpContext context,
        out StringValues correlationId,
        out string usedHeaderName
    )
    {
        usedHeaderName = HeaderName1;

        if (
            context.Request.Headers.TryGetValue(HeaderName1, out correlationId)
            && !StringValues.IsNullOrEmpty(correlationId)
        )
        {
            return true;
        }

        if (
            context.Request.Headers.TryGetValue(HeaderName2, out correlationId)
            && !StringValues.IsNullOrEmpty(correlationId)
        )
        {
            usedHeaderName = HeaderName2;
            return true;
        }

        correlationId = default;

        return false;
    }
}
