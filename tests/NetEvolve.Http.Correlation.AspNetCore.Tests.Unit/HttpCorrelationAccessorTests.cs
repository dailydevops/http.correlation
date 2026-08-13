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
}
