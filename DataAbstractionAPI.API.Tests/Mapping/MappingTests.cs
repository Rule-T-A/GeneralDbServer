using System.Text.Json;
using DataAbstractionAPI.API.Mapping;
using DataAbstractionAPI.API.Models.DTOs;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using Xunit;
using Record = DataAbstractionAPI.Core.Models.Record;

namespace DataAbstractionAPI.API.Tests.Mapping;

/// <summary>
/// Tests for mapping Core models to DTOs.
/// </summary>
public class MappingTests
{
    [Fact]
    public void Record_ToDto_MapsCorrectly()
    {
        // Arrange
        var record = new Record
        {
            Id = "test-id-123",
            Data = new Dictionary<string, object>
            {
                { "name", "Test User" },
                { "age", 30 }
            }
        };

        // Act
        var dto = record.ToDto();

        // Assert
        Assert.Equal("test-id-123", dto.Id);
        Assert.Equal(2, dto.Data.Count);
        Assert.Equal("Test User", dto.Data["name"]);
        Assert.Equal(30, dto.Data["age"]);
    }

    [Fact]
    public void ListResult_ToDto_MapsCorrectly()
    {
        // Arrange
        var listResult = new ListResult
        {
            Data = new List<Record>
            {
                new Record { Id = "1", Data = new Dictionary<string, object> { { "name", "Alice" } } },
                new Record { Id = "2", Data = new Dictionary<string, object> { { "name", "Bob" } } }
            },
            Total = 2,
            More = false
        };

        // Act
        var dto = listResult.ToDto();

        // Assert
        Assert.Equal(2, dto.Data.Count);
        Assert.Equal(2, dto.Total);
        Assert.False(dto.More);
        Assert.Equal("1", dto.Data[0].Id);
        Assert.Equal("2", dto.Data[1].Id);
    }

    [Fact]
    public void CreateResult_ToDto_MapsCorrectly()
    {
        // Arrange
        var createResult = new CreateResult
        {
            Id = "new-id-456",
            Record = new Record
            {
                Id = "new-id-456",
                Data = new Dictionary<string, object> { { "name", "New User" } }
            }
        };

        // Act
        var dto = createResult.ToDto();

        // Assert
        Assert.Equal("new-id-456", dto.Id);
        Assert.NotNull(dto.Record);
        Assert.Equal("new-id-456", dto.Record.Id);
    }

    [Fact]
    public void FieldDefinition_ToDto_MapsCorrectly()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "email",
            Type = FieldType.String,
            Nullable = false,
            Default = "user@example.com"
        };

        // Act
        var dto = field.ToDto();

        // Assert
        Assert.Equal("email", dto.Name);
        Assert.Equal("String", dto.Type); // Enum serialized as string
        Assert.False(dto.Nullable);
        Assert.Equal("user@example.com", dto.Default);
    }

    [Fact]
    public void CollectionSchema_ToDto_MapsCorrectly()
    {
        // Arrange
        var schema = new CollectionSchema
        {
            Name = "users",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "id", Type = FieldType.String },
                new FieldDefinition { Name = "name", Type = FieldType.String }
            }
        };

        // Act
        var dto = schema.ToDto();

        // Assert
        Assert.Equal("users", dto.Name);
        Assert.Equal(2, dto.Fields.Count);
        Assert.Equal("id", dto.Fields[0].Name);
        Assert.Equal("name", dto.Fields[1].Name);
    }

    [Fact]
    public void ListResponseDto_SerializesWithCorrectPropertyNames()
    {
        // Arrange
        var dto = new ListResponseDto
        {
            Data = new List<RecordDto>
            {
                new RecordDto { Id = "1", Data = new Dictionary<string, object> { { "name", "Test" } } }
            },
            Total = 1,
            More = false
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // Use [JsonPropertyName] attributes
        });

        // Assert
        Assert.Contains("\"d\":", json); // Compact key for data
        Assert.Contains("\"t\":", json); // Compact key for total
        Assert.Contains("\"more\":", json);
    }

    [Fact]
    public void RecordDto_SerializesWithCorrectPropertyNames()
    {
        // Arrange
        var dto = new RecordDto
        {
            Id = "test-id",
            Data = new Dictionary<string, object> { { "name", "Test" } }
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });

        // Assert
        Assert.Contains("\"id\":", json);
        Assert.Contains("\"d\":", json); // Compact key for data
    }

    [Fact]
    public void FieldDefinitionDto_SerializesTypeAsString()
    {
        // Arrange
        var dto = new FieldDefinitionDto
        {
            Name = "age",
            Type = "Integer", // Already a string
            Nullable = true,
            Default = 0
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });

        // Assert
        Assert.Contains("\"type\":\"Integer\"", json);
        Assert.Contains("\"name\":\"age\"", json);
        Assert.Contains("\"nullable\":true", json);
    }
}

