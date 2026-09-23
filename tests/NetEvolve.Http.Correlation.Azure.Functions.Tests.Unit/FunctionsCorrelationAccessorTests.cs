namespace NetEvolve.Http.Correlation.Azure.Functions.Tests.Unit;

using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using TUnit.Assertions.Extensions;
using TUnit.Core;
using TUnit.Mocks;

public class FunctionsCorrelationAccessorTests
{
    [Test]
    public async Task CorrelationId_Get_WithoutExplicitValue_FallsBackToInvocationId()
    {
        // Arrange
        var context = FunctionContext.Mock();
        _ = context.InvocationId.Returns("invocation-id");
        var accessor = new FunctionsCorrelationAccessor { Context = context.Object };

        // Act
        var result = accessor.CorrelationId;

        // Assert
        _ = await Assert.That(result).IsEqualTo("invocation-id");
    }

    [Test]
    public async Task CorrelationId_Set_OverridesFallbackValue()
    {
        // Arrange
        var context = FunctionContext.Mock();
        _ = context.InvocationId.Returns("invocation-id");

        // Act
        var accessor = new FunctionsCorrelationAccessor { Context = context.Object, CorrelationId = "explicit-id" };

        // Assert
        _ = await Assert.That(accessor.CorrelationId).IsEqualTo("explicit-id");
    }

    [Test]
    public async Task CorrelationId_Get_AfterContextChanged_ReturnsCurrentInvocationId()
    {
        // Arrange — one instance outlives its invocation (#859)
        var first = FunctionContext.Mock();
        _ = first.InvocationId.Returns("first-invocation");
        var second = FunctionContext.Mock();
        _ = second.InvocationId.Returns("second-invocation");

        var accessor = new FunctionsCorrelationAccessor { Context = first.Object };
        _ = accessor.CorrelationId;

        // Act
        accessor.Context = second.Object;
        var result = accessor.CorrelationId;

        // Assert
        _ = await Assert.That(result).IsEqualTo("second-invocation");
    }
}
