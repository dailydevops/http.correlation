namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEvolve.Http.Correlation.Abstractions;
using NSubstitute;

/// <summary>
/// Test infrastructure for <see cref="FunctionsCorrelationMiddleware"/> integration tests.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Invokes the middleware with the given configuration and returns the captured results.
    /// </summary>
    protected static async ValueTask<TestRunResult> RunAsync(
        Action<IHttpCorrelationBuilder>? correlationBuilder = null,
        Action<IServiceCollection>? serviceBuilder = null,
        Action<TestHttpRequestData>? requestSetup = null
    )
    {
        var services = new ServiceCollection().AddLogging();
        serviceBuilder?.Invoke(services);

        var builder = services.AddHttpCorrelation();
        correlationBuilder?.Invoke(builder);

        var serviceProvider = services.BuildServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            var context = Substitute.For<FunctionContext>();
            _ = context.InvocationId.Returns("test-invocation-id");

            var scope = serviceProvider.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                _ = context.InstanceServices.Returns(scope.ServiceProvider);

                if (requestSetup is not null)
                {
                    var requestData = new TestHttpRequestData(context);
                    requestSetup(requestData);

                    var httpRequestDataFeature = Substitute.For<IHttpRequestDataFeature>();

#pragma warning disable CA2012 // NSubstitute fluent setup: ValueTask is intercepted by the substitute proxy, not actually consumed here
                    _ = httpRequestDataFeature
                        .GetHttpRequestDataAsync(context)
                        .Returns(new ValueTask<HttpRequestData?>(requestData));
#pragma warning restore CA2012

                    var features = Substitute.For<IInvocationFeatures>();
                    _ = features.Get<IHttpRequestDataFeature>().Returns(httpRequestDataFeature);
                    _ = context.Features.Returns(features);
                }

                var middleware = new FunctionsCorrelationMiddleware(
                    scope.ServiceProvider.GetRequiredService<ILogger<FunctionsCorrelationMiddleware>>()
                );

                var nextCalled = false;

                await middleware.Invoke(context, next).ConfigureAwait(false);

                var functionsAccessor = scope.ServiceProvider.GetRequiredService<FunctionsCorrelationAccessor>();

                return new TestRunResult(nextCalled, functionsAccessor.CorrelationId, functionsAccessor.HeaderName);

                Task next(FunctionContext _)
                {
                    nextCalled = true;
                    return Task.CompletedTask;
                }
            }
        }
    }
}
