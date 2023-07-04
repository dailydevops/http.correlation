namespace Sample.WebApi.NET7;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NetEvolve.Http.Correlation;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        _ = builder.Services.AddControllers();

        // Add Http correlation support
        _ = builder.Services.AddHttpCorrelation().WithGuidGenerator();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        _ = app.UseHttpCorrelation();

        _ = app.UseHttpsRedirection();

        _ = app.UseAuthorization();

        _ = app.MapControllers();

        app.Run();
    }
}
