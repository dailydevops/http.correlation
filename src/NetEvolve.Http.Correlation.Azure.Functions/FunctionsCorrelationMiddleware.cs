namespace NetEvolve.Http.Correlation.Azure.Functions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEvolve.Http.Correlation.Abstractions;
using static NetEvolve.Http.Correlation.CorrelationConstants;

internal sealed class FunctionsCorrelationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<FunctionsCorrelationMiddleware> _logger;

    public FunctionsCorrelationMiddleware(ILogger<FunctionsCorrelationMiddleware> logger) => _logger = logger;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        var accessor = context.InstanceServices.GetRequiredService<FunctionsCorrelationAccessor>();
        accessor.Context = context;

        var httpRequestData = await context.GetHttpRequestDataAsync().ConfigureAwait(false);

        if (httpRequestData is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!GetIdFromHeader(httpRequestData, out var correlationId, out var usedHeaderName))
        {
            correlationId = GeneratedId(context);
        }

        accessor.CorrelationId = correlationId;
        accessor.HeaderName = usedHeaderName;

        var scopeProperties = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { usedHeaderName, correlationId },
        };

        using (_logger.BeginScope(scopeProperties))
        {
            await next(context).ConfigureAwait(false);

            // Add correlation ID to response headers after function execution
            var httpResponseData = context.GetInvocationResult().Value as HttpResponseData;
            if (httpResponseData is not null && !httpResponseData.Headers.Contains(usedHeaderName))
            {
                httpResponseData.Headers.Add(usedHeaderName, correlationId);
            }
        }
    }

    private static string GeneratedId(FunctionContext context) =>
        context.InstanceServices.GetService<IHttpCorrelationIdProvider>()?.GenerateId() ?? context.InvocationId;

    private static bool GetIdFromHeader(
        HttpRequestData httpRequestData,
        out string correlationId,
        out string usedHeaderName
    )
    {
        usedHeaderName = HeaderName1;
        correlationId = string.Empty;

        if (httpRequestData.Headers.TryGetValues(HeaderName1, out var headerValues1))
        {
            var firstValue = headerValues1.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstValue))
            {
                correlationId = firstValue;
                return true;
            }
        }

        if (httpRequestData.Headers.TryGetValues(HeaderName2, out var headerValues2))
        {
            var firstValue = headerValues2.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstValue))
            {
                correlationId = firstValue;
                usedHeaderName = HeaderName2;
                return true;
            }
        }

        return false;
    }
}
