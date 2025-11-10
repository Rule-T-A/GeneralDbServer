using DataAbstractionAPI.API.Mapping;
using DataAbstractionAPI.Core.Models;
using DataAbstractionAPI.Core.Enums;
using Xunit;
using Record = DataAbstractionAPI.Core.Models.Record;

namespace DataAbstractionAPI.API.Tests.Mapping;

public class MappingEdgeCaseTests
{
    // ============================================
    // Task 3.3.1: Mapping Edge Cases
    // ============================================

    [Fact]
    public void MappingExtensions_WithNullValues_HandlesGracefully()
    {
        // Arrange - Test Record with null Data
        var record = new Record
        {
            Id = "test-id",
            Data = null!
        };

        // Act - Record.ToDto() handles null Data gracefully (assigns null to DTO)
        var recordDto = record.ToDto();

        // Assert
        Assert.NotNull(recordDto);
        Assert.Equal("test-id", recordDto.Id);
        Assert.Null(recordDto.Data);

        // Arrange - Test ListResult with null Data list
        var listResult = new ListResult
        {
            Data = null!,
            Total = 0,
            More = false
        };

        // Act & Assert - Should throw ArgumentNullException from LINQ Select
        var exception = Assert.Throws<ArgumentNullException>(() => listResult.ToDto());
        Assert.NotNull(exception);

        // Arrange - Test CollectionSchema with null Fields
        var schema = new CollectionSchema
        {
            Name = "test",
            Fields = null!
        };

        // Act & Assert - Should throw ArgumentNullException from LINQ Select
        var exception2 = Assert.Throws<ArgumentNullException>(() => schema.ToDto());
        Assert.NotNull(exception2);
    }

    [Fact]
    public void MappingExtensions_WithEmptyCollections_HandlesGracefully()
    {
        // Arrange - Test ListResult with empty Data list
        var listResult = new ListResult
        {
            Data = new List<Record>(),
            Total = 0,
            More = false
        };

        // Act
        var dto = listResult.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.Data);
        Assert.Empty(dto.Data);
        Assert.Equal(0, dto.Total);
        Assert.False(dto.More);

        // Arrange - Test CollectionSchema with empty Fields list
        var schema = new CollectionSchema
        {
            Name = "test",
            Fields = new List<FieldDefinition>()
        };

        // Act
        var schemaDto = schema.ToDto();

        // Assert
        Assert.NotNull(schemaDto);
        Assert.NotNull(schemaDto.Fields);
        Assert.Empty(schemaDto.Fields);
        Assert.Equal("test", schemaDto.Name);

        // Arrange - Test BulkResult with empty Results list
        var bulkResult = new BulkResult
        {
            Success = true,
            Succeeded = 0,
            Failed = 0,
            Results = new List<BulkOperationItemResult>()
        };

        // Act
        var bulkDto = bulkResult.ToDto();

        // Assert
        Assert.NotNull(bulkDto);
        Assert.NotNull(bulkDto.Results);
        Assert.Empty(bulkDto.Results);
        Assert.True(bulkDto.Success);

        // Arrange - Test SummaryResult with empty Counts dictionary
        var summaryResult = new SummaryResult
        {
            Counts = new Dictionary<string, int>()
        };

        // Act
        var summaryDto = summaryResult.ToDto();

        // Assert
        Assert.NotNull(summaryDto);
        Assert.Empty(summaryDto);

        // Arrange - Test AggregateResult with empty Data list
        var aggregateResult = new AggregateResult
        {
            Data = new List<Dictionary<string, object>>()
        };

        // Act
        var aggregateDto = aggregateResult.ToDto();

        // Assert
        Assert.NotNull(aggregateDto);
        Assert.NotNull(aggregateDto.Data);
        Assert.Empty(aggregateDto.Data);
    }

    [Fact]
    public void MappingExtensions_WithMissingRequiredFields_HandlesGracefully()
    {
        // Arrange - Test Record with empty Id
        var record = new Record
        {
            Id = string.Empty,
            Data = new Dictionary<string, object> { { "name", "Test" } }
        };

        // Act
        var dto = record.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(string.Empty, dto.Id);
        Assert.NotNull(dto.Data);
        Assert.Equal("Test", dto.Data["name"]);

        // Arrange - Test CollectionSchema with empty Name
        var schema = new CollectionSchema
        {
            Name = string.Empty,
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "field1", Type = FieldType.String }
            }
        };

        // Act
        var schemaDto = schema.ToDto();

        // Assert
        Assert.NotNull(schemaDto);
        Assert.Equal(string.Empty, schemaDto.Name);
        Assert.Single(schemaDto.Fields);

        // Arrange - Test FieldDefinition with empty Name
        var field = new FieldDefinition
        {
            Name = string.Empty,
            Type = FieldType.String,
            Nullable = true
        };

        // Act
        var fieldDto = field.ToDto();

        // Assert
        Assert.NotNull(fieldDto);
        Assert.Equal(string.Empty, fieldDto.Name);
        Assert.Equal("String", fieldDto.Type); // FieldType is serialized as string
    }

    [Fact]
    public void MappingExtensions_WithTypeConversionErrors_HandlesGracefully()
    {
        // Arrange - Test Record with various data types (should all convert to object)
        var record = new Record
        {
            Id = "test-id",
            Data = new Dictionary<string, object>
            {
                { "string", "text" },
                { "integer", 123 },
                { "float", 45.67 },
                { "boolean", true },
                { "null", null! },
                { "array", new[] { 1, 2, 3 } },
                { "object", new { key = "value" } }
            }
        };

        // Act
        var dto = record.ToDto();

        // Assert - All types should be preserved as objects
        Assert.NotNull(dto);
        Assert.Equal("text", dto.Data["string"]);
        Assert.Equal(123, dto.Data["integer"]);
        Assert.Equal(45.67, dto.Data["float"]);
        Assert.Equal(true, dto.Data["boolean"]);
        Assert.Null(dto.Data["null"]);
        Assert.NotNull(dto.Data["array"]);
        Assert.NotNull(dto.Data["object"]);

        // Arrange - Test AggregateResult with various data types
        var aggregateResult = new AggregateResult
        {
            Data = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "group", "A" },
                    { "count", 10 },
                    { "sum", 100.5 },
                    { "avg", 10.05 }
                }
            }
        };

        // Act
        var aggregateDto = aggregateResult.ToDto();

        // Assert
        Assert.NotNull(aggregateDto);
        Assert.Single(aggregateDto.Data);
        Assert.Equal("A", aggregateDto.Data[0]["group"]);
        Assert.Equal(10, aggregateDto.Data[0]["count"]);
    }

    [Fact]
    public void MappingExtensions_Record_WithNullData_HandlesGracefully()
    {
        // Arrange
        var record = new Record
        {
            Id = "test",
            Data = null!
        };

        // Act - Record.ToDto() handles null Data gracefully
        var dto = record.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("test", dto.Id);
        Assert.Null(dto.Data);
    }

    [Fact]
    public void MappingExtensions_ListResult_WithNullData_ThrowsException()
    {
        // Arrange
        var listResult = new ListResult
        {
            Data = null!,
            Total = 0,
            More = false
        };

        // Act & Assert - LINQ Select throws ArgumentNullException
        var exception = Assert.Throws<ArgumentNullException>(() => listResult.ToDto());
        Assert.NotNull(exception);
        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void MappingExtensions_CollectionSchema_WithNullFields_ThrowsException()
    {
        // Arrange
        var schema = new CollectionSchema
        {
            Name = "test",
            Fields = null!
        };

        // Act & Assert - LINQ Select throws ArgumentNullException
        var exception = Assert.Throws<ArgumentNullException>(() => schema.ToDto());
        Assert.NotNull(exception);
        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void MappingExtensions_BulkResult_WithNullResults_HandlesGracefully()
    {
        // Arrange
        var bulkResult = new BulkResult
        {
            Success = true,
            Succeeded = 0,
            Failed = 0,
            Results = null!
        };

        // Act
        var dto = bulkResult.ToDto();

        // Assert - Null Results should be handled (nullable property)
        Assert.NotNull(dto);
        Assert.Null(dto.Results);
        Assert.True(dto.Success);
    }

    [Fact]
    public void MappingExtensions_BulkResult_WithNullIds_HandlesGracefully()
    {
        // Arrange
        var bulkResult = new BulkResult
        {
            Success = true,
            Succeeded = 0,
            Failed = 0,
            Results = new List<BulkOperationItemResult>(),
            Ids = null!
        };

        // Act
        var dto = bulkResult.ToDto();

        // Assert - Null Ids should be handled (nullable property)
        Assert.NotNull(dto);
        Assert.Null(dto.Ids);
    }

    [Fact]
    public void MappingExtensions_BulkOperationItemResult_WithNullId_HandlesGracefully()
    {
        // Arrange
        var item = new BulkOperationItemResult
        {
            Index = 0,
            Id = null!,
            Success = false,
            Error = "Test error"
        };

        // Act
        var dto = item.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.Id);
        Assert.Equal(0, dto.Index);
        Assert.False(dto.Success);
        Assert.Equal("Test error", dto.Error);
    }

    [Fact]
    public void MappingExtensions_BulkOperationItemResult_WithNullError_HandlesGracefully()
    {
        // Arrange
        var item = new BulkOperationItemResult
        {
            Index = 0,
            Id = "test-id",
            Success = true,
            Error = null!
        };

        // Act
        var dto = item.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("test-id", dto.Id);
        Assert.True(dto.Success);
        Assert.Null(dto.Error);
    }

    [Fact]
    public void MappingExtensions_SummaryResult_WithNullCounts_ThrowsException()
    {
        // Arrange
        var summaryResult = new SummaryResult
        {
            Counts = null!
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => summaryResult.ToDto());
    }

    [Fact]
    public void MappingExtensions_AggregateResult_WithNullData_HandlesGracefully()
    {
        // Arrange
        var aggregateResult = new AggregateResult
        {
            Data = null!
        };

        // Act - AggregateResult.ToDto() handles null Data gracefully (assigns null to DTO)
        var dto = aggregateResult.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.Data);
    }
}

