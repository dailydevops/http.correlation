namespace NetEvolve.Http.Correlation.Abstractions.Tests.Unit.Generators;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetEvolve.Http.Correlation.Generators;
using NetEvolve.SequentialGuid;
using NSubstitute;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public sealed class SequentialGuidCorrelationIdProviderTests
{
    [Test]
    [Arguments(SequentialGuidType.AsBinary)]
    [Arguments(SequentialGuidType.AsString)]
    [Arguments(SequentialGuidType.AtEnd)]
    public async Task GenerateId_NotEmpty(SequentialGuidType sequentialType)
    {
        // Arrange
        var optionsMonitorMock = Substitute.For<IOptionsMonitor<SequentialGuidOptions>>();
        _ = optionsMonitorMock.CurrentValue.Returns(new SequentialGuidOptions { SequentialType = sequentialType });
        var provider = new SequentialGuidCorrelationIdProvider(optionsMonitorMock);

        // Act
        var id = provider.GenerateId();

        // Assert
        _ = await Assert.That(id).IsNotEqualTo($"{Guid.Empty:N}");
    }
}
