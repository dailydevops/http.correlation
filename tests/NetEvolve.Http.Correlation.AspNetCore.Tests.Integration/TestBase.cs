namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Integration;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetEvolve.Http.Correlation.Abstractions;

public abstract class TestBase
{
    protected const string DefaultPath = "/";
    protected const string InvokePath = "/invoke";

    protected static async ValueTask<HttpResponseMessage> RunAsync(
        Action<IHttpCorrelationBuilder>? correlationBuilder = null,
        Action<IServiceCollection>? serviceBuilder = null,
        Action<HttpClient>? clientConfiguration = null,
        string requestPath = DefaultPath
    )
    {
        using var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                serviceBuilder?.Invoke(services);
                var builder = services.AddRouting().AddHttpCorrelation();
                correlationBuilder?.Invoke(builder);
            })
            .ConfigureWebHost(webBuilder =>
            {
                _ = webBuilder
                    .UseTestServer()
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
            })
            .Build();
        await host.StartAsync().ConfigureAwait(false);

        using var server = host.GetTestServer();
        using var client = server.CreateClient();

        clientConfiguration?.Invoke(client);

        var response = await client.GetAsync(new Uri(requestPath, UriKind.Relative)).ConfigureAwait(false);

        return response;
    }
}
