# NetEvolve.Http.Correlation.Ulid

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.Ulid)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid)

ULID-based implementation of `IHttpCorrelationIdProvider` for sortable, time-based correlation IDs.

## Overview

This package provides a ULID (Universally Unique Lexicographically Sortable Identifier) implementation for generating correlation IDs. ULIDs combine the benefits of UUIDs with the lexicographic sortability of timestamps, making them ideal for distributed systems.

## Key Features

- **Time-Based Sorting**: ULIDs are lexicographically sortable by creation time
- **High Entropy**: 128-bit identifiers with 80 bits of randomness
- **URL-Safe**: Base32-encoded string representation
- **Efficient**: Faster generation and comparison than GUIDs
- **Multi-Framework Support**: Compatible with .NET 8.0, 9.0, and 10.0

## Installation

```bash
dotnet add package NetEvolve.Http.Correlation.Ulid
```

## Usage

### Basic Setup

Configure services to use ULID-based correlation IDs in your `Program.cs`:

```csharp
using NetEvolve.Http.Correlation;

var builder = WebApplication.CreateBuilder(args);

// Register correlation services with ULID provider
builder.Services
    .AddHttpCorrelation()
    .WithUlidGenerator(); // Use ULID provider, instead of default behavior.

var app = builder.Build();
app.UseHttpCorrelation();
app.Run();
```

### With HTTP Client

Combine with HTTP client correlation forwarding:

```csharp
builder.Services
    .AddHttpCorrelation()
    .WithUlidGenerator(); // Use ULID provider, instead of default behavior.

builder.Services
    .AddHttpClient("MyApiClient")
    .WithHttpCorrelation();
```

## ULID Format

ULIDs are 26-character case-insensitive strings:

```
01ARZ3NDEKTSV4RRFFQ69G5FAV
```

Structure:
- **10 characters**: Timestamp (milliseconds since Unix epoch)
- **16 characters**: Random component

## Benefits

- **Temporal Ordering**: Natural sorting by creation time
- **Shorter Representation**: 26 characters vs 36 for standard GUIDs
- **Better Database Performance**: More efficient indexing
- **No Coordination Required**: Distributed generation without collisions

## Related Packages

### Required Package

- **[NetEvolve.Http.Correlation.Abstractions](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)** - Core abstractions and interfaces

### Companion Packages

- **[NetEvolve.Http.Correlation.AspNetCore](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)** - ASP.NET Core middleware and services
- **[NetEvolve.Http.Correlation.HttpClient](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)** - HTTP client correlation forwarding
- **[NetEvolve.Http.Correlation.TestGenerator](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)** - Test-friendly provider

## Dependencies

- `NetEvolve.Http.Correlation.Abstractions`
- `Ulid` (NuGet package)

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.