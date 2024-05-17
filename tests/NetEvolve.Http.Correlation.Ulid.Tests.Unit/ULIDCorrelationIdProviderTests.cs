namespace NetEvolve.Http.Correlation.Ulid.Tests.Unit;

using System.Linq;
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
        const int numberOfIds = 1000;
        var correlationIdProvider = new UlidCorrelationIdProvider();
        var values = new string[numberOfIds];

        // Act
        foreach (var i in Enumerable.Range(0, numberOfIds))
        {
            values[i] = correlationIdProvider.GenerateId();
        }

        // Assert
        Assert.Equal(numberOfIds, values.Distinct().Count());
    }
}
