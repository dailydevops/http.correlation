# NetEvolve.Http.Correlation.AspNetCore

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.AspNetCore)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)

ASP.NET Core middleware for managing HTTP correlation IDs across distributed requests.

## Overview

This package provides middleware and services to automatically handle correlation IDs in ASP.NET Core applications. It ensures that incoming requests with correlation headers are preserved and that new correlation IDs are generated when needed.

## Key Features

- **Automatic Correlation ID Handling**: Reads and preserves correlation IDs from incoming requests
- **Flexible Header Support**: Supports `X-Correlation-ID` (primary) and `X-Request-ID` (fallback)
- **Configurable ID Generation**: Pluggable correlation ID providers with default Sequential GUID implementation
- **Response Headers**: Optionally includes correlation ID in response headers
- **Dependency Injection**: Seamless integration with ASP.NET Core DI container
- **Multi-Framework Support**: Compatible with .NET 8.0, 9.0, and 10.0

## Installation

```bash
dotnet add package NetEvolve.Http.Correlation.AspNetCore
```

## Usage

### Basic Setup

Configure services and middleware in your `Program.cs`:

```csharp
using NetEvolve.Http.Correlation;

var builder = WebApplication.CreateBuilder(args);

// Register correlation services
builder.Services.AddHttpCorrelation();

var app = builder.Build();

// Add middleware to the pipeline
app.UseHttpCorrelation();

app.Run();
```

### Advanced Configuration

Use alternative ID generators:

```csharp
// Use GUID V7 (available in .NET 9.0+)
builder.Services
    .AddHttpCorrelation()
    .WithGuidV7Generator();

// Use standard GUID
builder.Services
    .AddHttpCorrelation()
    .WithGuidGenerator();
```

### Accessing Correlation IDs

Inject `IHttpCorrelationAccessor` to access the current correlation ID:

```csharp
public class MyService
{
    private readonly IHttpCorrelationAccessor _correlationAccessor;

    public MyService(IHttpCorrelationAccessor correlationAccessor)
    {
        _correlationAccessor = correlationAccessor;
    }

    public void LogRequest()
    {
        var correlationId = _correlationAccessor.CorrelationId;
        Console.WriteLine($"Processing request with correlation ID: {correlationId}");
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
- `Microsoft.AspNetCore.App` (Framework Reference)

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.