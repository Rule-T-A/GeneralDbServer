namespace DataAbstractionAPI.Core.Tests.Enums;

using DataAbstractionAPI.Core.Enums;

public class StorageTypeTests
{
    [Fact]
    public void StorageType_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)StorageType.Csv);
        Assert.Equal(1, (int)StorageType.Sql);
        Assert.Equal(2, (int)StorageType.NoSql);
        Assert.Equal(3, (int)StorageType.InMemory);
    }

    [Fact]
    public void StorageType_ToString_ReturnsEnumName()
    {
        // Arrange
        var csvType = StorageType.Csv;
        var sqlType = StorageType.Sql;

        // Act
        var csvString = csvType.ToString();
        var sqlString = sqlType.ToString();

        // Assert
        Assert.Equal("Csv", csvString);
        Assert.Equal("Sql", sqlString);
    }

    [Fact]
    public void StorageType_CanBeParsed_FromString()
    {
        // Arrange
        var csvString = "Csv";
        var inMemoryString = "InMemory";

        // Act
        var csvParsed = Enum.Parse<StorageType>(csvString);
        var inMemoryParsed = Enum.Parse<StorageType>(inMemoryString);

        // Assert
        Assert.Equal(StorageType.Csv, csvParsed);
        Assert.Equal(StorageType.InMemory, inMemoryParsed);
    }
}

