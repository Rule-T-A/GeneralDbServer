namespace DataAbstractionAPI.Core.Tests.Enums;

using DataAbstractionAPI.Core.Enums;

public class FieldTypeTests
{
    [Fact]
    public void FieldType_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)FieldType.String);
        Assert.Equal(1, (int)FieldType.Integer);
        Assert.Equal(2, (int)FieldType.Float);
        Assert.Equal(3, (int)FieldType.Boolean);
        Assert.Equal(4, (int)FieldType.DateTime);
        Assert.Equal(5, (int)FieldType.Date);
        Assert.Equal(6, (int)FieldType.Array);
        Assert.Equal(7, (int)FieldType.Object);
    }

    [Fact]
    public void FieldType_ToString_ReturnsEnumName()
    {
        // Act
        var stringValue = FieldType.String.ToString();
        var integerValue = FieldType.Integer.ToString();

        // Assert
        Assert.Equal("String", stringValue);
        Assert.Equal("Integer", integerValue);
    }
}

