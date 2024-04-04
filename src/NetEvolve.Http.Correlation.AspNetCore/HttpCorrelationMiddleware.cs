namespace NetEvolve.Http.Correlation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NetEvolve.Http.Correlation.Abstractions;
using static CorrelationConstants;

internal sealed class HttpCorrelationMiddleware
{
    private readonly ILogger<HttpCorrelationMiddleware> _logger;
    private readonly RequestDelegate _next;

    public HttpCorrelationMiddleware(
        ILogger<HttpCorrelationMiddleware> logger,
        RequestDelegate next
    )
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string? correlationId = null;
        if (GetIdFromHeader(context, out var idValues, out var usedHeaderName))
        {
            correlationId = idValues.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = GeneratedId(context);
        }

        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(usedHeaderName))
            {
                context.Response.Headers.Append(usedHeaderName, correlationId);
            }

            return Task.CompletedTask;
        });

        var accessor = context.RequestServices.GetService<IHttpCorrelationAccessor>()!;
        accessor.HeaderName = usedHeaderName;

        var scopeProperties = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { usedHeaderName, correlationId }
        };

        using (_logger.BeginScope(scopeProperties))
        {
            await _next(context).ConfigureAwait(false);
        }
    }

    private static string GeneratedId(HttpContext context)
    {
        var correlationIdGenerator =
            context.RequestServices.GetService<IHttpCorrelationIdProvider>();

        return correlationIdGenerator is null
            ? context.TraceIdentifier
            : correlationIdGenerator.GenerateId();
    }

    private static bool GetIdFromHeader(
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
