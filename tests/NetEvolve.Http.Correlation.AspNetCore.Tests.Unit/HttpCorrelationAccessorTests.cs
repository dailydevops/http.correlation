namespace NetEvolve.Http.Correlation.AspNetCore.Tests.Unit;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetEvolve.Http.Correlation.AspNetCore;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class HttpCorrelationAccessorTests
{
    [Test]
    public async Task CorrelationId_Get_WithoutExplicitValue_FallsBackToTraceIdentifier()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-id" },
        };
        var accessor = new HttpCorrelationAccessor(httpContextAccessor);

        // Act
        var result = accessor.CorrelationId;

        // Assert
        _ = await Assert.That(result).IsEqualTo("trace-id");
    }

    [Test]
    public async Task CorrelationId_Set_OverridesFallbackValue()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-id" },
        };
        // Act
        var accessor = new HttpCorrelationAccessor(httpContextAccessor) { CorrelationId = "explicit-id" };

        // Assert
        _ = await Assert.That(accessor.CorrelationId).IsEqualTo("explicit-id");
    }

    [Test]
    public async Task CorrelationId_Get_WithoutHttpContext_ReturnsNull()
    {
        // Arrange — no HttpContext available (e.g. accessed outside a request scope)
        var httpContextAccessor = new HttpContextAccessor();
        var accessor = new HttpCorrelationAccessor(httpContextAccessor);

        // Act
        var result = accessor.CorrelationId;

        // Assert
        _ = await Assert.That(result).IsNull();
    }

    [Test]
    public async Task CorrelationId_Get_AfterHttpContextChanged_ReturnsCurrentTraceIdentifier()
    {
        // Arrange — one instance outlives its request (#859), e.g. captured by a cached handler chain (#858)
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "first-request" },
        };
        var accessor = new HttpCorrelationAccessor(httpContextAccessor);
        _ = accessor.CorrelationId;

        // Act
        httpContextAccessor.HttpContext = new DefaultHttpContext { TraceIdentifier = "second-request" };
        var result = accessor.CorrelationId;

        // Assert
        _ = await Assert.That(result).IsEqualTo("second-request");
    }

    [Test]
    public async Task CorrelationId_Get_AfterHttpContextCleared_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "finished-request" },
        };
        var accessor = new HttpCorrelationAccessor(httpContextAccessor);
        _ = accessor.CorrelationId;

        // Act — the request is over, no HttpContext is in flight any more
        httpContextAccessor.HttpContext = null;
        var result = accessor.CorrelationId;

        // Assert
        _ = await Assert.That(result).IsNull();
    }

    [Test]
    public async Task CorrelationId_Get_AfterTraceIdentifierChanged_ReturnsCurrentTraceIdentifier()
    {
        // Arrange — read before HttpCorrelationMiddleware replaced the TraceIdentifier with the correlation id
        var httpContext = new DefaultHttpContext { TraceIdentifier = "server-trace-id" };
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var accessor = new HttpCorrelationAccessor(httpContextAccessor);
        _ = accessor.CorrelationId;

        // Act
        httpContext.TraceIdentifier = "incoming-correlation-id";
        var result = accessor.CorrelationId;

        // Assert
        _ = await Assert.That(result).IsEqualTo("incoming-correlation-id");
    }
}
