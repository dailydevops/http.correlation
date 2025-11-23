namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit.Generators;

using System;
using System.Threading.Tasks;
using NetEvolve.Http.Correlation.Generators;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class GuidCorrelationIdProviderTests
{
    [Test]
    public async Task GenerateId_NotEmpty()
    {
        // Arrange
        var provider = new GuidCorrelationIdProvider();

        // Act
        var id = provider.GenerateId();

        // Assert
        _ = await Assert.That(id).IsNotEqualTo($"{Guid.Empty:N}");
    }
}
