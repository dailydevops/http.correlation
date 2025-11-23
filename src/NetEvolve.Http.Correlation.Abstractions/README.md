# NetEvolve.Http.Correlation.Abstractions

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.Abstractions)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)

Core abstractions and interfaces for HTTP correlation ID management across distributed systems.

## Overview

This package provides the foundational abstractions and interfaces for managing correlation IDs in HTTP-based applications. It defines the contracts that enable consistent correlation ID handling across different implementations and frameworks.

## Key Features

- **Core Interfaces**: Defines `IHttpCorrelationIdProvider` and `IHttpCorrelationAccessor` contracts
- **Standard Constants**: Provides `CorrelationConstants` with standard header names
- **Generator Support**: Includes source generators for common correlation ID providers
- **Framework Agnostic**: No dependencies on specific web frameworks
- **Multi-Framework Support**: Compatible with .NET 8.0, 9.0, and 10.0

## Installation

```bash
dotnet add package NetEvolve.Http.Correlation.Abstractions
```

## Core Interfaces

### IHttpCorrelationIdProvider

Interface for generating correlation IDs:

```csharp
public interface IHttpCorrelationIdProvider
{
    string GenerateCorrelationId();
}
```

### IHttpCorrelationAccessor

Interface for accessing the current correlation ID:

```csharp
public interface IHttpCorrelationAccessor
{
    string? CorrelationId { get; }
}
```

## Constants

The `CorrelationConstants` class provides standard header names:

- `CorrelationConstants.CorrelationIdHeaderName`: `"X-Correlation-ID"` (primary)
- `CorrelationConstants.AlternativeHeaderName`: `"X-Request-ID"` (alternative)

## Usage

This package is typically not used directly but serves as a dependency for implementation packages.

## Related Packages

### Implementation Packages

- **[NetEvolve.Http.Correlation.AspNetCore](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)** - ASP.NET Core middleware and services
- **[NetEvolve.Http.Correlation.HttpClient](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)** - HTTP client handler for correlation forwarding

### Provider Packages

- **[NetEvolve.Http.Correlation.Ulid](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid)** - ULID-based correlation ID provider
- **[NetEvolve.Http.Correlation.TestGenerator](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)** - Predictable provider for testing

## Dependencies

- None (pure abstractions)

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.