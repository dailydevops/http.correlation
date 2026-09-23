namespace NetEvolve.Http.Correlation.Azure.Functions;

using Microsoft.Azure.Functions.Worker;
using NetEvolve.Http.Correlation.Abstractions;

internal sealed class FunctionsCorrelationAccessor : IHttpCorrelationAccessor
{
    private string? _explicitCorrelationId;

    public FunctionContext? Context { get; set; }

    public string CorrelationId
    {
        // Read live on every access: an instance that outlives its invocation must not keep answering for it.
        get => _explicitCorrelationId ?? Context?.InvocationId!;
        set => _explicitCorrelationId = value;
    }

    public string HeaderName { get; set; } = string.Empty;
}
