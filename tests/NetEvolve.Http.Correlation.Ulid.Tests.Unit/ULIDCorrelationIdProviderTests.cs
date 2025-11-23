namespace NetEvolve.Http.Correlation.Ulid.Tests.Unit;

using System.Linq;
using System.Threading.Tasks;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class ULIDCorrelationIdProviderTests
{
    [Test]
    public async Task GenerateId_Fact_Expected()
    {
        // Arrange
        var correlationIdProvider = new UlidCorrelationIdProvider();

        // Act
        var result = correlationIdProvider.GenerateId();

        // Assert
        _ = await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task GenerateId_UniqueIds_Expected()
    {
        // Arrange
        const int numberOfIds = 10_000;
        var correlationIdProvider = new UlidCorrelationIdProvider();
        var values = new string[numberOfIds];

        // Act
        _ = Parallel.For(0, numberOfIds, i => values[i] = correlationIdProvider.GenerateId());

        // Assert
        _ = await Assert.That(values.Distinct().Count()).IsEqualTo(numberOfIds);
    }
}
