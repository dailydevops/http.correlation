---
authors:
  - Martin Stühmer

applyTo:
  - "src/NetEvolve.Http.Correlation.Abstractions/**/*.cs"
  - "src/NetEvolve.Http.Correlation.AspNetCore/**/*.cs"
  - "src/NetEvolve.Http.Correlation.Azure.Functions/**/*.cs"
  - "src/NetEvolve.Http.Correlation.HttpClient/**/*.cs"

created: 2026-09-23

lastModified: 2026-09-23

state: proposed

instructions: |
  Outgoing correlation (HttpCorrelationIdHandler) MUST NOT capture IHttpCorrelationAccessor or any other scoped service through its constructor, because IHttpClientFactory builds and caches handlers in its own DI scope.
  The incoming middlewares (AspNetCore, Azure Functions) publish an immutable snapshot (correlation id + header name) into an internal AsyncLocal ambient context for the duration of the request; the handler reads that snapshot per call and does nothing when it is absent.
  Accessor implementations MUST NOT memoise values derived from HttpContext or FunctionContext; only an explicitly assigned value may be stored.
---

# Decision: Ambient Correlation Context for Outgoing HTTP Calls

Outgoing HTTP calls read the correlation id and header name from an ambient context. Each incoming middleware sets that context for the current request. The handler never takes a scoped `IHttpCorrelationAccessor` in its constructor. The accessors also stop memoising values derived from the host context. This resolves [#858](https://github.com/dailydevops/http.correlation/issues/858) and [#859](https://github.com/dailydevops/http.correlation/issues/859).

## Context

### Observed behaviour

`WithHttpCorrelation()` registers `HttpCorrelationIdHandler` as a transient `DelegatingHandler`. Its constructor takes `IHttpCorrelationAccessor`, which both hosts register as scoped.

`IHttpClientFactory` does **not** resolve message handlers from the scope of the incoming request. `DefaultHttpClientFactory.CreateHandlerEntry` creates its own scope (`_scopeFactory.CreateScope()`) for every handler chain. It caches that chain for `HandlerLifetime` (2 minutes by default) and shares it across all requests in that window. Microsoft documents this as a known limitation. The guidance is "DO NOT cache any scope-related information … inside `HttpMessageHandler` instances or its dependencies" ([IHttpClientFactory troubleshooting](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory-troubleshooting#httpclient-doesnt-respect-scoped-lifetime), [dotnet/runtime#47091](https://github.com/dotnet/runtime/issues/47091)).

The handler therefore holds an accessor instance that:

1. belongs to the factory's handler scope, **not** to any request scope;
2. was never touched by `HttpCorrelationMiddleware` / `FunctionsCorrelationMiddleware`, so its `HeaderName` is never set to the incoming dialect;
3. memoises its first answer (`_correlationId ??= …`), so it keeps that answer for as long as the handler chain lives (#859).

The consequences depend on the host.

| Host | Consequence | Severity |
| --- | --- | --- |
| ASP.NET Core | Every outgoing call forwards the id of the request that happened to build the chain first. | Silent, plausible-looking wrong trace |
| ASP.NET Core | An incoming `X-Request-ID` is forwarded as `X-Correlation-ID`, starting with the **first** request. | Dialect lost |
| ASP.NET Core | Calls with no request in flight send an **empty** `X-Correlation-ID` header, or leak the id of an earlier request once one was memoised. | Wrong or empty header |
| Azure Functions | `FunctionsCorrelationAccessor.Context` is `null` in the handler scope. The first outgoing call throws `NullReferenceException` in `FunctionsCorrelationAccessor.get_CorrelationId()`. | `WithHttpCorrelation()` is unusable in Functions |

The Functions case is not described in #858. It shows that the fix proposed in the issue, which resolves through `IHttpContextAccessor`, is not enough. The isolated worker has no `HttpContext` unless ASP.NET Core integration is used. `AddHttpCorrelation()` for Functions does not register `IHttpContextAccessor` either.

### Reproduction (tests added on this branch)

All tests below go through the real `IHttpClientFactory` pipeline. They use `AddHttpClient(...).WithHttpCorrelation()` with a recording primary handler, and keep one host or provider alive across several requests, as in production. Each test is red on the current code and defines the target behaviour.

`tests/NetEvolve.Http.Correlation.AspNetCore.Tests.Integration/HttpClientForwardingTests.cs`

| Test | Current result |
| --- | --- |
| `SendAsync_SequentialRequests_ForwardEachRequestsOwnCorrelationId` | forwarded `[First, First]` |
| `Send_SequentialRequests_ForwardEachRequestsOwnCorrelationId` (sync path) | forwarded `[First, First]` |
| `SendAsync_ConcurrentRequests_ForwardEachRequestsOwnCorrelationId` (16 parallel) | forwarded the first request's id 16 times |
| `SendAsync_IncomingAlternativeHeader_ForwardsUsingAlternativeHeader` | incoming `X-Request-ID: Alternative`, forwarded as `X-Correlation-ID: Alternative` |
| `SendAsync_MixedHeaderDialects_ForwardEachRequestsOwnHeaderAndId` | `X-Correlation-ID: Primary` twice, no `X-Request-ID` |
| `SendAsync_OutsideOfRequest_AddsNoCorrelationHeader` | `X-Correlation-ID` with an empty value |
| `SendAsync_OutsideOfRequestAfterRequest_DoesNotLeakPreviousCorrelationId` | background call forwarded the earlier request's id |

`tests/NetEvolve.Http.Correlation.Azure.Functions.Tests.Integration/HttpClientForwardingTests.cs`

| Test | Current result |
| --- | --- |
| `SendAsync_SingleInvocation_ForwardsInvocationCorrelationId` | `NullReferenceException` |
| `SendAsync_SequentialInvocations_ForwardEachInvocationsOwnCorrelationId` | `NullReferenceException` |
| `SendAsync_IncomingAlternativeHeader_ForwardsUsingAlternativeHeader` | `NullReferenceException` |
| `SendAsync_OutsideOfInvocation_AddsNoCorrelationHeader` | `NullReferenceException` |

Accessor memoisation (#859):

| Test | Current result |
| --- | --- |
| `HttpCorrelationAccessorTests.CorrelationId_Get_AfterHttpContextChanged_ReturnsCurrentTraceIdentifier` | returns the first request's id |
| `HttpCorrelationAccessorTests.CorrelationId_Get_AfterHttpContextCleared_ReturnsNull` | returns the finished request's id |
| `HttpCorrelationAccessorTests.CorrelationId_Get_AfterTraceIdentifierChanged_ReturnsCurrentTraceIdentifier` | returns the server trace id cached before the middleware replaced it |
| `FunctionsCorrelationAccessorTests.CorrelationId_Get_AfterContextChanged_ReturnsCurrentInvocationId` | returns the first invocation's id |

`CorrelationId_Set_OverridesFallbackValue` (both accessors) is green and must stay green. An explicitly assigned id still wins over the host fallback.

### Other captures checked

`HttpCorrelationIdHandler` is the only type in this repository that receives a scoped service and is cached by `IHttpClientFactory`. Both middlewares resolve the accessor per invocation, from `HttpContext.RequestServices` and `FunctionContext.InstanceServices`. If a consumer sets `HttpClientFactoryOptions.SuppressHandlerScope = true`, the handler is resolved from the root provider instead. That is the same bug in a different form, and the proposed design is immune to it as well.

## Decision

### 1. Ambient, immutable correlation snapshot (fixes #858)

Add an internal ambient context to `NetEvolve.Http.Correlation.Abstractions`. `InternalsVisibleTo` already lists AspNetCore, Azure.Functions and HttpClient, so no public API is added.

```csharp
namespace NetEvolve.Http.Correlation;

internal sealed record CorrelationSnapshot(string CorrelationId, string HeaderName);

internal static class CorrelationContext
{
    private static readonly AsyncLocal<CorrelationSnapshot?> _current = new();

    public static CorrelationSnapshot? Current => _current.Value;

    internal static void Set(CorrelationSnapshot? snapshot) => _current.Value = snapshot;
}
```

Both middlewares publish the snapshot right before calling `next`. The snapshot holds the resolved id and the header dialect actually used:

```csharp
// HttpCorrelationMiddleware.InvokeAsync / FunctionsCorrelationMiddleware.Invoke
CorrelationContext.Set(new CorrelationSnapshot(correlationId, usedHeaderName));
await next(context).ConfigureAwait(false);
```

An `AsyncLocal` value assigned inside an `async` method is visible to everything that method awaits. The caller does not see it once the method returns. The value is therefore bound to the request, including work the request starts itself, and absent in hosted services or timer or queue triggers.

`HttpCorrelationIdHandler` no longer takes constructor dependencies. It reads the snapshot on each call:

```csharp
private static void SetCorrelationId(HttpHeaders headers)
{
    if (CorrelationContext.Current is not { } snapshot || string.IsNullOrWhiteSpace(snapshot.CorrelationId))
    {
        return; // no request in flight: stay inert, never send an empty header
    }

    if (!headers.Contains(snapshot.HeaderName))
    {
        headers.Add(snapshot.HeaderName, snapshot.CorrelationId);
    }
}
```

The handler stays `internal`, and the `WithHttpCorrelation()` registration does not change. The public surface is unchanged. Only the existing handler unit tests need adapting: they construct the handler with a fake accessor and would publish a snapshot instead.

This pattern follows ASP.NET Core's own `Microsoft.AspNetCore.HeaderPropagation`. Its `HeaderPropagationMiddleware` writes into an `AsyncLocal` (`HeaderPropagationValues`), which `HeaderPropagationMessageHandler` reads from inside the factory-cached chain.

### 2. No memoisation of host-derived values (fixes #859)

Keep a field **only** for an explicitly assigned value and read the fallback live:

```csharp
// HttpCorrelationAccessor
private string? _explicitCorrelationId;

public string CorrelationId
{
    get => _explicitCorrelationId ?? _httpContextAccessor.HttpContext?.TraceIdentifier!;
    set => _explicitCorrelationId = value;
}

// FunctionsCorrelationAccessor
public string CorrelationId
{
    get => _explicitCorrelationId ?? Context?.InvocationId!;
    set => _explicitCorrelationId = value;
}
```

This is option 1 from #859. Once the value is no longer memoised, an accessor held beyond its request answers `null` instead of another request's id. The failure then shows up where it happens.

## Consequences

**Benefits**

- Correct id **and** header dialect on every outgoing call, for ASP.NET Core and Azure Functions, sync and async.
- `WithHttpCorrelation()` works in Azure Functions for the first time. Today it throws.
- No empty `X-Correlation-ID` headers from background work.
- The handler has no scoped dependencies, so it is immune to `HandlerLifetime` and `SuppressHandlerScope`.
- No public API change. The fix can ship as a `fix:` release.

**Trade-offs and risks**

- Requests handled by something other than the two middlewares get no forwarding. This includes the case where `UseHttpCorrelation()` is not registered. Today they get a plausible but wrong id. This change is intended, but it should go in the release notes.
- Fire-and-forget work started during a request keeps the request's snapshot after the request ends, because `AsyncLocal` flows with the `ExecutionContext`. For correlation this is desirable, since the work was caused by that request. The snapshot holds only two immutable strings, so there is no risk of use-after-dispose, unlike flowing a scoped service or `HttpContext`.
- Open question for Azure Functions: non-HTTP triggers (timer, queue, …) currently return early from the middleware. They could publish `CorrelationSnapshot(InvocationId, HeaderName1)` so outgoing calls from those functions are correlated too. The proposal leaves them inert, which the test `SendAsync_OutsideOfInvocation_AddsNoCorrelationHeader` expects.
- The handler currently also writes the correlation header into the **downstream response**. The proposal keeps that behaviour. Whether it is useful at all should be decided separately.

## Alternatives Considered

### A. Resolve per call through `IHttpContextAccessor` (proposal in #858)

The handler depends on `IHttpContextAccessor` and resolves `IHttpCorrelationAccessor` from `HttpContext.RequestServices` inside `SendAsync`.

- ✅ Small change and correct for ASP.NET Core.
- ❌ Does not fix Azure Functions. The isolated worker has no `HttpContext`, and `IHttpContextAccessor` is not registered there. A constructor dependency would fail when the handler is built. An optional lookup would turn today's exception into a silently missing header.
- ❌ Ties the HttpClient package to ASP.NET Core semantics.

Rejected because it fixes only one of the two supported hosts.

### B. Ambient snapshot via `AsyncLocal` (chosen)

See [Decision](#decision).

### C. Use `System.Diagnostics.Activity` / W3C `traceparent` baggage

Read the id from `Activity.Current` (baggage or tags) instead of a dedicated context.

- ✅ Standard mechanism, also flows through other instrumentation.
- ❌ Changes semantics. `X-Correlation-ID` and `X-Request-ID` are independent of W3C trace ids, and consumers may run without any `ActivitySource` listener.
- ❌ Much larger behavioural change than the bug warrants.

Rejected for this fix. It could be revisited as a separate feature.

### D. Document only ("never hold an accessor beyond its request")

Rejected. The compiler cannot enforce it, and the package's own handler already breaks it.

## Related Decisions

- [Conventional Commits](./2025-07-10-conventional-commits.md): the fix should be released as `fix(httpclient): …` / `fix(aspnetcore): …` / `fix(functions): …`.
- [AI Agent Authorization for Build, Restore, and Test Operations](./2025-11-01-ai-agent-build-restore-test-permissions.md): the reproduction tests were run at solution level.
