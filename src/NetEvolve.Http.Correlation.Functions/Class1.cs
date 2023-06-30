namespace NetEvolve.Http.Correlation.Functions;

using Microsoft.Azure.WebJobs.Host;
using System.Threading;
using System.Threading.Tasks;

public class Class1 : IFunctionInvocationFilter
{
    /// <inheritdoc />
    public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken) => throw new System.NotImplementedException();

    /// <inheritdoc />
    public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken) => Task.CompletedTask;
}
