namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Integration;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation.Abstractions;

public abstract class TestBase
{
    protected const string DefaultPath = "/";
    protected const string InvokePath = "/invoke";

    protected async ValueTask<HttpResponseMessage> RunAsync(
        Action<IHttpCorrelationBuilder>? correlationBuilder = null,
        Action<IServiceCollection>? serviceBuilder = null,
        Action<HttpClient>? clientConfiguration = null,
        string requestPath = DefaultPath
    )
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                serviceBuilder?.Invoke(services);
                var builder = services.AddRouting().AddHttpCorrelation();
                correlationBuilder?.Invoke(builder);
            })
            .Configure(app =>
            {
                _ = app.UseHttpCorrelation()
                    .UseRouting()
                    .UseEndpoints(endpoints =>
                    {
                        _ = endpoints.MapGet(
                            DefaultPath,
                            async context => await context.Response.WriteAsync("Hello World!")
                        );
                        _ = endpoints.MapGet(
                            InvokePath,
                            async (HttpContext context, IHttpCorrelationAccessor accessor) =>
                                await context.Response.WriteAsync(accessor.CorrelationId)
                        );
                    });
            });

        using (var server = new TestServer(builder))
        {
            var client = server.CreateClient();

            clientConfiguration?.Invoke(client);

            var response = await client
                .GetAsync(new Uri(requestPath, UriKind.Relative))
                .ConfigureAwait(false);

            return response;
        }
    }
}
