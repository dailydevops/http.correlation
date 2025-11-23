# NetEvolve.Http.Correlation.TestGenerator

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.TestGenerator)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)

Test-friendly implementation of `IHttpCorrelationIdProvider` with predictable correlation ID generation.

## Overview

This package provides a deterministic correlation ID provider specifically designed for testing scenarios. It generates predictable, sequential correlation IDs that make test assertions easier and test output more readable.

## Key Features

- **Predictable IDs**: Generates sequential, deterministic correlation IDs
- **Test-Friendly**: Simplifies test assertions and debugging
- **Configurable Format**: Customize ID format and prefix
- **Reset Capability**: Reset counter between test runs
- **Multi-Framework Support**: Compatible with .NET 8.0, 9.0, and 10.0

## Installation

```bash
dotnet add package NetEvolve.Http.Correlation.TestGenerator
```

## Usage

### Basic Setup

Configure services to use test correlation IDs in your test setup:

```csharp
using NetEvolve.Http.Correlation;
using Microsoft.AspNetCore.Mvc.Testing;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddHttpCorrelation()
                    .WithTestGenerator(); // Use test provider
            });
        });
    }

    [Fact]
    public async Task Request_ShouldHaveCorrelationId()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/data");

        // Assert
        response.Headers.Should().Contain(h => 
            h.Key == "X-Correlation-ID" && 
            h.Value.First() == "test-00000001");
    }
}
```

### Custom Format

Configure custom test ID:

```csharp
services
    .AddHttpCorrelation()
    .WithTestGenerator("my-custom-test-id");
// Always generates: my-custom-test-id
```

## ID Format

Default: `Generated_Test_Id`

Custom example:
```csharp
.WithTestGenerator("my-test-123")
// Always generates: my-test-123
```

## Use Cases

- **Integration Tests**: Verify correlation ID flow through the system
- **Unit Tests**: Mock correlation ID behavior with predictable values
- **E2E Tests**: Trace requests with readable correlation IDs
- **Debug Sessions**: Identify test requests in logs

## Related Packages

### Required Package

- **[NetEvolve.Http.Correlation.Abstractions](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)** - Core abstractions and interfaces

### Companion Packages

- **[NetEvolve.Http.Correlation.AspNetCore](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)** - ASP.NET Core middleware and services
- **[NetEvolve.Http.Correlation.HttpClient](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)** - HTTP client correlation forwarding
- **[NetEvolve.Http.Correlation.Ulid](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid)** - ULID-based provider for production

## Dependencies

- `NetEvolve.Http.Correlation.Abstractions`

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.