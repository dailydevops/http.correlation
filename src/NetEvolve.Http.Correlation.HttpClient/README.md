# NetEvolve.Http.Correlation.HttpClient

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.HttpClient)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)

HTTP client delegating handler for forwarding correlation IDs in outgoing requests.

## Overview

This package provides a `DelegatingHandler` implementation that automatically adds correlation IDs to outgoing HTTP requests. It ensures that correlation IDs flow through your distributed system by reading them from the current context and adding them to request headers.

## Key Features

- **Automatic Header Forwarding**: Adds correlation ID to outgoing HTTP requests
- **Context-Aware**: Reads correlation ID from `IHttpCorrelationAccessor`
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

1. Retrieves the current correlation ID from `IHttpCorrelationAccessor`
2. Adds the correlation ID to the outgoing request headers
3. Respects the configured header preference (primary or alternative)

## Related Packages

### Required Package

- **[NetEvolve.Http.Correlation.Abstractions](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)** - Core abstractions and interfaces

### Companion Packages

- **[NetEvolve.Http.Correlation.AspNetCore](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)** - ASP.NET Core middleware (required for `IHttpCorrelationAccessor`)
- **[NetEvolve.Http.Correlation.Ulid](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid)** - ULID-based correlation ID provider
- **[NetEvolve.Http.Correlation.TestGenerator](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)** - Test-friendly provider

## Dependencies

- `NetEvolve.Http.Correlation.Abstractions`
- `Microsoft.AspNetCore.App` (Framework Reference)

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.