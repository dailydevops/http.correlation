# NetEvolve.Http.Correlation.Ulid.ByteAether

[![Nuget](https://img.shields.io/nuget/v/NetEvolve.Http.Correlation.Ulid.ByteAether)](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Ulid.ByteAether)

ULID-based implementation of `IHttpCorrelationIdProvider` for sortable, time-based correlation IDs.

## Overview

This package provides a ULID (Universally Unique Lexicographically Sortable Identifier) implementation for generating correlation IDs. ULIDs combine the benefits of UUIDs with the lexicographic sortability of timestamps, making them ideal for distributed systems.

## Key Features

- **Monotonic Time-Based Sorting**: ULIDs are strictly ordered lexicographically, guaranteeing sequential ordering even for multiple IDs generated within the exact same millisecond.
- **High Entropy**: 128-bit identifiers with 80 bits of randomness.
- **URL-Safe**: Base32-encoded string representation.
- **Efficient & Lock-Free**: Uses ByteAether's lock-free CAS operations for high-throughput, thread-safe monotonic generation.
- **Multi-Framework Support**: Compatible with .NET 8.0, 9.0, and 10.0.

## Installation

```bash
dotnet add package NetEvolve.Http.Correlation.Ulid.ByteAether
```

## Usage

### Basic Setup

Configure services to use ULID-based correlation IDs in your `Program.cs`:

```csharp
using NetEvolve.Http.Correlation;
using NetEvolve.Http.Correlation.Ulid.ByteAether;

var builder = WebApplication.CreateBuilder(args);

// Register correlation services with ByteAether ULID provider
builder.Services
    .AddHttpCorrelation()
    .WithUlidGenerator(); // Use the ByteAether ULID provider instead of default behavior.

var app = builder.Build();
app.UseHttpCorrelation();
app.Run();
```

### With HTTP Client

Combine with HTTP client correlation forwarding:

```csharp
builder.Services
    .AddHttpCorrelation()
    .WithUlidGenerator(); // Use the ByteAether ULID provider instead of default behavior.

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

- **Strict Temporal & Monotonic Ordering**: Unlike standard ULID implementations that produce randomly ordered IDs within the same millisecond, this package leverages `ByteAether.Ulid` to guarantee strictly ordered monotonic ULIDs even during high-throughput sub-millisecond bursts.
- **Shorter Representation**: 26 characters vs 36 for standard GUIDs.
- **Better Database Performance**: Monotonic time-first ordering minimizes B-Tree index page fragmentation during high-volume database writes.
- **No Central Coordinator Required**: Thread-safe monotonic generation within a process; cross-node ordering is not guaranteed and collisions remain probabilistic.

## Related Packages

### Required Package

- **[NetEvolve.Http.Correlation.Abstractions](https://www.nuget.org/packages/NetEvolve.Http.Correlation.Abstractions)** - Core abstractions and interfaces

### Companion Packages

- **[NetEvolve.Http.Correlation.AspNetCore](https://www.nuget.org/packages/NetEvolve.Http.Correlation.AspNetCore)** - ASP.NET Core middleware and services
- **[NetEvolve.Http.Correlation.HttpClient](https://www.nuget.org/packages/NetEvolve.Http.Correlation.HttpClient)** - HTTP client correlation forwarding
- **[NetEvolve.Http.Correlation.TestGenerator](https://www.nuget.org/packages/NetEvolve.Http.Correlation.TestGenerator)** - Test-friendly provider

## Dependencies

- `NetEvolve.Http.Correlation.Abstractions`
- `ByteAether.Ulid` (NuGet package)

## License

Licensed under the MIT License. See [LICENSE](https://github.com/dailydevops/http.correlation/blob/main/LICENSE) for details.