# NetEvolve.Http.Correlation.HttpClient

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.HttpClient)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)

HTTP client delegating handler for forwarding correlation IDs in outgoing requests.

## Overview

This package provides a `DelegatingHandler` implementation that automatically adds correlation IDs to outgoing HTTP requests. It ensures that correlation IDs flow through your distributed system by reading them from the current context and adding them to request headers.

## Key Features

- **Automatic Header Forwarding**: Adds correlation ID to outgoing HTTP requests
- **Context-Aware**: Reads the correlation ID and header of the request currently in flight, per call
- **Flexible Header Support**: Supports `X-Correlation-ID` (primary) and `X-Request-ID` (alternative)
- **Seamless Integration**: Works with `IHttpClientFactory` and `HttpClientBuilder`
- **Multi-Framework Support**: Compatible with .NET 8.0, 9.0, and 10.0

## Installation

```bash
dotnet add package NetEvolve.Http.Correlation.HttpClient
```

## Usage

### Basic Setup

Configure HTTP client with correlation forwarding in your `Program.cs`:

```csharp
using NetEvolve.Http.Correlation;

var builder = WebApplication.CreateBuilder(args);

// Register correlation services (from AspNetCore package)
builder.Services.AddHttpCorrelation();

// Configure HTTP client with correlation forwarding
builder.Services
    .AddHttpClient("MyApiClient")
    .WithHttpCorrelation();

var app = builder.Build();
app.UseHttpCorrelation();
app.Run();
```

### Named Clients

Add correlation handling to specific HTTP clients:

```csharp
builder.Services
    .AddHttpClient<IMyService, MyService>(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com");
    })
    .WithHttpCorrelation();
```

### Typed Clients

Works seamlessly with typed HTTP clients:

```csharp
public class MyApiClient
{
    private readonly HttpClient _httpClient;

    public MyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetDataAsync()
    {
        // Correlation ID is automatically added to this request
        return await _httpClient.GetStringAsync("/data");
    }
}

// Registration
builder.Services
    .AddHttpClient<MyApiClient>()
    .WithHttpCorrelation();
```

## How It Works

The `HttpCorrelationIdHandler` is a `DelegatingHandler` that:

1. Reads the correlation of the request currently in flight, as published by `UseHttpCorrelation()` (ASP.NET Core or Azure Functions)
2. Adds the correlation ID to the outgoing request headers, unless the request already carries it
3. Uses the same header the incoming request arrived with (`X-Correlation-ID` or `X-Request-ID`)

The correlation is read on every call rather than captured when the handler is created. `IHttpClientFactory` builds and caches handlers in its own dependency injection scope and shares them across requests, so each outgoing call carries the identifier of the request that made it. Outside of a request, for example in a hosted service or a timer triggered function, no correlation header is added.

## Related Packages

### Required Package

- **[NetEvolve.Http.Correlation.Abstractions](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)** - Core abstractions and interfaces

### Companion Packages

- **[NetEvolve.Http.Correlation.AspNetCore](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)** - ASP.NET Core middleware (publishes the correlation of incoming requests)
- **[NetEvolve.Http.Correlation.Azure.Functions](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Azure.Functions)** - Azure Functions middleware (publishes the correlation of HTTP triggered invocations)
- **[NetEvolve.Http.Correlation.Ulid](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid)** - ULID-based correlation ID provider
- **[NetEvolve.Http.Correlation.TestGenerator](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)** - Test-friendly provider

## Dependencies

- `NetEvolve.Http.Correlation.Abstractions`
- `Microsoft.AspNetCore.App` (Framework Reference)

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.