#if NET9_0_OR_GREATER
namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit.Generators;

using System;
using System.Threading.Tasks;
using NetEvolve.Http.Correlation.Generators;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public sealed class GuidV7CorrelationIdProviderTests
{
    [Test]
    public async Task GenerateId_NotEmpty()
    {
        // Arrange
        var provider = new GuidV7CorrelationIdProvider();
        // Act
        var id = provider.GenerateId();
        // Assert
        _ = await Assert.That(id).IsNotEqualTo($"{Guid.Empty:N}");
    }
}
#endif
