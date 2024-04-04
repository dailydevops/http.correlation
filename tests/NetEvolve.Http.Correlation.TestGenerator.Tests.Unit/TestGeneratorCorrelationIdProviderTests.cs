namespace NetEvolve.Http.Correlation.TestGenerator.Tests.Unit;

using Xunit;

public class TestGeneratorCorrelationIdProviderTests
{
    [Theory]
    [MemberData(nameof(GenerateIdData))]
    public void GenerateId_Theory_Expected(string expected, string? value)
    {
        // Arrange
        var correlationIdProvider = new TestGeneratorCorrelationIdProvider(value);

        // Act
        var result = correlationIdProvider.GenerateId();

        // Assert
        Assert.Equal(expected, result);
    }

    public static TheoryData<string, string?> GenerateIdData =>
        new TheoryData<string, string?>
        {
            { "Generated_Test_Id", null },
            { "Generated_Test_Id", string.Empty },
            { "Generated_Test_Id", " " },
            { "HelloWorldID", "HelloWorldID" }
        };
}
