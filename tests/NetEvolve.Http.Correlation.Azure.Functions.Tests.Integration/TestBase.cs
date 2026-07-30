namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEvolve.Http.Correlation.Abstractions;
using TUnit.Mocks;

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
            var context = FunctionContext.Mock();
            _ = context.InvocationId.Returns("test-invocation-id");

            var scope = serviceProvider.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                _ = context.InstanceServices.Returns(scope.ServiceProvider);

                // Always required: FunctionContextHttpRequestExtensions.GetHttpRequestDataAsync() dereferences
                // context.Features directly (no null-check), so an unconfigured (null) Features property throws
                // even when there is no HTTP request to set up.
                var features = new TestInvocationFeatures();
                _ = context.Features.Returns(features);

                if (requestSetup is not null)
                {
                    var requestData = new TestHttpRequestData(context);
                    requestSetup(requestData);
                    SetupHttpRequestFeature(context, features, requestData);
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

    // See TestInvocationFeatures.cs: the middleware requests the internal Microsoft.Azure.Functions.Worker type
    // IFunctionBindingsFeature indirectly via context.GetInvocationResult(). This assembly cannot name that type,
    // so TestInvocationFeatures.Get<T>() falls back to a DispatchProxy stub for any unregistered interface T.
    private static void SetupHttpRequestFeature(
        Mock<FunctionContext> context,
        TestInvocationFeatures features,
        TestHttpRequestData requestData
    )
    {
        var httpRequestDataFeature = IHttpRequestDataFeature.Mock();

#pragma warning disable CA2012 // TUnit.Mocks fluent setup: ValueTask is intercepted by the mock proxy, not awaited here
        // context.Object: a single implicit conversion to Arg<FunctionContext> is allowed, but chaining
        // Mock<FunctionContext> -> FunctionContext -> Arg<FunctionContext> is two, which C# won't apply implicitly.
        _ = httpRequestDataFeature.GetHttpRequestDataAsync(context.Object).Returns(requestData);
#pragma warning restore CA2012

        // The explicit <IHttpRequestDataFeature> type argument is load-bearing: it keys the dictionary on the
        // interface type. If inferred from httpRequestDataFeature's concrete mock type instead, Get<IHttpRequestDataFeature>()
        // below would miss and silently fall back to a DispatchProxy stub (see risk in TestInvocationFeatures.cs).
        features.Set<IHttpRequestDataFeature>(httpRequestDataFeature);
    }
}
