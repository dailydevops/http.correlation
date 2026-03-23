# NetEvolve.Http.Correlation.Azure.Functions

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.Azure.Functions)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Azure.Functions)

Azure Functions isolated worker middleware for managing HTTP correlation IDs across distributed requests.

## Overview

This package provides middleware and services to automatically handle correlation IDs in Azure Functions isolated worker applications. It ensures that incoming HTTP-triggered functions preserve incoming correlation headers and that new correlation IDs are generated when needed.

## Key Features

- **Automatic Correlation ID Handling**: Reads and preserves correlation IDs from incoming HTTP requests
- **Flexible Header Support**: Supports `X-Correlation-ID` (primary) and `X-Request-ID` (fallback)
- **Configurable ID Generation**: Pluggable correlation ID providers with default Sequential GUID implementation
- **Response Headers**: Includes correlation ID in response headers
- **Dependency Injection**: Seamless integration with Azure Functions DI container
- **Multi-Framework Support**: Compatible with .NET 8.0, 9.0, and 10.0

## Installation

```bash
dotnet add package NetEvolve.Http.Correlation.Azure.Functions
```

## Usage

### Basic Setup

Configure services and middleware in your `Program.cs`:

```csharp
using NetEvolve.Http.Correlation;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        // Add middleware to the pipeline
        builder.Services.AddHttpCorrelation();
        builder.UseHttpCorrelation();
    })
    .Build();

host.Run();
```

### Advanced Configuration

Use alternative ID generators:

```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        // Use GUID V7 (available in .NET 9.0+)
        builder.Services
            .AddHttpCorrelation()
            .WithGuidV7Generator();

        builder.UseHttpCorrelation();
    })
    .Build();
```

### Accessing Correlation IDs

Inject `IHttpCorrelationAccessor` to access the current correlation ID:

```csharp
public class MyFunction
{
    private readonly IHttpCorrelationAccessor _correlationAccessor;

    public MyFunction(IHttpCorrelationAccessor correlationAccessor)
    {
        _correlationAccessor = correlationAccessor;
    }

    [Function("MyFunction")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var correlationId = _correlationAccessor.CorrelationId;
        Console.WriteLine($"Processing request with correlation ID: {correlationId}");

        var response = req.CreateResponse(HttpStatusCode.OK);
        return response;
    }
}
```

## Related Packages

- **[NetEvolve.Http.Correlation.Abstractions](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)** - Core abstractions and interfaces
- **[NetEvolve.Http.Correlation.HttpClient](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)** - Correlation forwarding for HTTP clients
- **[NetEvolve.Http.Correlation.Ulid](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid)** - Alternative ULID-based provider
- **[NetEvolve.Http.Correlation.TestGenerator](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)** - Test-friendly provider

## Dependencies

- `NetEvolve.Http.Correlation.Abstractions`
- `Microsoft.Azure.Functions.Worker.Core`

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.
