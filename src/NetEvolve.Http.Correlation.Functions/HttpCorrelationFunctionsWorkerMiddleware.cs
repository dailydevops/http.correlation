namespace NetEvolve.Http.Correlation.Functions;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NetEvolve.Http.Correlation.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static CorrelationConstants;

internal sealed class HttpCorrelationFunctionsWorkerMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var logger = context.GetLogger<HttpCorrelationFunctionsWorkerMiddleware>();
        var correlationId = StringValues.Empty;
        var usedHeaderName = HeaderName1;

        try
        {
            // Support HttpTrigger
            if (StringValues.IsNullOrEmpty(correlationId))
            {
                var httpRequest = await context.GetHttpRequestDataAsync().ConfigureAwait(false);
                if (httpRequest is not null)
                {
                    correlationId = GetCorrelationIdFromHttpRequest(
                        httpRequest,
                        out usedHeaderName
                    );
                }
            }

            if (StringValues.IsNullOrEmpty(correlationId))
            {
                correlationId = GenerateCorrelationId(context);
            }

            var scopeProperties = new Dictionary<string, object>
            {
                { usedHeaderName, correlationId.ToString() }
            };
            using (logger.BeginScope(scopeProperties))
            {
                var accessor = context.InstanceServices.GetService<IHttpCorrelationAccessor>();
                if (accessor is not null)
                {
                    accessor.HeaderName = usedHeaderName;
                }

                await next(context).ConfigureAwait(false);
            }
        }
        finally
        {
            var response = context.GetHttpResponseData();
            if (response is not null)
            {
                _ = response.Headers.Remove(usedHeaderName);
                response.Headers.Add(usedHeaderName, correlationId.ToString());
            }
        }
    }

    private static StringValues GenerateCorrelationId(FunctionContext context)
    {
        StringValues correlationId;
        var correlationIdGenerator =
            context.InstanceServices.GetService<IHttpCorrelationIdProvider>();

        correlationId = correlationIdGenerator is null
            ? context.InvocationId
            : correlationIdGenerator.GenerateId();
        return correlationId;
    }

    private static StringValues GetCorrelationIdFromHttpRequest(
        HttpRequestData request,
        out string usedHeaderName
    )
    {
        if (request.Headers.TryGetValues(HeaderName1, out var values) && values.Any())
        {
            usedHeaderName = HeaderName1;
            return values.FirstOrDefault();
        }
        else if (request.Headers.TryGetValues(HeaderName2, out values) && values.Any())
        {
            usedHeaderName = HeaderName2;
            return values.FirstOrDefault();
        }

        usedHeaderName = HeaderName1;

        return StringValues.Empty;
    }
}
