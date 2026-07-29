namespace NetEvolve.Http.Correlation.Ulid.ByteAether.Tests.Unit;

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
        _ = await Assert.That(values.Distinct(StringComparer.Ordinal).Count()).IsEqualTo(numberOfIds);
    }

    [Test]
    public async Task GenerateId_Sequential_Expected()
    {
        // Arrange
        const int numberOfIds = 10_000;
        var correlationIdProvider = new UlidCorrelationIdProvider();
        var values = new string[numberOfIds];

        // Act
        _ = Enumerable.Range(0, numberOfIds).Select(i => values[i] = correlationIdProvider.GenerateId()).ToList();

        // Assert
        foreach (var id in values.Zip(values.Skip(1), (a, b) => (a, b)))
        {
            _ = await Assert.That(id.a).IsLessThan(id.b);
        }
    }
}
