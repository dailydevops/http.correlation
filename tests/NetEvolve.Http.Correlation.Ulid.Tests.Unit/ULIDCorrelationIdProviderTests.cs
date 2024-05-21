namespace NetEvolve.Http.Correlation.Ulid.Tests.Unit;

using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class ULIDCorrelationIdProviderTests
{
    [Fact]
    public void GenerateId_Fact_Expected()
    {
        // Arrange
        var correlationIdProvider = new UlidCorrelationIdProvider();

        // Act
        var result = correlationIdProvider.GenerateId();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GenerateId_UniqueIds_Expected()
    {
        // Arrange
        const int numberOfIds = 10_000;
        var correlationIdProvider = new UlidCorrelationIdProvider();
        var values = new string[numberOfIds];

        // Act
        _ = Parallel.For(0, numberOfIds, i => values[i] = correlationIdProvider.GenerateId());

        // Assert
        Assert.Equal(numberOfIds, values.Distinct().Count());
    }
}
