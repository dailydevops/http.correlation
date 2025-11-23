namespace NetEvolve.Http.Correlation.TestGenerator.Tests.Unit;

using System.Collections.Generic;
using System.Threading.Tasks;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class TestGeneratorCorrelationIdProviderTests
{
    [Test]
    [MethodDataSource(nameof(GenerateIdData))]
    public async Task GenerateId_Theory_Expected(string expected, string? value)
    {
        // Arrange
        var correlationIdProvider = new TestGeneratorCorrelationIdProvider(value);

        // Act
        var result = correlationIdProvider.GenerateId();

        // Assert
        _ = await Assert.That(result).IsEqualTo(expected);
    }

    public static IEnumerable<(string expected, string? value)> GenerateIdData()
    {
        yield return ("Generated_Test_Id", null);
        yield return ("Generated_Test_Id", string.Empty);
        yield return ("Generated_Test_Id", " ");
        yield return ("HelloWorldID", "HelloWorldID");
    }
}
