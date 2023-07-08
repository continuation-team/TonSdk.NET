using NUnit.Framework;

namespace TonSdk.Contracts.Tests;

public class SmcUtilsTest {
    [Test]
    public void GenerateQueryId_WithoutRandomId_GeneratesExpectedId() {
        // Arrange
        int timeout = 5;

        // Act
        ulong result = SmcUtils.GenerateQueryId(timeout);

        // Assert
        // Here we are checking that the result is not equal to zero, as we expect the function to return some value
        Assert.That(result, Is.Not.EqualTo(0));
    }

    [Test]
    public void GenerateQueryId_WithRandomId_GeneratesExpectedId() {
        // Arrange
        int timeout = 5;
        int randomId = 123;

        // Act
        ulong result = SmcUtils.GenerateQueryId(timeout, randomId);

        // Assert
        // Check that the last 32 bits of the result match the provided randomId
        Assert.That((uint)result, Is.EqualTo(randomId));
    }

    [Test]
    public void GenerateQueryId_GenerateManyIds_AtLeast99PercentAreUnique() {
        // Arrange
        var ids = new HashSet<ulong>();
        int timeout = 5;
        int totalGeneratedIds = 100000;

        // Act
        for (int i = 0; i < totalGeneratedIds; i++) {
            // Generating many IDs
            ulong newId = SmcUtils.GenerateQueryId(timeout);
            ids.Add(newId);
        }

        // Assert
        // Checking that at least 99% IDs are unique
        double percentageUnique = (double)ids.Count / totalGeneratedIds;
        Assert.That(percentageUnique, Is.GreaterThanOrEqualTo(0.99));
    }
}
