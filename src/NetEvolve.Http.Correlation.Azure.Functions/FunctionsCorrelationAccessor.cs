namespace NetEvolve.Http.Correlation.Azure.Functions;

using Microsoft.Azure.Functions.Worker;
using NetEvolve.Http.Correlation.Abstractions;

internal sealed class FunctionsCorrelationAccessor : IHttpCorrelationAccessor
{
    private string? _correlationId;

    public FunctionContext Context { get; set; } = default!;

    public string CorrelationId
    {
        get => _correlationId ??= Context.InvocationId;
        set => _correlationId = value;
    }

    public string HeaderName { get; set; } = string.Empty;
}
