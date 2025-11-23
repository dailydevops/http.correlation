# NetEvolve.Http.Correlation

A comprehensive suite of NuGet packages for managing HTTP correlation IDs across distributed .NET applications. Enables request tracing and distributed logging using standard HTTP headers (`X-Correlation-ID`, or `X-Request-ID` if `X-Correlation-ID` is not present).

## Overview

This library provides a complete solution for implementing correlation IDs in your .NET applications, making it easy to track requests across microservices, logs, and distributed systems. It follows industry best practices and supports modern .NET versions (8.0, 9.0, and 10.0).

## Key Features

- **Standard HTTP Headers**: Support for `X-Correlation-ID` (primary) and `X-Request-ID` (fallback)
- **ASP.NET Core Integration**: Middleware for automatic correlation ID handling
- **HTTP Client Integration**: Automatic correlation ID forwarding in outgoing requests
- **Flexible ID Generation**: Pluggable providers (GUID, GUID V7, ULID, or custom)
- **Test-Friendly**: Dedicated test provider for predictable correlation IDs
- **Modern .NET**: Full support for .NET 8.0, 9.0, and 10.0
- **Source Generators**: Compile-time generation for optimal performance

## Packages

### Core Packages

#### NetEvolve.Http.Correlation.Abstractions
[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.Abstractions)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)

Core abstractions and interfaces for correlation ID management. Provides the foundational contracts (`IHttpCorrelationIdProvider`, `IHttpCorrelationAccessor`) and constants.

**[View Package Documentation →](https://github.com/dailydevops/http.correlation/blob/main/src/NetEvolve.Http.Correlation.Abstractions/README.md)**

#### NetEvolve.Http.Correlation.AspNetCore
[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.AspNetCore)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)

ASP.NET Core middleware and services for handling correlation IDs in web applications. Automatically reads, generates, and propagates correlation IDs.

**[View Package Documentation →](https://github.com/dailydevops/http.correlation/blob/main/src/NetEvolve.Http.Correlation.AspNetCore/README.md)**

#### NetEvolve.Http.Correlation.HttpClient
[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.HttpClient)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)

HTTP client delegating handler for forwarding correlation IDs in outgoing requests. Ensures correlation IDs flow through your distributed system.

**[View Package Documentation →](https://github.com/dailydevops/http.correlation/blob/main/src/NetEvolve.Http.Correlation.HttpClient/README.md)**

### Provider Packages

#### NetEvolve.Http.Correlation.Ulid
[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.Ulid)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid)

ULID-based correlation ID provider. Provides sortable, globally unique identifiers that are more compact than GUIDs.

**[View Package Documentation →](https://github.com/dailydevops/http.correlation/blob/main/src/NetEvolve.Http.Correlation.Ulid/README.md)**

#### NetEvolve.Http.Correlation.TestGenerator
[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.TestGenerator)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)

Predictable correlation ID provider for testing scenarios. Generates sequential, deterministic IDs for reliable unit and integration tests.

**[View Package Documentation →](https://github.com/dailydevops/http.correlation/blob/main/src/NetEvolve.Http.Correlation.TestGenerator/README.md)**

## Quick Start

### 1. Install Packages

```bash
# Core ASP.NET Core package
dotnet add package NetEvolve.Http.Correlation.AspNetCore

# HTTP Client package (for outgoing requests)
dotnet add package NetEvolve.Http.Correlation.HttpClient

# Optional: ULID provider
dotnet add package NetEvolve.Http.Correlation.Ulid
```

### 2. Configure Services

Add correlation services to your `Program.cs`:

```csharp
using NetEvolve.Http.Correlation;

var builder = WebApplication.CreateBuilder(args);

// Register correlation services
builder.Services.AddHttpCorrelation();

// Configure HTTP clients with correlation forwarding
builder.Services
    .AddHttpClient("MyApiClient")
    .WithHttpCorrelation();

var app = builder.Build();

// Add middleware to the pipeline (should be early)
app.UseHttpCorrelation();

app.MapControllers();
app.Run();
```

### 3. Access Correlation IDs

Inject `IHttpCorrelationAccessor` to access the current correlation ID:

```csharp
public class OrderService
{
    private readonly IHttpCorrelationAccessor _correlationAccessor;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IHttpCorrelationAccessor correlationAccessor,
        ILogger<OrderService> logger)
    {
        _correlationAccessor = correlationAccessor;
        _logger = logger;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        var correlationId = _correlationAccessor.CorrelationId;
        _logger.LogInformation(
            "Processing order {OrderId} with correlation ID {CorrelationId}",
            order.Id,
            correlationId);

        // Your business logic here
    }
}
```

## Advanced Configuration

### Using ULID Provider

```csharp
builder.Services
    .AddHttpCorrelation()
    .WithUlidGenerator();
```

### Custom Provider

Implement `IHttpCorrelationIdProvider` for custom ID generation:

```csharp
public class CustomCorrelationIdProvider : IHttpCorrelationIdProvider
{
    public string GenerateId()
    {
        return $"CUSTOM-{DateTime.UtcNow.Ticks}";
    }
}

// Register custom provider
builder.Services.AddHttpCorrelation();
builder.Services.AddSingleton<IHttpCorrelationIdProvider, CustomCorrelationIdProvider>();
```

## Use Cases

### Distributed Logging

Track requests across multiple services:

```csharp
// Service A (Entry Point)
app.UseHttpCorrelation(); // Generates or reads correlation ID
_logger.LogInformation("Request received"); // Logs with correlation ID

// Service B (Called by Service A)
httpClient.GetAsync("/api/data"); // Correlation ID automatically forwarded
_logger.LogInformation("Data retrieved"); // Same correlation ID in logs
```

### Microservices Tracing

Follow a single user request through your entire system:

```
User Request → API Gateway → Service A → Service B → Database
               [CorrelationId: abc123] flows through entire chain
```

### Testing

Use predictable correlation IDs in tests:

```csharp
builder.Services
    .AddHttpCorrelation()
    .WithTestGenerator(); // Generates predictable IDs

// Tests now have consistent correlation IDs
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.

## Repository

- **GitHub**: [dailydevops/http.correlation](https://github.com/dailydevops/http.correlation)
- **Issues**: [Report Issues](https://github.com/dailydevops/http.correlation/issues)
- **Releases**: [View Releases](https://github.com/dailydevops/http.correlation/releases)
